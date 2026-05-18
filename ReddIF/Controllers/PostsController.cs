using Microsoft.AspNetCore.Mvc;
using ReddIF.Models;
using Supabase;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;

namespace ReddIF.Controllers;

[ApiController]
[Route("api/comunidades/{communityId:int}/posts")]
public class PostsController: ControllerBase
{
    private readonly Client _supabase;

    public PostsController(Client supabase)
    {
        _supabase = supabase;
    }

[HttpPost]
[Authorize]
public async Task<IActionResult> CreatePost(int communityId, [FromBody] PostForm req)
{
    try
    {    
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userIdStr == null)
            return Unauthorized();

        var userId = int.Parse(userIdStr);

        var post = new Post
        {
            Title = req.Title,
            Content = req.Content,
            CommunityId = communityId,
            UserAutorId = userId
        };

        var response = await _supabase
            .From<Post>()
            .Insert(post);

        var createdPost = response.Models.First();

        return Created($"/api/comunidades/{communityId}/posts/{createdPost.PostId}", new
        {
            createdPost.PostId,
            createdPost.Title,
            createdPost.Content,
            createdPost.CommunityId,
            createdPost.UserAutorId
        });
    }

    catch (Exception ex)
    {
        return BadRequest(new { erro = ex.Message });
    }    
}

[HttpGet]
public async Task<IActionResult> GetPosts(int communityId)
{
    try
    {    
        var response = await _supabase
            .From<Post>()
            .Where(p => p.CommunityId == communityId)
            .Get();

        return Ok(response.Models);
    }

    catch (Exception ex)
    {
        return StatusCode(500, new { erro = ex.Message });
    }
}


[HttpDelete("{postId:int}")]
[Authorize]
public async Task<IActionResult> DeletePost(int communityId, int postId)
{
    try
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (userIdStr == null) return Unauthorized();
        
        var userId = int.Parse(userIdStr);

        var response = await _supabase
            .From<Post>()
            .Where(p => p.PostId == postId)
            .Get();

        var post = response.Models.FirstOrDefault();
        
        if (post == null) return NotFound(new { erro = "Post não encontrado" });

        if (post.UserAutorId != userId)
            return Forbid();

        await _supabase
            .From<Post>()
            .Where(p => p.PostId == postId)
            .Delete();

        return Ok(new { message = "Post deletado com sucesso" });
    }

    catch (Exception ex)
    {
        return StatusCode(500, new { erro = ex.Message });
    }
}
}
public record PostForm([Required] string Title, [Required] string Content);

