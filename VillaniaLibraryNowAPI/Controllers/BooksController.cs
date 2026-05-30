using Microsoft.AspNetCore.Mvc;
using VillaniaLibraryNowAPI.Models;
using VillaniaLibraryNowAPI.Repositories;
using VillaniaLibraryNowAPI.Services;

namespace VillaniaLibraryNowAPI.Controllers
{
    [Route("api/v1/books")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly IBookRepository _bookRepository;
        private readonly IBookImportService _bookImportService;

        public BooksController(IBookRepository bookRepository, IBookImportService bookImportService)
        {
            _bookRepository = bookRepository;
            _bookImportService = bookImportService;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var books = _bookRepository.GetAll();

            return Ok(new
            {
                status = "success",
                data = books,
                message = "books retrieved"
            });
        } 
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var book = _bookRepository.GetById(id);
            if (book == null)
            {
                return NotFound(new
                {
                    status = "error",
                    data = (object?)null,
                    message = "book not found"
                });
            }
            return Ok(new
            {
                status = "success",
                data = book,
                message = "book retrieved"
            });
        }

        [HttpPost]
        public IActionResult Create([FromBody] Book newBook)
        {
            var createdBook = _bookRepository.Add(newBook);

            return CreatedAtAction(nameof(GetById),
                new
                {
                    id = createdBook.Id
                },
                new
                {
                    status = "success",
                    data = createdBook,
                    message = "book created"
                });
        }

        [HttpPost("import")]
        public IActionResult ImportLegacyBooks([FromQuery] string? filePath = null)
        {
            try
            {
                var summary = _bookImportService.Import(filePath);

                return Ok(new
                {
                    status = "success",
                    data = summary,
                    message = "legacy books imported"
                });
            }
            catch (FileNotFoundException)
            {
                return NotFound(new
                {
                    status = "error",
                    data = (object?)null,
                    message = "legacy CSV file not found"
                });
            }
            catch (InvalidDataException exception)
            {
                return BadRequest(new
                {
                    status = "error",
                    data = (object?)null,
                    message = exception.Message
                });
            }
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Book updateBook)
        {
            var updatedBook = _bookRepository.Update(id, updateBook);
            if (updatedBook == null)
            {
                return NotFound(new
                {
                    status = "error",
                    data = (object?)null,
                    message = "book not found"
                });
            }

            return Ok(new
            {
                status = "success",
                data = updatedBook,
                message = "book updated"
            });
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var deleted = _bookRepository.Delete(id);
            if (!deleted)
            {
                return NotFound(new
                {
                    status = "error",
                    data = (object?)null,
                    message = "book not found"
                });
            }

            return Ok(new
            {
                status = "success",
                data = (object?)null,
                message = "book deleted"
            });
        }
    }
}
