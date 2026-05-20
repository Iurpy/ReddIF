using Microsoft.AspNetCore.Mvc;
using ReddIF.Models;
using Supabase;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;

namespace ReddIF.Controllers;

[ApiController]
[Route("api/comunidades/{communityId:int}/posts")]
public class PostsController : ControllerBase
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
                UserAuthorId = userId,
                CreatedAt = DateTime.Now
            };

            var response = await _supabase
                .From<Post>()
                .Insert(post);

            var createdPost = response.Models.First();

            return Created($"/api/comunidades/{communityId}/posts/{createdPost.PostId}", new
            {
                postId = createdPost.PostId,
                title = createdPost.Title,
                content = createdPost.Content,
                communityId = createdPost.CommunityId,
                userAuthorId = createdPost.UserAuthorId,
                createdAt = createdPost.CreatedAt
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

            var posts = response.Models
                .OrderByDescending(post => post.CreatedAt)
                .Select(post => new
                {
                    postId = post.PostId,
                    title = post.Title,
                    content = post.Content,
                    communityId = post.CommunityId,
                    userAuthorId = post.UserAuthorId,
                    createdAt = post.CreatedAt
                })
                .ToList();

            return Ok(posts);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { erro = ex.Message });
        }
    }

    [HttpGet("~/api/posts/feed")]
    public async Task<IActionResult> GetFeedPosts()
    {
        try
        {
            var postsResponse = await _supabase
                .From<Post>()
                .Get();

            var communitiesResponse = await _supabase
                .From<Community>()
                .Get();

            var usersResponse = await _supabase
                .From<ReddIF.Models.User>()
                .Get();

            var posts = postsResponse.Models;
            var communities = communitiesResponse.Models;
            var users = usersResponse.Models;

            var feed = posts
                .OrderByDescending(post => post.CreatedAt)
                .Select(post =>
                {
                    var community = communities
                        .FirstOrDefault(c => c.CommunityId == post.CommunityId);

                    var author = users
                        .FirstOrDefault(u => u.UserId == post.UserAuthorId);

                    return new
                    {
                        postId = post.PostId,
                        title = post.Title,
                        content = post.Content,
                        communityId = post.CommunityId,
                        communityName = community != null ? community.Name : "Comunidade",
                        userAuthorId = post.UserAuthorId,
                        authorName = author != null ? author.Name : "Usuário",
                        createdAt = post.CreatedAt
                    };
                })
                .ToList();

            return Ok(feed);
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

            if (userIdStr == null)
                return Unauthorized();

            var userId = int.Parse(userIdStr);

            var response = await _supabase
                .From<Post>()
                .Where(p => p.PostId == postId)
                .Get();

            var post = response.Models.FirstOrDefault();

            if (post == null)
                return NotFound(new { erro = "Post não encontrado" });

            if (post.UserAuthorId != userId)
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