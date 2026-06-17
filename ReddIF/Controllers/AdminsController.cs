using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReddIF.Models;
using Supabase;
using System.Security.Claims;

namespace ReddIF.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize]
public class AdminController : ControllerBase
{
    private readonly Client _supabase;

    public AdminController(Client supabase)
    {
        _supabase = supabase;
    }

    private bool UsuarioLogadoEhAdmin()
    {
        var loggedUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

        return string.Equals(
            loggedUserRole,
            "admin",
            StringComparison.OrdinalIgnoreCase
        );
    }

    private string? ObterIdUsuarioLogado()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    [HttpGet("verificar")]
    public IActionResult VerificarAdmin()
    {
        var loggedUserIdStr = ObterIdUsuarioLogado();
        var loggedUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

        if (loggedUserIdStr == null)
        {
            return Unauthorized(new { erro = "Usuário não autenticado." });
        }

        return Ok(new
        {
            isAdmin = UsuarioLogadoEhAdmin(),
            role = loggedUserRole
        });
    }

    [HttpPut("usuarios/{userId:int}/promover-admin")]
    public async Task<IActionResult> PromoteUserToAdmin(int userId)
    {
        try
        {
            var loggedUserIdStr = ObterIdUsuarioLogado();

            if (loggedUserIdStr == null)
                return Unauthorized(new { erro = "Usuário não autenticado." });

            if (!UsuarioLogadoEhAdmin())
                return Forbid();

            var userResponse = await _supabase
                .From<ReddIF.Models.User>()
                .Where(u => u.UserId == userId)
                .Get();

            var user = userResponse.Models.FirstOrDefault();

            if (user == null)
                return NotFound(new { erro = "Usuário não encontrado." });

            if (user.Role == "admin")
            {
                return Ok(new
                {
                    message = "Usuário já é admin.",
                    userId = user.UserId,
                    name = user.Name,
                    email = user.Email,
                    role = user.Role
                });
            }

            user.Role = "admin";

            await _supabase
                .From<ReddIF.Models.User>()
                .Update(user);

            return Ok(new
            {
                message = "Usuário promovido a admin com sucesso.",
                userId = user.UserId,
                name = user.Name,
                email = user.Email,
                role = user.Role
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                erro = ex.Message,
                detalhe = ex.InnerException?.Message
            });
        }
    }

    [HttpPut("usuarios/{userId:int}/banir")]
    public async Task<IActionResult> BanirUsuario(int userId)
    {
        try
        {
            var loggedUserIdStr = ObterIdUsuarioLogado();

            if (loggedUserIdStr == null)
                return Unauthorized(new { erro = "Usuário não autenticado." });

            if (!UsuarioLogadoEhAdmin())
                return Forbid();

            if (int.TryParse(loggedUserIdStr, out var loggedUserId) && loggedUserId == userId)
            {
                return BadRequest(new { erro = "Você não pode banir a si mesmo." });
            }

            var userResponse = await _supabase
                .From<ReddIF.Models.User>()
                .Where(u => u.UserId == userId)
                .Get();

            var user = userResponse.Models.FirstOrDefault();

            if (user == null)
                return NotFound(new { erro = "Usuário não encontrado." });

            if (!user.Active)
            {
                return Ok(new
                {
                    message = "Usuário já está banido.",
                    userId = user.UserId,
                    name = user.Name,
                    email = user.Email,
                    active = user.Active
                });
            }

            user.Active = false;

            await _supabase
                .From<ReddIF.Models.User>()
                .Update(user);

            return Ok(new
            {
                message = "Usuário banido com sucesso.",
                userId = user.UserId,
                name = user.Name,
                email = user.Email,
                active = user.Active
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                erro = ex.Message,
                detalhe = ex.InnerException?.Message
            });
        }
    }

    [HttpDelete("posts/{postId:int}")]
    public async Task<IActionResult> RemoverPost(int postId)
    {
        try
        {
            var loggedUserIdStr = ObterIdUsuarioLogado();

            if (loggedUserIdStr == null)
                return Unauthorized(new { erro = "Usuário não autenticado." });

            if (!UsuarioLogadoEhAdmin())
                return Forbid();

            var postResponse = await _supabase
                .From<ReddIF.Models.Post>()
                .Where(p => p.PostId == postId)
                .Get();

            var post = postResponse.Models.FirstOrDefault();

            if (post == null)
                return NotFound(new { erro = "Post não encontrado." });

            await _supabase
                .From<ReddIF.Models.Post>()
                .Where(p => p.PostId == postId)
                .Delete();

            return Ok(new
            {
                message = "Post removido com sucesso.",
                postId = postId
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                erro = ex.Message,
                detalhe = ex.InnerException?.Message
            });
        }
    }
}