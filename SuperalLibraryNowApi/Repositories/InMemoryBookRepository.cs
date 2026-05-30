using SuperalLibraryNowApi.Models;

namespace SuperalLibraryNowApi.Repositories
{
    public class InMemoryBookRepository : IBookRepository
    {
        private readonly List<Book> _books =
        [
            new Book
            {
                Id = 1,
                Title = "Crime and Punishment",
                Author = "Fyodor Dostoevsky",
                Genre = "Drama",
                Available = true,
                PublishedYear = 1866
            },
            new Book
            {
                Id = 2,
                Title = "Lord Of The Rings",
                Author = "J.R.R Tolkien",
                Genre = "Fantasy",
                Available = true,
                PublishedYear = 1954
            }
        ];
        private readonly object _syncRoot = new();
        private int _nextId = 3;

        public IReadOnlyList<Book> GetAll()
        {
            lock (_syncRoot)
            {
                return _books
                    .OrderBy(book => book.Id)
                    .Select(Clone)
                    .ToList();
            }
        }

        public Book? GetById(int id)
        {
            lock (_syncRoot)
            {
                var book = _books.FirstOrDefault(existingBook => existingBook.Id == id);
                return book == null ? null : Clone(book);
            }
        }

        public Book Add(Book newBook)
        {
            lock (_syncRoot)
            {
                var storedBook = Clone(newBook);
                storedBook.Id = _nextId++;
                _books.Add(storedBook);
                return Clone(storedBook);
            }
        }

        public Book? Update(int id, Book updatedBook)
        {
            lock (_syncRoot)
            {
                var existingBook = _books.FirstOrDefault(book => book.Id == id);
                if (existingBook == null)
                {
                    return null;
                }

                existingBook.Title = updatedBook.Title;
                existingBook.Author = updatedBook.Author;
                existingBook.Genre = updatedBook.Genre;
                existingBook.Available = updatedBook.Available;
                existingBook.PublishedYear = updatedBook.PublishedYear;

                return Clone(existingBook);
            }
        }

        public bool Delete(int id)
        {
            lock (_syncRoot)
            {
                var book = _books.FirstOrDefault(existingBook => existingBook.Id == id);
                if (book == null)
                {
                    return false;
                }

                _books.Remove(book);
                return true;
            }
        }

        public void ReplaceAll(IEnumerable<Book> books)
        {
            lock (_syncRoot)
            {
                _books.Clear();
                _nextId = 1;

                foreach (var book in books)
                {
                    var storedBook = Clone(book);
                    storedBook.Id = _nextId++;
                    _books.Add(storedBook);
                }
            }
        }

        private static Book Clone(Book source)
        {
            return new Book
            {
                Id = source.Id,
                Title = source.Title,
                Author = source.Author,
                Genre = source.Genre,
                Available = source.Available,
                PublishedYear = source.PublishedYear
            };
        }
    }
}
