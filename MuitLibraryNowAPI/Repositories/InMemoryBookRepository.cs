using MuitLibraryNowAPI.Models;

namespace MuitLibraryNowAPI.Repositories
{
    public class InMemoryBookRepository : IBookRepository
    {
        private readonly List<Book> _books =
        [
            new Book
            {
                Id = 1,
                Title = "Realms of Runeterra",
                Author = "Riot Games",
                Genre = "Lore / World Guide",
                Available = true,
                PublishedYear = 2019
            },
            new Book
            {
                Id = 2,
                Title = "Ruination: A League of Legends Novel",
                Author = "Anthony Reynolds",
                Genre = "Fantasy / Dark Fiction",
                Available = true,
                PublishedYear = 2022
            },
            new Book
            {
                Id = 3,
                Title = "The Name of the Wind",
                Author = "Patrick Rothfuss",
                Genre = "Epic Fantasy",
                Available = true,
                PublishedYear = 2007
            },
            new Book
            {
                Id = 4,
                Title = "Mistborn: The Final Empire",
                Author = "Brandon Sanderson",
                Genre = "High Fantasy",
                Available = true,
                PublishedYear = 2006
            },
            new Book
            {
                Id = 5,
                Title = "The Way of Kings",
                Author = "Brandon Sanderson",
                Genre = "Epic Fantasy",
                Available = false,
                PublishedYear = 2010
            },
            new Book
            {
                Id = 6,
                Title = "Project Hail Mary",
                Author = "Andy Weir",
                Genre = "Science Fiction",
                Available = true,
                PublishedYear = 2021
            },
            new Book
            {
                Id = 7,
                Title = "The Priory of the Orange Tree",
                Author = "Samantha Shannon",
                Genre = "Fantasy",
                Available = true,
                PublishedYear = 2019
            },
            new Book
            {
                Id = 8,
                Title = "The Silent Patient",
                Author = "Alex Michaelides",
                Genre = "Psychological Thriller",
                Available = false,
                PublishedYear = 2019
            },
            new Book
            {
                Id = 9,
                Title = "Dune",
                Author = "Frank Herbert",
                Genre = "Science Fiction",
                Available = true,
                PublishedYear = 1965
            },
            new Book
            {
                Id = 10,
                Title = "Ninth House",
                Author = "Leigh Bardugo",
                Genre = "Dark Academia Fantasy",
                Available = true,
                PublishedYear = 2019
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
