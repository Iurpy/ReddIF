using System.Collections.ObjectModel;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;


namespace ReddIF.Models;

[Table("posts")]
public class Post: BaseModel
{
    [PrimaryKey("post_id", false)]
    public int PostId { get; set; }
    
    [Column("author_user_id")]
    public int UserAuthorId { get; set; }

    [Column("title")]
    public string Title { get; set; } = string.Empty;
    
    [Column("content")]
    public string Content { get; set; } = string.Empty;

     
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }= DateTime.Now;
    
    [Column("community_id")]
    public int CommunityId { get; set; } 

    [Column("active")]
    public bool Active { get; set; } = true;

    [Column("image_url")]
    public string? ImageUrl { get; set; } = null;
}


//falta colocar votos