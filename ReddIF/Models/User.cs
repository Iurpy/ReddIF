using System.Collections.ObjectModel;
using Supabase.Gotrue;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace ReddIF.Models;

[Table("usuario")]
public class User: BaseModel
{
    /*
    public User()
    {
        Posts = new Collection<Post>();
        Communities = new Collection<Community>();
    }
    */
    
    [PrimaryKey("userid",false)]
    public int UserId { get; set; }
    
    [Column("nome")]
    public string Name { get; set; } = string.Empty;
    
    [Column("email")]
    public string Email { get; set; } = string.Empty;
    
    [Column("password")]
    public string SenhaHash { get; set; } = string.Empty;

    [Column("karma")] 
    public int Karma { get; set; }

    [Column("active")]
    public bool Active { get; set; } = true;

    [Column("criadoem")]
    public DateTime CreateTime { get; set; } = DateTime.Now;

    [Column("role")] public string Role { get; set; } = "user";

/*
    public ICollection<Post> Posts { get; set; }
    //isso aqui diz que um usuario pode ter varios posts
    public ICollection<Community> Communities { get; set; }
    //pode pertencer a varias comunidades
*/
}