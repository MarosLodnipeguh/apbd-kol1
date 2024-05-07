using apbd_kol1.Models;
using apbd_kol1.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace apbd_kol1.Controllers;

[ApiController]
[Route("api/books")]

public class BookController : ControllerBase
{
    private readonly IBookRepository _bookRepository;

    public BookController(IBookRepository bookRepository)
    {
        _bookRepository = bookRepository;
    }
    
    
    [HttpGet("{id}/authors")]
    public async Task<IActionResult> GetBookAuthors(int id)
    {
        // check if book does not exists
        if (!await _bookRepository.DoesBookExist(id))
            return NotFound($"Book with id {id} does not exist");

        var authors = await _bookRepository.GetBookAuthors(id);

        return Ok(authors);
    }

    [HttpPost]
    public async Task<IActionResult> AddBook(BookDTO newBookDTO)
    {
        // check if book exists
        if (await _bookRepository.DoesBookExist(newBookDTO.id))
            return NotFound($"Book with id {newBookDTO.id} does exist");

        var id = await _bookRepository.AddBook(newBookDTO);

        var authors = await _bookRepository.GetBookAuthors(id);


        return Created("api/books/" + id, $"Book with id = {id} has been added: " + authors.title);
    }
}