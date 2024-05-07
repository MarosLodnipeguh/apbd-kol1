using System.ComponentModel.DataAnnotations;

namespace apbd_kol1.Models;

public class BookDTO
{
    public int id { get; set; }
    [Required]
    public string title { get; set; }
    public List<AuthorDTO> authors { get; set; }
}