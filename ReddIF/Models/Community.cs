using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace ReddIF.Models;

public class Community
{
    public Community()
    {
        Users = new Collection<User>();
    }
    [Key]
    public int Id {get; set;}
    
    [Required(ErrorMessage = "A comunidade deve ter um nome")]
    [MaxLength(25, ErrorMessage = "O nome da comunidade deve conter no maximo 25 caracteres")]
    public required string Nome {get; set;} = string.Empty;
    
    public ICollection<User> Users { get; set; }
    
}