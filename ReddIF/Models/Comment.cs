using System.Collections.ObjectModel;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;


namespace ReddIF.Models;

public class Comment: BaseModel
{
    public int CommentId { get; set; }
    
    public User User { get; set; } //qm fez o coment

    public string CommentContent { get; set; } = string.Empty;

    public DateTime CommentTime = DateTime.Now;
    
}

//falta colocar votos 