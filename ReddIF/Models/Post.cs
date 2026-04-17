using System.ComponentModel.DataAnnotations;

namespace ReddIF.Models;

public class Post
{
    [Key]
    public int Id { get; set; }
    
    public int UserId { get; set; }
    
    public required User User { get; set; }
}