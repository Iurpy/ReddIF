using System.ComponentModel.DataAnnotations;

namespace ReddIF.Models;

public class Post
{
    [Key]
    public int Id { get; set; }
    
    public int UserAutorId { get; set; }

    public string Title { get; set; } = string.Empty;
    
    [MaxLength(250)]
    public string Content { get; set; } = string.Empty;
    
    public DateTime PostTime { get; set; }= DateTime.Now;
    
    public required User User { get; set; }
}

//falta colocar votos