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
                return Unauthorized(new { erro = "Usuário não autenticado." });

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

            var createdPost = response.Models.FirstOrDefault();

            if (createdPost == null)
                return BadRequest(new { erro = "Erro ao criar post." });

            var userResponse = await _supabase
                .From<ReddIF.Models.User>()
                .Where(u => u.UserId == userId)
                .Get();

            var author = userResponse.Models.FirstOrDefault();

            var communityResponse = await _supabase
                .From<Community>()
                .Where(c => c.CommunityId == communityId)
                .Get();

            var community = communityResponse.Models.FirstOrDefault();

            return Created($"/api/comunidades/{communityId}/posts/{createdPost.PostId}", new
            {
                postId = createdPost.PostId,
                title = createdPost.Title,
                content = createdPost.Content,
                communityId = createdPost.CommunityId,
                communityName = community != null ? community.Name : "Comunidade",
                userAuthorId = createdPost.UserAuthorId,
                authorName = author != null ? author.Name : "Usuário",
                createdAt = createdPost.CreatedAt,
                votes = 0,
                comments = 0,
                userVote = 0
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                erro = ex.Message,
                detalhe = ex.InnerException?.Message
            });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetPosts(int communityId)
    {
        try
        {
            var postsResponse = await _supabase
                .From<Post>()
                .Where(p => p.CommunityId == communityId)
                .Get();

            var communitiesResponse = await _supabase
                .From<Community>()
                .Get();

            var usersResponse = await _supabase
                .From<ReddIF.Models.User>()
                .Get();

            var commentsResponse = await _supabase
                .From<Comment>()
                .Get();

            var votesResponse = await _supabase
                .From<PostVote>()
                .Get();

            var posts = postsResponse.Models;
            var communities = communitiesResponse.Models;
            var users = usersResponse.Models;
            var comments = commentsResponse.Models;
            var votes = votesResponse.Models;

            var postsDaComunidade = posts
                .OrderByDescending(post => post.CreatedAt)
                .Select(post =>
                {
                    var community = communities
                        .FirstOrDefault(c => c.CommunityId == post.CommunityId);

                    var author = users
                        .FirstOrDefault(u => u.UserId == post.UserAuthorId);

                    var commentsCount = comments
                        .Count(c => c.PostId == post.PostId);

                    var votesCount = votes
                        .Where(v => v.PostId == post.PostId)
                        .Sum(v => v.VoteValue);

                    return new
                    {
                        postId = post.PostId,
                        title = post.Title,
                        content = post.Content,
                        communityId = post.CommunityId,
                        communityName = community != null ? community.Name : "Comunidade",
                        userAuthorId = post.UserAuthorId,
                        authorName = author != null ? author.Name : "Usuário",
                        createdAt = post.CreatedAt,
                        votes = votesCount,
                        comments = commentsCount,
                        userVote = 0
                    };
                })
                .ToList();

            return Ok(postsDaComunidade);
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

            var commentsResponse = await _supabase
                .From<Comment>()
                .Get();

            var votesResponse = await _supabase
                .From<PostVote>()
                .Get();

            var posts = postsResponse.Models;
            var communities = communitiesResponse.Models;
            var users = usersResponse.Models;
            var comments = commentsResponse.Models;
            var votes = votesResponse.Models;

            var feed = posts
                .Where(post => post.Active)
                .OrderByDescending(post => post.CreatedAt)
                .Select(post =>
                {
                    var community = communities
                        .FirstOrDefault(c => c.CommunityId == post.CommunityId);

                    var author = users
                        .FirstOrDefault(u => u.UserId == post.UserAuthorId);

                    var commentsCount = comments
                        .Count(c => c.PostId == post.PostId);

                    var votesCount = votes
                        .Where(v => v.PostId == post.PostId)
                        .Sum(v => v.VoteValue);

                    return new
                    {
                        postId = post.PostId,
                        title = post.Title,
                        content = post.Content,
                        communityId = post.CommunityId,
                        communityName = community != null ? community.Name : "Comunidade",
                        userAuthorId = post.UserAuthorId,
                        authorName = author != null ? author.Name : "Usuário",
                        createdAt = post.CreatedAt,
                        votes = votesCount,
                        comments = commentsCount,
                        userVote = 0
                    };
                })
                .ToList();

            return Ok(feed);
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

    [HttpGet("{postId:int}")]
    public async Task<IActionResult> GetPostById(int communityId, int postId)
    {
        try
        {
            var postResponse = await _supabase
                .From<Post>()
                .Where(p => p.PostId == postId)
                .Where(p => p.CommunityId == communityId)
                .Get();

            var post = postResponse.Models.FirstOrDefault();

            if (post == null)
                return NotFound(new { erro = $"Post {postId} não encontrado nessa comunidade." });

            var communityResponse = await _supabase
                .From<Community>()
                .Where(c => c.CommunityId == post.CommunityId)
                .Get();

            var community = communityResponse.Models.FirstOrDefault();

            var userResponse = await _supabase
                .From<ReddIF.Models.User>()
                .Where(u => u.UserId == post.UserAuthorId)
                .Get();

            var author = userResponse.Models.FirstOrDefault();

            var commentsResponse = await _supabase
                .From<Comment>()
                .Where(c => c.PostId == postId)
                .Get();

            var commentsCount = commentsResponse.Models.Count;

            var votesResponse = await _supabase
                .From<PostVote>()
                .Where(v => v.PostId == postId)
                .Get();

            var votesCount = votesResponse.Models
                .Sum(v => v.VoteValue);

            return Ok(new
            {
                postId = post.PostId,
                title = post.Title,
                content = post.Content,
                communityId = post.CommunityId,
                communityName = community != null ? community.Name : "Comunidade",
                userAuthorId = post.UserAuthorId,
                authorName = author != null ? author.Name : "Usuário",
                createdAt = post.CreatedAt,
                votes = votesCount,
                comments = commentsCount,
                userVote = 0
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

    [HttpDelete("{postId:int}")]
    [Authorize]
    public async Task<IActionResult> DeletePost(int communityId, int postId)
    {
        try
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userIdStr == null)
                return Unauthorized(new { erro = "Usuário não autenticado." });

            var userId = int.Parse(userIdStr);

            var response = await _supabase
                .From<Post>()
                .Where(p => p.PostId == postId)
                .Where(p => p.CommunityId == communityId)
                .Get();

            var post = response.Models.FirstOrDefault();

            if (post == null)
                return NotFound(new { erro = "Post não encontrado." });

            if (post.UserAuthorId != userId)
                return Forbid();

            await _supabase
                .From<Post>()
                .Where(p => p.PostId == postId)
                .Where(p => p.CommunityId == communityId)
                .Delete();

            return Ok(new { message = "Post deletado com sucesso." });
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

    [HttpGet("{postId:int}/comentarios")]
    public async Task<IActionResult> GetComments(int communityId, int postId)
    {
        try
        {
            var postResponse = await _supabase
                .From<Post>()
                .Where(p => p.PostId == postId)
                .Where(p => p.CommunityId == communityId)
                .Get();

            var post = postResponse.Models.FirstOrDefault();

            if (post == null)
                return NotFound(new { erro = "Post não encontrado nessa comunidade." });

            var commentsResponse = await _supabase
                .From<Comment>()
                .Where(c => c.PostId == postId)
                .Get();

            var usersResponse = await _supabase
                .From<ReddIF.Models.User>()
                .Get();

            var users = usersResponse.Models;

            var comments = commentsResponse.Models
                .OrderByDescending(comment => comment.CreatedAt)
                .Select(comment =>
                {
                    var author = users
                        .FirstOrDefault(u => u.UserId == comment.AuthorUserId);

                    return new
                    {
                        commentId = comment.CommentId,
                        postId = comment.PostId,
                        authorUserId = comment.AuthorUserId,
                        authorName = author != null ? author.Name : "Usuário",
                        content = comment.Content,
                        createdAt = comment.CreatedAt
                    };
                })
                .ToList();

            return Ok(comments);
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



    [HttpGet("~/api/posts/recentes")]
public async Task<IActionResult> GetRecentPosts()
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

        var commentsResponse = await _supabase
            .From<Comment>()
            .Get();

        var votesResponse = await _supabase
            .From<PostVote>()
            .Get();

        var posts = postsResponse.Models;
        var communities = communitiesResponse.Models;
        var users = usersResponse.Models;
        var comments = commentsResponse.Models;
        var votes = votesResponse.Models;

        var limite = DateTime.Now.AddHours(-24);

        var recentes = posts
            .Where(post => post.CreatedAt >= limite)
            .OrderByDescending(post => post.CreatedAt)
            .Select(post =>
            {
                var community = communities
                    .FirstOrDefault(c => c.CommunityId == post.CommunityId);

                var author = users
                    .FirstOrDefault(u => u.UserId == post.UserAuthorId);

                var commentsCount = comments
                    .Count(c => c.PostId == post.PostId);

                var votesCount = votes
                    .Where(v => v.PostId == post.PostId)
                    .Sum(v => v.VoteValue);

                return new
                {
                    postId = post.PostId,
                    title = post.Title,
                    content = post.Content,
                    communityId = post.CommunityId,
                    communityName = community != null ? community.Name : "Comunidade",
                    userAuthorId = post.UserAuthorId,
                    authorName = author != null ? author.Name : "Usuário",
                    createdAt = post.CreatedAt,
                    votes = votesCount,
                    comments = commentsCount,
                    userVote = 0
                };
            })
            .ToList();

        return Ok(recentes);
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

    [HttpPost("{postId:int}/comentarios")]
    [Authorize]
    public async Task<IActionResult> CreateComment(int communityId, int postId, [FromBody] CommentForm req)
    {
        try
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userIdStr == null)
                return Unauthorized(new { erro = "Usuário não autenticado." });

            var userId = int.Parse(userIdStr);

            if (string.IsNullOrWhiteSpace(req.Content))
                return BadRequest(new { erro = "O comentário não pode estar vazio." });

            var postResponse = await _supabase
                .From<Post>()
                .Where(p => p.PostId == postId)
                .Where(p => p.CommunityId == communityId)
                .Get();

            var post = postResponse.Models.FirstOrDefault();

            if (post == null)
                return NotFound(new { erro = "Post não encontrado nessa comunidade." });

            var comment = new Comment
            {
                PostId = postId,
                AuthorUserId = userId,
                Content = req.Content,
                CreatedAt = DateTime.Now
            };

            var response = await _supabase
                .From<Comment>()
                .Insert(comment);

            var createdComment = response.Models.FirstOrDefault();

            if (createdComment == null)
                return BadRequest(new { erro = "Erro ao criar comentário." });

            var userResponse = await _supabase
                .From<ReddIF.Models.User>()
                .Where(u => u.UserId == userId)
                .Get();

            var author = userResponse.Models.FirstOrDefault();

            return Created(
                $"/api/comunidades/{communityId}/posts/{postId}/comentarios/{createdComment.CommentId}",
                new
                {
                    commentId = createdComment.CommentId,
                    postId = createdComment.PostId,
                    authorUserId = createdComment.AuthorUserId,
                    authorName = author != null ? author.Name : "Usuário",
                    content = createdComment.Content,
                    createdAt = createdComment.CreatedAt
                }
            );
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

    [HttpPost("{postId:int}/votar")]
    [Authorize]
    public async Task<IActionResult> VotePost(int communityId, int postId, [FromBody] VoteForm req)
    {
        try
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userIdStr == null)
                return Unauthorized(new { erro = "Usuário não autenticado." });

            var userId = int.Parse(userIdStr);

            if (req.VoteValue != 1 && req.VoteValue != -1)
                return BadRequest(new { erro = "Voto inválido." });

            var postResponse = await _supabase
                .From<Post>()
                .Where(p => p.PostId == postId)
                .Where(p => p.CommunityId == communityId)
                .Get();

            var post = postResponse.Models.FirstOrDefault();

            if (post == null)
                return NotFound(new { erro = "Post não encontrado nessa comunidade." });

            var existingVoteResponse = await _supabase
                .From<PostVote>()
                .Where(v => v.PostId == postId)
                .Where(v => v.UserId == userId)
                .Get();

            var existingVote = existingVoteResponse.Models.FirstOrDefault();

            var oldVoteValue = existingVote?.VoteValue ?? 0;
            var newVoteValue = 0;

            if (existingVote == null)
            {
                newVoteValue = req.VoteValue;

                var vote = new PostVote
                {
                    PostId = postId,
                    UserId = userId,
                    VoteValue = req.VoteValue,
                    CreatedAt = DateTime.Now
                };

                await _supabase
                    .From<PostVote>()
                    .Insert(vote);
            }
            else if (existingVote.VoteValue == req.VoteValue)
            {
                newVoteValue = 0;

                await _supabase
                    .From<PostVote>()
                    .Where(v => v.PostId == postId)
                    .Where(v => v.UserId == userId)
                    .Delete();
            }
            else
            {
                newVoteValue = req.VoteValue;

                existingVote.VoteValue = req.VoteValue;

                await _supabase
                    .From<PostVote>()
                    .Update(existingVote);
            }

            var karmaDelta = newVoteValue - oldVoteValue;

            if (post.UserAuthorId != userId && karmaDelta != 0)
            {
                var authorResponse = await _supabase
                    .From<ReddIF.Models.User>()
                    .Where(u => u.UserId == post.UserAuthorId)
                    .Get();

                var author = authorResponse.Models.FirstOrDefault();

                if (author != null)
                {
                    author.Karma += karmaDelta;

                    await _supabase
                        .From<ReddIF.Models.User>()
                        .Update(author);
                }
            }

            var votesResponse = await _supabase
                .From<PostVote>()
                .Where(v => v.PostId == postId)
                .Get();

            var votes = votesResponse.Models
                .Sum(v => v.VoteValue);

            var currentUserVote = votesResponse.Models
                .FirstOrDefault(v => v.UserId == userId)?.VoteValue ?? 0;

            return Ok(new
            {
                postId,
                votes,
                userVote = currentUserVote,
                karmaDelta
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

public record PostForm([Required] string Title, [Required] string Content);
public record CommentForm([Required] string Content);
public record VoteForm([Required] int VoteValue);