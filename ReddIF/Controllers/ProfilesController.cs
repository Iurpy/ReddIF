using Microsoft.AspNetCore.Mvc;
using ReddIF.Models;
using Supabase;

namespace ReddIF.Controllers;

[ApiController]
[Route("api/perfil")]
public class ProfileController : ControllerBase
{
    private readonly Client _supabase;

    public ProfileController(Client supabase)
    {
        _supabase = supabase;
    }

    [HttpGet("{userId:int}")]
    public async Task<IActionResult> GetProfile(int userId)
    {
        try
        {
            var userResponse = await _supabase
                .From<ReddIF.Models.User>()
                .Where(u => u.UserId == userId)
                .Get();

            var user = userResponse.Models.FirstOrDefault();

            if (user == null)
                return NotFound(new { erro = "Usuário não encontrado." });

            var postsResponse = await _supabase
                .From<Post>()
                .Where(p => p.UserAuthorId == userId)
                .Get();

            var communitiesResponse = await _supabase
                .From<Community>()
                .Get();

            var membersResponse = await _supabase
                .From<CommunityMember>()
                .Where(m => m.UserId == userId)
                .Get();

            var commentsResponse = await _supabase
                .From<Comment>()
                .Get();

            var allCommunities = communitiesResponse.Models;
            var userPosts = postsResponse.Models;
            var memberships = membersResponse.Models;
            var comments = commentsResponse.Models;

            var userCommunityIds = memberships
                .Select(m => m.CommunityId)
                .ToList();

            var userCommunities = allCommunities
                .Where(c => userCommunityIds.Contains(c.CommunityId))
                .Select(c => new
                {
                    communityId = c.CommunityId,
                    name = c.Name,
                    description = c.Description,
                    ownerId = c.OwnerId,
                    createdAt = c.CreatedAt
                })
                .ToList();

            var posts = userPosts
                .OrderByDescending(p => p.CreatedAt)
                .Select(post =>
                {
                    var community = allCommunities
                        .FirstOrDefault(c => c.CommunityId == post.CommunityId);

                    var commentsCount = comments
                        .Count(c => c.PostId == post.PostId);

                    return new
                    {
                        postId = post.PostId,
                        title = post.Title,
                        content = post.Content,
                        communityId = post.CommunityId,
                        communityName = community != null ? community.Name : "Comunidade",
                        userAuthorId = post.UserAuthorId,
                        authorName = user.Name,
                        createdAt = post.CreatedAt,
                        votes = 0,
                        comments = commentsCount
                    };
                })
                .ToList();

            return Ok(new
            {
                user = new
                {
                    userId = user.UserId,
                    name = user.Name,
                    email = user.Email,
                    karma = user.Karma,
                    active = user.Active,
                    role = user.Role,
                    createdAt = user.CreateTime
                },
                posts,
                communities = userCommunities,
                stats = new
                {
                    totalPosts = posts.Count,
                    totalCommunities = userCommunities.Count,
                    karma = user.Karma
                }
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