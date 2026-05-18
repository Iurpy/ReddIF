using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using ReddIF.Models;
using Supabase;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.ComponentModel.DataAnnotations;



namespace ReddIF.Controllers;

[ApiController]
[Route("api/usuarios")]
public class UsersController : ControllerBase
{
    private readonly Client _supabase;
    private readonly IConfiguration _configuration;

    public UsersController(Client supabase, IConfiguration configuration)
    {
        _supabase = supabase;
        _configuration = configuration;
    }
    
private string HashPassword(string password)
{
    return BCrypt.Net.BCrypt.HashPassword(password);
}

[HttpPost("registro")]
public async Task<IActionResult> Register([FromBody] RegisterRequest req)
{
    try
    {
        var emailExists = await _supabase
            .From<User>()
            .Where(u => u.Email == req.Email)
            .Get();
    
        var NamesExists = await _supabase
            .From<User>()
            .Where(u => u.Name == req.Name)
            .Get();
            
        if (emailExists.Models.Any())
            return BadRequest(new { erro = "Email já cadastrado" });
        
        if (NamesExists.Models.Any())
            return BadRequest(new { erro = "Nome de usuario já cadastrado"});

        var existingUsers = await _supabase
            .From<User>()
            .Limit(1)
            .Get();
            
        var isFirstUser = !existingUsers.Models.Any();
            
        var newUser = new User 
        {
            Name = req.Name,
            Email = req.Email,
            PasswordHash = HashPassword(req.Password),
            Karma = 0,
            Active = true,
            CreateTime = DateTime.Now,
            Role = isFirstUser ? "admin" : "user",
        };

        var response = await _supabase.From<User>().Insert(newUser);
        
        var user = response.Models.FirstOrDefault();
            
        return Ok(new
        {
            message = "Usuário criado com sucesso!",
            UserId = user?.UserId,
            user?.Name,
            user?.Email
        });
    }
    
    catch (Exception ex)
    {
        return StatusCode(500, new { erro = ex.Message });
    }
}

[HttpPost("login")]
public async Task<IActionResult> Login([FromBody] LoginRequest req)
{
    try
    {
        var response = await _supabase
            .From<User>()
            .Where(u => u.Email == req.Email)                
            .Get();

        var user = response.Models.FirstOrDefault();

        if (user == null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            return Unauthorized(new { erro = "Email ou senha inválidos" });
 
        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!);
       
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role ?? "User")
        };
            
            
        var creds = new SigningCredentials(
            new SymmetricSecurityKey(key),    
            SecurityAlgorithms.HmacSha256
        );

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: creds
        );
            
        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            
        if (!user.Active)
            return Unauthorized(new { erro = "Usuário banido" });

        return Ok(new
        {
            message = "Login realizado com sucesso!",
            token = tokenString,    
            user = new
            {
                user.UserId,
                user.Name,
                user.Email,
                user.Karma,
                user.Role
            }
        });
    }
   
    catch (Exception ex)
    {
        return StatusCode(500, new { erro = ex.Message });
    }
}

[HttpGet]
public async Task<IActionResult> GetAll()
{
    try
    {
        var response = await _supabase
            .From<User>()
            .Get();
        var users = response.Models.Select(u => new
        {
            u.UserId,
            u.Name,
            u.Email,
            u.Karma,
            u.Active,
            u.CreateTime
        });

        return Ok(new { total = users.Count(), users });
    }
   
    catch (Exception ex)
    {
        return StatusCode(500, new { erro = ex.Message });
    }
}

[HttpGet("{name}")]
public async Task<IActionResult> GetById(string name)
{
    try
    {
        var response = await _supabase
            .From<User>()
            .Where(u => u.Name == name)
            .Get();

        var user = response.Models.FirstOrDefault();

        if (user == null)
            return NotFound(new { erro = "Usuário não encontrado" });

        return Ok(new 
        {
            user.UserId,
            user.Name,
            user.Email,
            user.Karma,
            user.Active,
            user.CreateTime
        });
    }
    
    catch (Exception ex)
    {
        return StatusCode(500, new { erro = ex.Message });
    }
}

[HttpPut("{id}")]
public async Task<IActionResult> Update(int id, [FromBody] UpdateRequest req)
{
    try
    {
        var response = await _supabase
            .From<User>()
            .Where(u => u.UserId == id)
            .Get();

        var user = response.Models.FirstOrDefault();

        if (user == null)
            return NotFound(new { erro = "Usuário não encontrado" });

        if (!string.IsNullOrEmpty(req.Name))
            user.Name = req.Name;

        if (!string.IsNullOrEmpty(req.Password))
            user.PasswordHash = HashPassword(req.Password);

        await _supabase
              .From<User>()
              .Update(user);

        return Ok(new { message = "Perfil atualizado com sucesso!" });
    }
    
    catch (Exception ex)
    {
        return StatusCode(500, new { erro = ex.Message });
    }
}   
}

public record RegisterRequest([Required] string? Name, [Required] string? Email, [Required] string? Password);
public record LoginRequest([Required] string? Email, [Required] string? Password);
public record UpdateRequest([Required] string? Name, [Required] string? Password);