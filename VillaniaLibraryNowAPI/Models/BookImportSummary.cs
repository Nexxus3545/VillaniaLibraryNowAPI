namespace VillaniaLibraryNowAPI.Models
{
    public class BookImportSummary
    {
        public string SourceFile { get; init; } = string.Empty;
        public int ExtractedRows { get; init; }
        public int LoadedRows { get; init; }
        public int SkippedRows { get; init; }
        public DateTime ImportedAtUtc { get; init; }
        public IReadOnlyList<string> Warnings { get; init; } = Array.Empty<string>();
    }
}
