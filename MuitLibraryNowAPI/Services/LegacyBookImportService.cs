using System.Globalization;
using Microsoft.VisualBasic.FileIO;
using MuitLibraryNowAPI.Models;
using MuitLibraryNowAPI.Repositories;

namespace MuitLibraryNowAPI.Services
{
    public class LegacyBookImportService : IBookImportService
    {
        private static readonly string[] TitleColumns = ["title", "book_title", "book_name", "name"];
        private static readonly string[] AuthorColumns = ["author", "writer", "book_author", "author_name"];
        private static readonly string[] GenreColumns = ["genre", "category", "section", "shelf_category", "book_type"];
        private static readonly string[] YearColumns = ["publishedyear", "published_year", "year_published", "publication_year", "year", "year_pub"];
        private static readonly string[] AvailabilityColumns = ["available", "availability", "status", "circulation_status", "is_available"];

        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;
        private readonly IBookRepository _bookRepository;
        private readonly ILogger<LegacyBookImportService> _logger;

        public LegacyBookImportService(
            IConfiguration configuration,
            IWebHostEnvironment environment,
            IBookRepository bookRepository,
            ILogger<LegacyBookImportService> logger)
        {
            _configuration = configuration;
            _environment = environment;
            _bookRepository = bookRepository;
            _logger = logger;
        }

        public BookImportSummary Import(string? filePath = null)
        {
            var resolvedPath = ResolvePath(filePath);
            if (!File.Exists(resolvedPath))
            {
                throw new FileNotFoundException("The legacy CSV file could not be found.", resolvedPath);
            }

            var extractedRows = ExtractRows(resolvedPath);
            var transformedBooks = new List<Book>();
            var warnings = new List<string>();

            foreach (var row in extractedRows)
            {
                if (TryTransform(row.Values, out var transformedBook, out var warning))
                {
                    transformedBooks.Add(transformedBook!);
                    continue;
                }

                warnings.Add($"Line {row.LineNumber}: {warning}");
            }

            foreach (var transformedBook in transformedBooks)
            {
                _bookRepository.Add(transformedBook);
            }

            var summary = new BookImportSummary
            {
                SourceFile = resolvedPath,
                ExtractedRows = extractedRows.Count,
                LoadedRows = transformedBooks.Count,
                SkippedRows = extractedRows.Count - transformedBooks.Count,
                ImportedAtUtc = DateTime.UtcNow,
                Warnings = warnings
            };

            _logger.LogInformation(
                "Legacy ETL complete. Extracted {ExtractedRows}, loaded {LoadedRows}, skipped {SkippedRows}.",
                summary.ExtractedRows,
                summary.LoadedRows,
                summary.SkippedRows);

            return summary;
        }

        private string ResolvePath(string? filePath)
        {
            var configuredPath = string.IsNullOrWhiteSpace(filePath)
                ? _configuration["ETL:LegacyBooksCsvPath"]
                : filePath;

            if (string.IsNullOrWhiteSpace(configuredPath))
            {
                throw new InvalidDataException("No legacy CSV path was configured for the ETL process.");
            }

            return Path.IsPathRooted(configuredPath)
                ? configuredPath
                : Path.GetFullPath(configuredPath, _environment.ContentRootPath);
        }

        private static List<ExtractedRow> ExtractRows(string resolvedPath)
        {
            using var parser = new TextFieldParser(resolvedPath);
            parser.TextFieldType = FieldType.Delimited;
            parser.SetDelimiters(",");
            parser.HasFieldsEnclosedInQuotes = true;
            parser.TrimWhiteSpace = true;

            if (parser.EndOfData)
            {
                throw new InvalidDataException("The legacy CSV file is empty.");
            }

            var headers = parser.ReadFields();
            if (headers == null || headers.Length == 0)
            {
                throw new InvalidDataException("The legacy CSV file does not contain a header row.");
            }

            var normalizedHeaders = headers
                .Select(header => header.Trim())
                .ToArray();

            var rows = new List<ExtractedRow>();

            while (!parser.EndOfData)
            {
                string[]? fields;
                try
                {
                    fields = parser.ReadFields();
                }
                catch (MalformedLineException exception)
                {
                    throw new InvalidDataException(
                        $"The legacy CSV file contains a malformed line near row {parser.LineNumber}.",
                        exception);
                }

                if (fields == null || fields.All(string.IsNullOrWhiteSpace))
                {
                    continue;
                }

                var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                for (var index = 0; index < normalizedHeaders.Length; index++)
                {
                    var value = index < fields.Length ? fields[index].Trim() : string.Empty;
                    values[normalizedHeaders[index]] = value;
                }

                rows.Add(new ExtractedRow(parser.LineNumber, values));
            }

            return rows;
        }

        private static bool TryTransform(
            IReadOnlyDictionary<string, string> values,
            out Book? transformedBook,
            out string warning)
        {
            transformedBook = null;

            var title = GetFirstValue(values, TitleColumns);
            if (string.IsNullOrWhiteSpace(title))
            {
                warning = "missing title";
                return false;
            }

            title = ToTitleCase(title);

            var author = GetFirstValue(values, AuthorColumns);
            if (string.IsNullOrWhiteSpace(author))
            {
                author = string.Empty;
            }
            else
            {
                author = ToTitleCase(author);
            }

            var genre = GetFirstValue(values, GenreColumns);
            if (string.IsNullOrWhiteSpace(genre))
            {
                genre = "General";
            }
            else
            {
                genre = ToTitleCase(genre);
            }

            var yearText = GetFirstValue(values, YearColumns);
            if (!int.TryParse(yearText, out var publishedYear))
            {
                warning = $"invalid published year '{yearText}'";
                return false;
            }

            var availabilityText = GetFirstValue(values, AvailabilityColumns);
            var available = ParseAvailability(availabilityText);

            transformedBook = new Book
            {
                Title = title,
                Author = author,
                Genre = genre,
                Available = available,
                PublishedYear = publishedYear
            };

            warning = string.Empty;
            return true;
        }

        private static string ToTitleCase(string value)
        {
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value.Trim().ToLowerInvariant());
        }

        private static string? GetFirstValue(IReadOnlyDictionary<string, string> values, IEnumerable<string> aliases)
        {
            foreach (var alias in aliases)
            {
                if (values.TryGetValue(alias, out var value) && !string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }

            return null;
        }

        private static bool ParseAvailability(string? availabilityText)
        {
            if (string.IsNullOrWhiteSpace(availabilityText))
            {
                return true;
            }

            return availabilityText.Trim().ToLowerInvariant() switch
            {
                "available" => true,
                "yes" => true,
                "true" => true,
                "1" => true,
                "on shelf" => true,
                "checked_out" => false,
                "checked out" => false,
                "borrowed" => false,
                "loaned" => false,
                "no" => false,
                "false" => false,
                "0" => false,
                _ => true
            };
        }

        private sealed record ExtractedRow(long LineNumber, IReadOnlyDictionary<string, string> Values);
    }
}
