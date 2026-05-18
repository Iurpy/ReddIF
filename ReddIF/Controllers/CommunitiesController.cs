using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReddIF.Models;
using Supabase;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;

namespace ReddIF.Controllers;

[ApiController]
[Route("api/comunidades")]
public class CommunitiesController : ControllerBase
{
    private readonly Client _supabase;

    public CommunitiesController(Client supabase)
    {
        _supabase = supabase;
    }
 
[HttpPost]
[Authorize]
public async Task<IActionResult> Create([FromBody] CommunityForm req)
{
    try
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (userIdStr == null) return Unauthorized();
        
        var userId = int.Parse(userIdStr);

        var NameExists = await _supabase
            .From<Community>()
            .Where(c => c.Name == req.Name)
            .Get();

        if (NameExists.Models.Any())
            return BadRequest(new { erro = "Já existe uma comunidade com esse nome" });    
        
        var newCommunity = new Community
        {
            Name = req.Name,
            Description = req.Description ?? string.Empty,
            OwnerId = userId,
            CreatedAt = DateTime.Now
        };

        var response = await _supabase
            .From<Community>()
            .Insert(newCommunity);
        
        var community = response.Models.FirstOrDefault();

        return Ok(new
        {
            message = "Comunidade criada com sucesso!",
            community?.CommunityId,
            community?.Name,
            community?.Description
        });
    }
    
    catch (Exception ex)
    {
        return StatusCode(500, new { erro = ex.Message });
    }
}
   
[HttpGet]
public async Task<IActionResult> GetAll()
{
    try
    {
        var response = await _supabase.From<Community>().Get();

        var communities = response.Models.Select(c => new
        {
            c.CommunityId,
            c.Name,
            c.Description,
            c.OwnerId,
            c.CreatedAt
        });

        return Ok(new { total = communities.Count(), communities });
    }
    
    catch (Exception ex)
    {
        return StatusCode(500, new { erro = ex.Message });
    }
}

[HttpGet("{communityId:int}")]
public async Task<ActionResult<Community>> GetCommunity(int communityId)
{
    var response = await _supabase
        .From<Community>()
        .Where(c => c.CommunityId == communityId)
        .Get();
    
    var community = response.Models.FirstOrDefault();
    
    return Ok(community);
}

[HttpGet("{name}")]
public async Task<IActionResult> GetByName(string name)
{
    try
    {
        var response = await _supabase
            .From<Community>()
            .Where(c => c.Name == name)
            .Get();

        var community = response.Models.FirstOrDefault();

        if (community == null)
            return NotFound(new { erro = "Comunidade não encontrada" });

        return Ok(new
        {
            community.CommunityId,
            community.Name,
            community.Description,
            community.OwnerId,
            community.CreatedAt
        });
    }
    
    catch (Exception ex)
    {
        return StatusCode(500, new { erro = ex.Message });
    }
}
 
[HttpPut("{id}")]
[Authorize]
public async Task<IActionResult> Update(int id, [FromBody] UpdateCommunityForm req)
{
    try
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
        
        if (userIdStr == null) return Unauthorized();
        
        var userId = int.Parse(userIdStr);

        var response = await _supabase
            .From<Community>()
            .Where(c => c.CommunityId == id)
            .Get();

        var community = response.Models.FirstOrDefault();
        
        if (community == null)
            return NotFound(new { erro = "Comunidade não encontrada" });

        if (community.OwnerId != userId && userRole != "admin")
            return Forbid();

        if (!string.IsNullOrEmpty(req.Name))
            community.Name = req.Name;

        if (!string.IsNullOrEmpty(req.Description))
            community.Description = req.Description;

        await _supabase
            .From<Community>()
            .Update(community);

        return Ok(new { message = "Comunidade atualizada com sucesso!" });
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { erro = ex.Message });
    }
}

[HttpDelete("{id}")]
[Authorize]
public async Task<IActionResult> Delete(int id)
{
    try
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
        
        if (userIdStr == null) return Unauthorized();
        
        var userId = int.Parse(userIdStr);

        var response = await _supabase
            .From<Community>()
            .Where(c => c.CommunityId == id)
            .Get();

        var community = response.Models.FirstOrDefault();
            
        if (community == null)
            return NotFound(new { erro = "Comunidade não encontrada" });

        if (community.OwnerId != userId && userRole != "admin")
            return Forbid();

        await _supabase
            .From<Community>()
            .Where(c => c.CommunityId == id)
            .Delete();

        return Ok(new { message = "Comunidade deletada com sucesso!" });
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { erro = ex.Message });
    }
}
}

public record CommunityForm([Required] string Name, [Required] string? Description);
public record UpdateCommunityForm([Required] string? Name, [Required] string? Description);