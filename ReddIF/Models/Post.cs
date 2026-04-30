using System.Collections.ObjectModel;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;


namespace ReddIF.Models;

[Table("post")]
public class Post: BaseModel
{
    [PrimaryKey("postid", false)]
    public int PostId { get; set; }
    
    [Column("userautorid")]
    public int UserAutorId { get; set; }

    [Column("title")]
    public string Title { get; set; } = string.Empty;
    
    [Column("content")]
    public string Content { get; set; } = string.Empty;
    
    [Column("posttime")]
    public DateTime PostTime { get; set; }= DateTime.Now;
    
}

//falta colocar votos