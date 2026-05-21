using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace ReddIF.Models;

[Table("comments")]
public class Comment : BaseModel
{
    [PrimaryKey("comment_id", false)]
    public long CommentId { get; set; }

    [Column("post_id")]
    public int PostId { get; set; }

    [Column("author_user_id")]
    public int AuthorUserId { get; set; }

    [Column("content")]
    public string Content { get; set; } = string.Empty;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}