namespace ReddIF.Models;

public class Comment
{
    public int Id { get; set; }
    
    public User User { get; set; } //qm fez o coment

    public string CommentContent { get; set; } = string.Empty;

    public DateTime CommentTime = DateTime.Now;
    
}

//falta colocar votos 