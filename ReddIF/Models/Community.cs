using System.Collections.ObjectModel;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;


namespace ReddIF.Models;

public class Community: BaseModel
{
    public Community()
    {
        Users = new Collection<User>();
    }
    
    [PrimaryKey("",false)]
    public int CommunityId {get; set;}
    
    [Column("dono")]
    public int OwnerId { get; set; }

    public DateTime CreateTime { get; set; } = DateTime.Now;
    
    public required string Nome {get; set;} = string.Empty;
    
    public string Description { get; set; } = string.Empty; 
    public ICollection<User> Users { get; set; }
    
}