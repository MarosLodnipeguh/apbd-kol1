using apbd_kol1.Models;

namespace apbd_kol1.Repositories;

public interface IBookRepository
{
    Task<bool> DoesBookExist(int id);
    Task<string> GetBookTitle(int id);
    Task<BookDTO> GetBookAuthors(int id);
    Task<int> AddBook(BookDTO newBookDTO);
}