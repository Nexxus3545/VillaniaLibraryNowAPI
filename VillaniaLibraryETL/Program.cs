using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json;

var legacyBooksCsvPath = Path.Combine(AppContext.BaseDirectory, "legacy_books.csv");
var lines = File.ReadAllLines(legacyBooksCsvPath);
var rawRecords = lines.Skip(1)
    .Where(line => !string.IsNullOrWhiteSpace(line))
    .Select(line => line.Split(','))
    .ToList();

Console.WriteLine($"Extracted {rawRecords.Count} records");

var transformed = rawRecords.Select(record => new TransformedBook
{
    Id = int.Parse(record[0].Trim()),
    Title = ToTitleCase(record[1]),
    Author = ToTitleCase(record[2]),
    Genre = string.IsNullOrWhiteSpace(record[3]) ? "General" : ToTitleCase(record[3]),
    Available = record[4].Trim().Equals("YES", StringComparison.OrdinalIgnoreCase),
    PublishedYear = int.Parse(record[5].Trim())
}).ToList();

foreach (var book in transformed)
{
    Console.WriteLine($"{book.Id} | {book.Title} | {book.Author} | {book.Genre} | {book.Available} | {book.PublishedYear}");
}

using var client = new HttpClient();
client.DefaultRequestVersion = new Version(1, 1);
client.Timeout = TimeSpan.FromSeconds(5);
client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("API_BASE_URL") ?? "http://localhost:8080/");

Console.WriteLine($"Waiting for API at {client.BaseAddress}");
await WaitForApiAsync(client, TimeSpan.FromSeconds(30));

foreach (var book in transformed)
{
    var json = JsonSerializer.Serialize(book);
    var content = new StringContent(json, Encoding.UTF8, "application/json");
    var response = await client.PostAsync("api/v1/books", content);
    Console.WriteLine($"Loaded: {book.Title} ? {response.StatusCode}");
}

static string ToTitleCase(string value)
{
    return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value.Trim().ToLowerInvariant());
}

static async Task WaitForApiAsync(HttpClient client, TimeSpan timeout)
{
    var deadline = DateTimeOffset.UtcNow + timeout;
    Exception? lastException = null;

    while (DateTimeOffset.UtcNow < deadline)
    {
        try
        {
            using var response = await client.GetAsync("api/v1/books");
            if (response.IsSuccessStatusCode)
            {
                return;
            }
        }
        catch (HttpRequestException exception)
        {
            lastException = exception;
        }
        catch (TaskCanceledException exception)
        {
            lastException = exception;
        }

        await Task.Delay(1000);
    }

    throw new HttpRequestException(
        $"Unable to reach the API at {client.BaseAddress} after waiting {timeout.TotalSeconds:F0} seconds.",
        lastException);
}

class TransformedBook
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Author { get; set; }
    public string? Genre { get; set; }
    public bool Available { get; set; }
    public int PublishedYear { get; set; }
}
