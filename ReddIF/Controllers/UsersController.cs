using Microsoft.AspNetCore.Mvc;
using ReddIF.Models;
using Supabase;

namespace ReddIF.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly Client _supabase;

    public UsersController(Client supabase)
    {
        _supabase = supabase;
    }


    // função auxiliar pra criptografar senha
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
            var existe = await _supabase
                .From<User>()
                .Where(u => u.Email == req.Email)
                .Get();

            if (existe.Models.Any())
                return BadRequest(new { erro = "Email já cadastrado" });

            var novoUser = new User
            {
                Name = req.Nome,
                Email = req.Email,
                SenhaHash = HashSenha(req.Senha),
                Karma = 0,
                Active = true,
                CreateTime = DateTime.Now
            };

            var response = await _supabase.From<User>().Insert(novoUser);
            var user = response.Models.FirstOrDefault();

            return Ok(new
            {
                mensagem = "Usuário criado com sucesso!",
                user?.UserId,
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

            if (!user.Active)
                return Unauthorized(new { erro = "Usuário banido" });

            return Ok(new
            {
                mensagem = "Login realizado com sucesso!",
                user.UserId,
                user.Name,
                user.Email,
                user.Karma
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

    // GET /api/users/1 — busca por ID
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
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

            if (!string.IsNullOrEmpty(req.Nome))
                user.Name = req.Nome;

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
public record RegisterRequest(string Nome, string Email, string Senha);
public record LoginRequest(string Email, string Senha);
public record UpdateRequest(string? Nome, string? Senha);