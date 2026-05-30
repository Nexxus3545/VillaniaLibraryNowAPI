using SuperalLibraryNowApi.Models;

namespace SuperalLibraryNowApi.Services
{
    public interface IBookImportService
    {
        BookImportSummary Import(string? filePath = null);
    }
}
