using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ReddIF.Models;
using Supabase;

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
            if (req == null)
                return BadRequest(new { erro = "Dados inválidos." });

            if (string.IsNullOrWhiteSpace(req.Name))
                return BadRequest(new { erro = "Nome é obrigatório." });

            if (string.IsNullOrWhiteSpace(req.Email))
                return BadRequest(new { erro = "Email é obrigatório." });

            if (string.IsNullOrWhiteSpace(req.Senha))
                return BadRequest(new { erro = "Senha é obrigatória." });

            var name = req.Name.Trim();
            var email = req.Email.Trim();
            var senha = req.Senha;

            var existeEmail = await _supabase
                .From<User>()
                .Where(u => u.Email == email)
                .Get();

            var existeNome = await _supabase
                .From<User>()
                .Where(u => u.Name == name)
                .Get();

            if (existeEmail.Models.Any())
                return BadRequest(new { erro = "Email já cadastrado" });

            if (existeNome.Models.Any())
                return BadRequest(new { erro = "Nome de usuário já cadastrado" });

            var existingUsers = await _supabase
                .From<User>()
                .Limit(1)
                .Get();

            var isFirstUser = !existingUsers.Models.Any();

            var novoUser = new User
            {
                Name = name,
                Email = email,
                SenhaHash = HashSenha(senha),
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
            if (req == null)
                return BadRequest(new { erro = "Dados inválidos." });

            if (string.IsNullOrWhiteSpace(req.Email))
                return BadRequest(new { erro = "Email é obrigatório." });

            if (string.IsNullOrWhiteSpace(req.Senha))
                return BadRequest(new { erro = "Senha é obrigatória." });

            var email = req.Email.Trim();
            var senha = req.Senha;

            var response = await _supabase
                .From<User>()
                .Where(u => u.Email == email)
                .Get();

            var user = response.Models.FirstOrDefault();

            if (user == null)
                return Unauthorized(new { erro = "Email ou senha inválidos" });

            if (string.IsNullOrWhiteSpace(user.SenhaHash))
                return Unauthorized(new { erro = "Email ou senha inválidos" });

            var senhaCorreta = BCrypt.Net.BCrypt.Verify(senha, user.SenhaHash);

            if (!senhaCorreta)
                return Unauthorized(new { erro = "Email ou senha inválidos" });

            if (!user.Active)
                return Unauthorized(new { erro = "Usuário banido" });

            var jwtKey = _configuration["Jwt:Key"];

            if (string.IsNullOrWhiteSpace(jwtKey))
                return StatusCode(500, new { erro = "Chave JWT não configurada." });

            var keyBytes = Encoding.UTF8.GetBytes(jwtKey);

            if (keyBytes.Length < 16)
                return StatusCode(500, new { erro = "A chave JWT precisa ter pelo menos 16 caracteres." });

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Name ?? ""),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim(ClaimTypes.Role, user.Role ?? "user")
            };

            var creds = new SigningCredentials(
                new SymmetricSecurityKey(keyBytes),
                SecurityAlgorithms.HmacSha256
            );

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

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

    // GET /api/users/{name} — busca por nome
    [HttpGet("{name}")]
    public async Task<IActionResult> GetByName(string name)
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

    // PUT /api/users/{id} — edita perfil
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateRequest req)
    {
        try
        {
            if (req == null)
                return BadRequest(new { erro = "Dados inválidos." });

            var response = await _supabase
                .From<User>()
                .Where(u => u.UserId == id)
                .Get();

            var user = response.Models.FirstOrDefault();

            if (user == null)
                return NotFound(new { erro = "Usuário não encontrado" });

            if (!string.IsNullOrWhiteSpace(req.Name))
                user.Name = req.Name.Trim();

            if (!string.IsNullOrWhiteSpace(req.Senha))
                user.SenhaHash = HashSenha(req.Senha);

            await _supabase.From<User>().Update(user);

            return Ok(new { mensagem = "Perfil atualizado com sucesso!" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { erro = ex.Message });
        }
    }
}

// Classes auxiliares para receber dados do body
public record RegisterRequest(string? Name, string? Email, string? Senha);
public record LoginRequest(string? Email, string? Senha);
public record UpdateRequest(string? Name, string? Senha);