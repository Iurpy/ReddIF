using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using ReddIF.Models;
using Supabase;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel;
using Microsoft.IdentityModel.Tokens;

namespace ReddIF.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly Client _supabase;
    private readonly IConfiguration _configuration;

    public UsersController(Client supabase, IConfiguration configuration)
    {
        _supabase = supabase;
        _configuration = configuration;
    }
    
private string HashSenha(string senha)
{
    return BCrypt.Net.BCrypt.HashPassword(senha);
}

    // POST /api/users/register — cria conta
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
    {
        try
        {
            // Verifica se email já existe
            var existeEmail = await _supabase
                .From<User>()
                .Where(u => u.Email == req.Email)
                .Get();
            
            var exiteNome = await _supabase
                .From<User>()
                .Where(u => u.Name == req.Name)
                .Get();
            
            if (existeEmail.Models.Any())
                return BadRequest(new { erro = "Email já cadastrado" });
            if (exiteNome.Models.Any())
                return BadRequest(new { erro = "Nome de usuario já cadastrado"});

            var existingUsers = await _supabase
                .From<User>()
                .Limit(1)
                .Get();
            
            var isFirstUser = !existingUsers.Models.Any();
            
            var novoUser = new User 
            {
                Name = req.Name,
                Email = req.Email,
                SenhaHash = HashSenha(req.Senha),
                Karma = 0,
                Active = true,
                CreateTime = DateTime.Now,
                Role = isFirstUser ? "admin" : "user",
            };

            var response = await _supabase.From<User>().Insert(novoUser);
            var user = response.Models.FirstOrDefault();
            
            return Ok(new
            {
                mensagem = "Usuário criado com sucesso!",
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

    // POST /api/users/login — login
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

            if (user == null || !BCrypt.Net.BCrypt.Verify(req.Senha, user.SenhaHash))
                return Unauthorized(new { erro = "Email ou senha inválidos" });

            var key = Convert.FromBase64String(_configuration["Jwt:Key"]);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role ?? "User")
            };

            var keyByte = Convert.ToByte(key);
            
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
                mensagem = "Login realizado com sucesso!",
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

    // GET /api/users — lista todos
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var response = await _supabase.From<User>().Get();

            var usuarios = response.Models.Select(u => new
            {
                u.UserId,
                u.Name,
                u.Email,
                u.Karma,
                u.Active,
                u.CreateTime
            });

            return Ok(new { total = usuarios.Count(), usuarios });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { erro = ex.Message });
        }
    }

    // GET /api/users/1 — busca por nome
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

    // PUT /api/users/1 — edita perfil
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

            if (!string.IsNullOrEmpty(req.Senha))
                user.SenhaHash = HashSenha(req.Senha);

            await _supabase.From<User>().Update(user);

            return Ok(new { mensagem = "Perfil atualizado com sucesso!" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { erro = ex.Message });
        }
    }

    // DELETE /api/users/1 — desativa usuário (ban)
   
}

// Classes auxiliares para receber dados do body
public record RegisterRequest(string? Name, string? Email, string? Senha);
public record LoginRequest(string? Email, string? Senha);
public record UpdateRequest(string? Name, string? Senha);