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
    public int UserAutorId { get; set; }

    [Column("title")]
    public string Title { get; set; } = string.Empty;
    
    [Column("content")]
    public string Content { get; set; } = string.Empty;

     
    [Column("post_time")]
    public DateTime PostTime { get; set; }= DateTime.Now;
    
    [Column("community_id")]
    public int CommunityId { get; set; } 
}


//falta colocar votos