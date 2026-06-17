using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReddIF.Models;
using Supabase;

namespace ReddIF.Controllers;

[ApiController]
[Route("api/search")]
[Authorize]
public class SearchController : ControllerBase
{
    private readonly Client _supabase;

    public SearchController(Client supabase)
    {
        _supabase = supabase;
    }

    // Retorna sugestões para a barra de pesquisa da Home.
    [HttpGet("suggestions")]
    public async Task<IActionResult> GetSuggestions([FromQuery] string q)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(q))
                return BadRequest(new { erro = "Digite algo para pesquisar." });

            q = q.Trim().ToLower();

            var usersResponse = await _supabase
                .From<User>()
                .Get();

            var postsResponse = await _supabase
                .From<Post>()
                .Get();

            var communitiesResponse = await _supabase
                .From<Community>()
                .Get();

            var users = usersResponse.Models
                .Where(u =>
                    u.Active &&
                    (
                        u.Name.ToLower().Contains(q) ||
                        u.Email.ToLower().Contains(q)
                    ))
                .Take(3)
                .Select(u => new
                {
                    userId = u.UserId,
                    name = u.Name,
                    email = u.Email,
                    karma = u.Karma
                })
                .ToList();

            var posts = postsResponse.Models
                .Where(p =>
                    p.Active &&
                    (
                        p.Title.ToLower().Contains(q) ||
                        p.Content.ToLower().Contains(q)
                    ))
                .Take(3)
                .Select(p => new
                {
                    postId = p.PostId,
                    title = p.Title,
                    content = p.Content,
                    communityId = p.CommunityId,
                    userAuthorId = p.UserAuthorId,
                    createdAt = p.CreatedAt
                })
                .ToList();

            var communities = communitiesResponse.Models
                .Where(c =>
                    c.Name.ToLower().Contains(q))
                .Take(3)
                .Select(c => new
                {
                    communityId = c.CommunityId,
                    name = c.Name
                })
                .ToList();

            return Ok(new
            {
                query = q,

                options = new[]
                {
                    $"Pesquisar por \"{q}\" em usuários",
                    $"Pesquisar por \"{q}\" em posts",
                    $"Pesquisar por \"{q}\" em comunidades"
                },

                users,
                posts,
                communities
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

    // Pesquisa usuários pelo nome ou email.
    [HttpGet("usuarios")]
    public async Task<IActionResult> SearchUsers([FromQuery] string q)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(q))
                return BadRequest(new { erro = "Digite algo para pesquisar." });

            q = q.Trim().ToLower();

            var response = await _supabase
                .From<User>()
                .Get();

            var users = response.Models
                .Where(u =>
                    u.Active &&
                    (
                        u.Name.ToLower().Contains(q) ||
                        u.Email.ToLower().Contains(q)
                    ))
                .Select(u => new
                {
                    userId = u.UserId,
                    name = u.Name,
                    email = u.Email,
                    karma = u.Karma,
                    createdAt = u.CreateTime
                })
                .ToList();

            return Ok(users);
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

    // Pesquisa posts pelo título ou conteúdo.
    [HttpGet("posts")]
    public async Task<IActionResult> SearchPosts([FromQuery] string q)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(q))
                return BadRequest(new { erro = "Digite algo para pesquisar." });

            q = q.Trim().ToLower();

            var response = await _supabase
                .From<Post>()
                .Get();

            var posts = response.Models
                .Where(p =>
                    p.Active &&
                    (
                        p.Title.ToLower().Contains(q) ||
                        p.Content.ToLower().Contains(q)
                    ))
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new
                {
                    postId = p.PostId,
                    title = p.Title,
                    content = p.Content,
                    communityId = p.CommunityId,
                    userAuthorId = p.UserAuthorId,
                    createdAt = p.CreatedAt
                })
                .ToList();

            return Ok(posts);
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

    // Pesquisa comunidades pelo nome.
    [HttpGet("comunidades")]
    public async Task<IActionResult> SearchCommunities([FromQuery] string q)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(q))
                return BadRequest(new { erro = "Digite algo para pesquisar." });

            q = q.Trim().ToLower();

            var response = await _supabase
                .From<Community>()
                .Get();

            var communities = response.Models
                .Where(c => c.Name.ToLower().Contains(q))
                .Select(c => new
                {
                    communityId = c.CommunityId,
                    name = c.Name
                })
                .ToList();

            return Ok(communities);
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