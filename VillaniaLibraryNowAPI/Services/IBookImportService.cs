using VillaniaLibraryNowAPI.Models;

namespace VillaniaLibraryNowAPI.Services
{
    public interface IBookImportService
    {
        BookImportSummary Import(string? filePath = null);
    }
}
