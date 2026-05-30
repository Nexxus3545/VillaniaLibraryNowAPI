using OleleLibraryNowAPI.Models;

namespace OleleLibraryNowAPI.Services
{
    public interface IBookImportService
    {
        BookImportSummary Import(string? filePath = null);
    }
}
