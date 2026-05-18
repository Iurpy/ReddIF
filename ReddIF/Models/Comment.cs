using System.Collections.ObjectModel;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;


namespace ReddIF.Models;

[Table("comments")]
public class Comment: BaseModel
{
    [Column("comment_id")]
    public int CommentId { get; set; }

    [Column("post_id")]
    public int PostId{get; set;}
    
    [Column("author_user_id")]
    public int AuthorUserId { get; set; } //qm fez o coment
    
    [Column("content")]
    public string Content { get; set; } = string.Empty;

    [Column("created_at")]
    public DateTime CreatedAt {get; set;} = DateTime.Now;

    [Column("up_vote")]
    public int UpVote{get; set;}
    
}

//falta colocar votos 