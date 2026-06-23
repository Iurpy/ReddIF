using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Supabase;
using Microsoft.AspNetCore.Mvc;
using ReddIF.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;

namespace ReddIF.Controllers;

[ApiController]
public class CommentsController: ControllerBase
{
private readonly Client _supabase;
    public CommentsController(Client supabase)
    {
        _supabase = supabase;
    } 

    [HttpPost("api/posts/{postId:int}/comments")]
    [Authorize]
    public async Task<IActionResult> CreateComment(int postId, [FromBody] CommentForm req)
    {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userIdStr == null)
                return Unauthorized();

            var userId = int.Parse(userIdStr);

            var comment = new Comment
            {
                Content = req.Content,
                PostId = postId,
                AuthorUserId = userId
            };

            var response = await _supabase
                .From<Comment>()
                .Insert(comment);

            var createdComment = response.Models.First();

            return Ok(new
            {
                createdComment.CommentId,
                createdComment.Content,
                createdComment.PostId,
                createdComment.AuthorUserId
            });
    }
        
    [HttpGet("api/posts/{postId:int}/comments")]
    public async Task<IActionResult> GetComments(int postId)
    {
        var response = await _supabase
            .From<Comment>()
            .Where(c => c.PostId == postId)
            .Get();

        return Ok(response.Models);
    }

    [HttpDelete("api/comments/{commentId:int}")]
    public async Task<IActionResult> DeleteComment(int commentId)
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userIdStr == null)
            return Unauthorized();

        var userId = int.Parse(userIdStr);

        var response = await _supabase
            .From<Comment>()
            .Where(c => c.CommentId == commentId && c.AuthorUserId == userId)
            .Get();

        var comment = response.Models.FirstOrDefault();
        
        if (comment == null)
            return NotFound(new { erro = "Comentário não encontrado ou você não tem permissão para deletar" });

        await _supabase
            .From<Comment>()
            .Where(c => c.CommentId == commentId)
            .Delete();

        return Ok(new { mensagem = "Comentário deletado com sucesso!" });
    }
}

public record Comment2Form([Required] string Content);