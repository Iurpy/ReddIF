using System.Collections.ObjectModel;
using Supabase.Gotrue;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace ReddIF.Models;

[Table("users")]
public class User: BaseModel
{
    
    [PrimaryKey("user_id",false)]
    public int UserId { get; set; }
    
    [Column("name")]
    public string Name { get; set; } = string.Empty;
    
    [Column("email")]
    public string Email { get; set; } = string.Empty;
    
    [Column("password_hash")]
    public string PasswordHash { get; set; } = string.Empty;

    [Column("karma")] 
    public int Karma { get; set; }

    [Column("active")]
    public bool Active { get; set; } = true;

    [Column("created_at")]
    public DateTime CreateTime { get; set; } = DateTime.Now;

    [Column("role")] public string Role { get; set; } = "user";

}