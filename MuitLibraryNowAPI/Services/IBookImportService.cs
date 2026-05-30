using MuitLibraryNowAPI.Models;

namespace MuitLibraryNowAPI.Services
{
    public interface IBookImportService
    {
        BookImportSummary Import(string? filePath = null);
    }
}
