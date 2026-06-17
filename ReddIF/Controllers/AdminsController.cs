using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReddIF.Models;
using Supabase;

namespace ReddIF.Controllers;

[ApiController]
[Route("api/admin")]
<<<<<<< HEAD
[Authorize]
=======
[Authorize(Roles = "admin")]
>>>>>>> bde61456ec030f41881981a97a1350c00cadf4c9
public class AdminController : ControllerBase
{
    private readonly Client _supabase;

    public AdminController(Client supabase)
    {
        _supabase = supabase;
    }

<<<<<<< HEAD
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

=======
    // Lista todos os usuários cadastrados.
    [HttpGet("usuarios")]
    public async Task<IActionResult> GetAllUsers()
    {
        try
        {
            var usersResponse = await _supabase
                .From<User>()
                .Range(0, 100)
                .Get();

            return Ok(usersResponse.Models.Select(u => new
            {
                userId = u.UserId,
                name = u.Name,
                email = u.Email,
                role = u.Role,
                karma = u.Karma,
                active = u.Active,
                createdAt = u.CreateTime
            }));
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

    // Busca um usuário específico pelo ID.
    [HttpGet("usuarios/{userId:int}")]
    public async Task<IActionResult> GetUserById(int userId)
    {
        try
        {
            var user = await GetUser(userId);

            if (user == null)
                return NotFound(new { erro = "Usuário não encontrado." });

            return Ok(new
            {
                userId = user.UserId,
                name = user.Name,
                email = user.Email,
                role = user.Role,
                karma = user.Karma,
                active = user.Active,
                createdAt = user.CreateTime
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

    // Promove um usuário comum para administrador.
>>>>>>> bde61456ec030f41881981a97a1350c00cadf4c9
    [HttpPut("usuarios/{userId:int}/promover-admin")]
    public async Task<IActionResult> PromoteUserToAdmin(int userId)
    {
        try
        {
<<<<<<< HEAD
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
=======
            var user = await GetUser(userId);
>>>>>>> bde61456ec030f41881981a97a1350c00cadf4c9

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
                .From<User>()
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

<<<<<<< HEAD
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
=======

    // Desativa um usuário, funcionando como banimento.
    [HttpPut("usuarios/{userId:int}/banir")]
    public async Task<IActionResult> BanUser(int userId)
    {
        try
        {
            var user = await GetUser(userId);
>>>>>>> bde61456ec030f41881981a97a1350c00cadf4c9

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
<<<<<<< HEAD
                .From<ReddIF.Models.User>()
=======
                .From<User>()
>>>>>>> bde61456ec030f41881981a97a1350c00cadf4c9
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

<<<<<<< HEAD
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
=======
    // Reativa um usuário banido.
    [HttpPut("usuarios/{userId:int}/desbanir")]
    public async Task<IActionResult> UnbanUser(int userId)
    {
        try
        {
            var user = await GetUser(userId);

            if (user == null)
                return NotFound(new { erro = "Usuário não encontrado." });

            if (user.Active)
            {
                return Ok(new
                {
                    message = "Usuário já está ativo.",
                    userId = user.UserId,
                    name = user.Name,
                    email = user.Email,
                    active = user.Active
                });
            }

            user.Active = true;

            await _supabase
                .From<User>()
                .Update(user);

            return Ok(new
            {
                message = "Usuário desbanido com sucesso.",
                userId = user.UserId,
                name = user.Name,
                email = user.Email,
                active = user.Active
>>>>>>> bde61456ec030f41881981a97a1350c00cadf4c9
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
<<<<<<< HEAD
=======

    // Retorna os dados principais para o dashboard do painel administrativo.
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        try
        {
            var usersResponse = await _supabase
                .From<User>()
                .Get();
            var communitiesResponse = await _supabase
                .From<Community>()
                .Get();
            var postsResponse = await _supabase
                .From<Post>()
                .Get();

            var users = usersResponse.Models;
            var communities = communitiesResponse.Models;
            var posts = postsResponse.Models;

            var today = DateTime.Today;
            var last7Days = today.AddDays(-7);

            return Ok(new
            {
                totalUsers = users.Count,
                totalAdmins = users.Count(u => u.Role == "admin"),
                totalCommonUsers = users.Count(u => u.Role == "user"),
                totalActiveUsers = users.Count(u => u.Active),
                totalBannedUsers = users.Count(u => !u.Active),
                usersCreatedToday = users.Count(u => u.CreateTime.Date == today),
                usersCreatedLast7Days = users.Count(u => u.CreateTime.Date >= last7Days),
                totalCommunities = communities.Count,
                totalPosts = posts.Count,
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

    // Remove um post do feed (soft delete).
    [HttpPut("posts/{postId:int}/remover")]
    public async Task<IActionResult> RemovePost(int postId)
    {
        try
        {
            var postResponse = await _supabase
                .From<Post>()
                .Where(p => p.PostId == postId)
                .Get();

            var post = postResponse.Models.FirstOrDefault();

            if (post == null)
                return NotFound(new { erro = "Post não encontrado." });

            if (!post.Active)
            {
                return Ok(new
                {
                    message = "O post já foi removido.",
                    postId = post.PostId
                });
            }

            post.Active = false;

            await _supabase
                .From<Post>()
                .Update(post);

            return Ok(new
            {
                message = "Post removido com sucesso.",
                postId = post.PostId
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

    // Função auxiliar para buscar um usuário pelo ID.
    private async Task<User?> GetUser(int userId)
    {
        var response = await _supabase
            .From<User>()
            .Where(u => u.UserId == userId)
            .Get();

        return response.Models.FirstOrDefault();
    }
>>>>>>> bde61456ec030f41881981a97a1350c00cadf4c9
}