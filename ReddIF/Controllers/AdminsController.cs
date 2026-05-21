using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReddIF.Models;
using Supabase;
using System.Security.Claims;

namespace ReddIF.Controllers;

[ApiController]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly Client _supabase;

    public AdminController(Client supabase)
    {
        _supabase = supabase;
    }

    [HttpPut("usuarios/{userId:int}/promover-admin")]
    [Authorize]
    public async Task<IActionResult> PromoteUserToAdmin(int userId)
    {
        try
        {
            var loggedUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var loggedUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (loggedUserIdStr == null)
                return Unauthorized(new { erro = "Usuário não autenticado." });

            if (loggedUserRole != "admin")
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
}