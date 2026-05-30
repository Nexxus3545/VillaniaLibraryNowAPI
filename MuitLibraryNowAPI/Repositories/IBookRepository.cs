using MuitLibraryNowAPI.Models;

namespace MuitLibraryNowAPI.Repositories
{
    public interface IBookRepository
    {
        IReadOnlyList<Book> GetAll();
        Book? GetById(int id);
        Book Add(Book newBook);
        Book? Update(int id, Book updatedBook);
        bool Delete(int id);
        void ReplaceAll(IEnumerable<Book> books);
    }
}
