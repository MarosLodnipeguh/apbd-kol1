using apbd_kol1.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Data.SqlClient;

namespace apbd_kol1.Repositories;

public class BookRepository : IBookRepository
{
    private readonly IConfiguration _configuration;

    public BookRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }


    public async Task<bool> DoesBookExist(int id)
    {
        await using var connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using var command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = "SELECT 1 FROM books WHERE @Id = PK";
        command.Parameters.AddWithValue("@Id", id);

        await connection.OpenAsync();

        var result = await command.ExecuteScalarAsync();

        return result is not null;
    }
    
    public async Task<string> GetBookTitle(int id)
    {
        await using var connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using var command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = "SELECT title FROM books WHERE PK = @id";
        command.Parameters.AddWithValue("@Id", id);

        await connection.OpenAsync();
        
        var reader = await command.ExecuteReaderAsync();
        
        var title = reader.GetOrdinal("title");
        string titl = reader.GetString(title);

        return titl;
    }

    public async Task<BookDTO> GetBookAuthors(int id)
    {
        await using var connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using var command = new SqlCommand();

        command.Connection = connection;
        command.CommandText =
            "SELECT a.first_name, a.last_name FROM authors a JOIN books_authors ba ON a.PK = ba.FK_author JOIN books b ON ba.FK_book = b.PK";

        await connection.OpenAsync();

        var reader = await command.ExecuteReaderAsync();

        var first = reader.GetOrdinal("first_name");
        var last = reader.GetOrdinal("last_name");

        var authors = new List<AuthorDTO>();

        while (reader.Read())
        {
            authors.Add(new AuthorDTO()
            {
                first_name = reader.GetString(first),
                last_name = reader.GetString(last),
            });
        }

        // get title
        // command.Parameters.Clear();
        // command.Connection = connection;
        // command.CommandText = "SELECT title FROM books WHERE PK = @id";
        // command.Parameters.AddWithValue("@Id", id);
        //
        // await connection.OpenAsync();
        //
        // reader = await command.ExecuteReaderAsync();
        // var title = reader.GetOrdinal("title");
        // string titleStr = reader.GetString(title);
        
        var book = new BookDTO() { id = id, title = "titleStr" , authors = authors};
        
        return book;
    }

    public async Task<int> AddBook(BookDTO newBookDTO)
    {
        await using var connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using var command = new SqlCommand();

        // add book
        command.Connection = connection;
        command.CommandText = "INSERT INTO books VALUES (@title) SELECT @@IDENTITY AS ID";
        command.Parameters.AddWithValue("@title", newBookDTO.title);
        
        await connection.OpenAsync();
        var transaction = await connection.BeginTransactionAsync();
        command.Transaction = transaction as SqlTransaction;

        object? id;
        
        // add authors
        try
        {
            id = Convert.ToInt32(await command.ExecuteScalarAsync());

            foreach (var author in newBookDTO.authors)
            {
                command.Parameters.Clear();
                command.CommandText = "INSERT INTO authors VALUES (@first, @last) SELECT @@IDENTITY AS ID";
                command.Parameters.AddWithValue("@first", author.first_name);
                command.Parameters.AddWithValue("@last", author.last_name);

                await command.ExecuteScalarAsync();

                object? authorId;
                authorId = Convert.ToInt32(await command.ExecuteScalarAsync());
                command.Parameters.Clear();
                command.CommandText = "INSERT INTO books_authors VALUES (@bookId, @authorId)";
                command.Parameters.AddWithValue("@bookId", id);
                command.Parameters.AddWithValue("@authorId", authorId);
                
                await command.ExecuteScalarAsync();
            }

            await transaction.CommitAsync();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }

        return (int)id;
        
    }
}