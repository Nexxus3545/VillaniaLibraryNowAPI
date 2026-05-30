using OleleLibraryNowAPI.Models;

namespace OleleLibraryNowAPI.Repositories
{
    public class InMemoryBookRepository : IBookRepository
    {
        private readonly List<Book> _books =
        [
            new Book
            {
                Id = 1,
                Title = "The Gentle Reminder",
                Author = "Bianca Sparacino",
                Genre = "Poetic Self help",
                Available = true,
                PublishedYear = 2021
            },
            new Book
            {
                Id = 2,
                Title = "let Go and Let God",
                Author = "Albert E. Cliffe",
                Genre = "Spritual Self Help",
                Available = true,
                PublishedYear = 1954
            },
            new Book
            {
                Id = 3,
                Title = "Atomic Habits",
                Author = "James Clear",
                Genre = "Self Help",
                Available = true,
                PublishedYear = 2018
            },
            new Book
            {
                Id = 4,
                Title = "The Alchemist",
                Author = "Paulo Coelho",
                Genre = "Adventure Fiction",
                Available = true,
                PublishedYear = 1988
            },
            new Book
            {
                Id = 5,
                Title = "To Kill a Mockingbird",
                Author = "Harper Lee",
                Genre = "Classic Fiction",
                Available = true,
                PublishedYear = 1960
            },
            new Book
            {
                Id = 6,
                Title = "The Midnight Library",
                Author = "Matt Haig",
                Genre = "Contemporary Fiction",
                Available = true,
                PublishedYear = 2020
            },
            new Book
            {
                Id = 7,
                Title = "The Four Agreements",
                Author = "Don Miguel Ruiz",
                Genre = "Spiritual Self Help",
                Available = false,
                PublishedYear = 1997
            },
            new Book
            {
                Id = 8,
                Title = "It Ends with Us",
                Author = "Colleen Hoover",
                Genre = "Romance Fiction",
                Available = true,
                PublishedYear = 2016
            },
            new Book
            {
                Id = 9,
                Title = "Deep Work",
                Author = "Cal Newport",
                Genre = "Productivity",
                Available = true,
                PublishedYear = 2016
            },
            new Book
            {
                Id = 10,
                Title = "Think and Grow Rich",
                Author = "Napoleon Hill",
                Genre = "Personal Development",
                Available = false,
                PublishedYear = 1937
            }
        ];
        private readonly object _syncRoot = new();
        private int _nextId = 11;

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
