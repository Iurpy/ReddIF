using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace ReddIF.Models;

public class User
{
    public User()
    {
        Posts = new Collection<Post>();
        Communitys = new Collection<Community>();
    }
    
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "O nome deve ser informado.")]
    [MaxLength(100)]
    public string Nome { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "O e-mail precisa ser informado.")]
    [EmailAddress(ErrorMessage = "E-mail invalido.")]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "A senha é obrigatoria.")]
    [MinLength(8, ErrorMessage = "A senha deve conter no minimo 8 caracteres")]
    [MaxLength(20, ErrorMessage = "A senha deve ter no maximo 20 caracteres")]
    public string SenhaHash { get; set; } = string.Empty;
    
    public int Karma { get; set; }

    public bool Ativo { get; set; } = true;

    public DateTime CriadoEm { get; set; } = DateTime.Now;

    public ICollection<Post> Posts { get; set; }
    //isso aqui diz que um usuario pode ter varios posts
    public ICollection<Community> Communitys { get; set; }
    //pode pertencer a varias comunidades
   
}