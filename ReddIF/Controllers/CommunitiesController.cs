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

            if (userIdStr == null)
                return Unauthorized();

            var userId = int.Parse(userIdStr);

            var nameExists = await _supabase
                .From<Community>()
                .Where(c => c.Name == req.Name)
                .Get();

            if (nameExists.Models.Any())
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

            if (community == null)
                return BadRequest(new { erro = "Erro ao criar comunidade." });

            var member = new CommunityMember
            {
                CommunityId = community.CommunityId,
                UserId = userId,
                JoinedAt = DateTime.Now
            };

            await _supabase
                .From<CommunityMember>()
                .Insert(member);

            return Ok(new
            {
                message = "Comunidade criada com sucesso!",
                communityId = community.CommunityId,
                name = community.Name,
                description = community.Description,
                ownerId = community.OwnerId,
                createdAt = community.CreatedAt
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
            var response = await _supabase
                .From<Community>()
                .Get();

            var membersResponse = await _supabase
                .From<CommunityMember>()
                .Get();

            var members = membersResponse.Models;

            var communities = response.Models.Select(c => new
            {
                c.CommunityId,
                c.Name,
                c.Description,
                c.OwnerId,
                c.CreatedAt,
                MembersCount = members.Count(m => m.CommunityId == c.CommunityId)
            });

            return Ok(new
            {
                total = communities.Count(),
                comunidades = communities
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { erro = ex.Message });
        }
    }

    [HttpGet("{communityId:int}")]
    public async Task<IActionResult> GetCommunity(int communityId)
    {
        try
        {
            var response = await _supabase
                .From<Community>()
                .Where(c => c.CommunityId == communityId)
                .Get();

            var community = response.Models.FirstOrDefault();

            if (community == null)
                return NotFound(new { erro = "Comunidade não encontrada" });

            var membersResponse = await _supabase
                .From<CommunityMember>()
                .Where(m => m.CommunityId == communityId)
                .Get();

            var membersCount = membersResponse.Models.Count;

            return Ok(new
            {
                community.CommunityId,
                community.Name,
                community.Description,
                community.OwnerId,
                community.CreatedAt,
                MembersCount = membersCount
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { erro = ex.Message });
        }
    }

    [HttpGet("nome/{name}")]
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

            var membersResponse = await _supabase
                .From<CommunityMember>()
                .Where(m => m.CommunityId == community.CommunityId)
                .Get();

            var membersCount = membersResponse.Models.Count;

            return Ok(new
            {
                community.CommunityId,
                community.Name,
                community.Description,
                community.OwnerId,
                community.CreatedAt,
                MembersCount = membersCount
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { erro = ex.Message });
        }
    }

    [HttpPost("{communityId:int}/entrar")]
[Authorize]
public async Task<IActionResult> JoinCommunity(int communityId)
{
    try
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userIdStr == null)
            return Unauthorized(new { erro = "Usuário não autenticado." });

        var userId = int.Parse(userIdStr);

        var communityResponse = await _supabase
            .From<Community>()
            .Where(c => c.CommunityId == communityId)
            .Get();

        var community = communityResponse.Models.FirstOrDefault();

        if (community == null)
            return NotFound(new { erro = "Comunidade não encontrada." });

        var existingMemberResponse = await _supabase
            .From<CommunityMember>()
            .Where(m => m.CommunityId == communityId)
            .Where(m => m.UserId == userId)
            .Get();

        if (existingMemberResponse.Models.Any())
        {
            return Ok(new
            {
                message = "Você já participa dessa comunidade.",
                communityId,
                userId,
                isMember = true
            });
        }

        var member = new CommunityMember
        {
            CommunityId = communityId,
            UserId = userId,
            JoinedAt = DateTime.Now
        };

        var insertResponse = await _supabase
            .From<CommunityMember>()
            .Insert(member);

        return Ok(new
        {
            message = "Você entrou na comunidade.",
            communityId,
            userId,
            isMember = true
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

    [HttpDelete("{communityId:int}/sair")]
    [Authorize]
    public async Task<IActionResult> LeaveCommunity(int communityId)
    {
        try
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userIdStr == null)
                return Unauthorized(new { erro = "Usuário não autenticado." });

            var userId = int.Parse(userIdStr);

            var memberResponse = await _supabase
                .From<CommunityMember>()
                .Where(m => m.CommunityId == communityId)
                .Where(m => m.UserId == userId)
                .Get();

            var member = memberResponse.Models.FirstOrDefault();

            if (member == null)
                return NotFound(new { erro = "Você não participa dessa comunidade." });

            await _supabase
                .From<CommunityMember>()
                .Where(m => m.CommunityId == communityId)
                .Where(m => m.UserId == userId)
                .Delete();

            return Ok(new
            {
                message = "Você saiu da comunidade.",
                communityId,
                userId,
                isMember = false
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { erro = ex.Message });
        }
    }

    [HttpGet("{communityId:int}/membro")]
    [Authorize]
    public async Task<IActionResult> CheckMembership(int communityId)
    {
        try
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userIdStr == null)
                return Unauthorized(new { erro = "Usuário não autenticado." });

            var userId = int.Parse(userIdStr);

            var memberResponse = await _supabase
                .From<CommunityMember>()
                .Where(m => m.CommunityId == communityId)
                .Where(m => m.UserId == userId)
                .Get();

            var isMember = memberResponse.Models.Any();

            return Ok(new
            {
                communityId,
                userId,
                isMember
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { erro = ex.Message });
        }
    }

    [HttpGet("minhas")]
[Authorize]
public async Task<IActionResult> GetMyCommunities()
{
    try
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userIdStr == null)
            return Unauthorized(new { erro = "Usuário não autenticado." });

        var userId = int.Parse(userIdStr);

        var membersResponse = await _supabase
            .From<CommunityMember>()
            .Where(m => m.UserId == userId)
            .Get();

        var memberships = membersResponse.Models;

        if (!memberships.Any())
        {
            return Ok(new
            {
                total = 0,
                comunidades = new List<object>()
            });
        }

        var communitiesResponse = await _supabase
            .From<Community>()
            .Get();

        var allCommunities = communitiesResponse.Models;

        var myCommunityIds = memberships
            .Select(m => m.CommunityId)
            .ToList();

        var myCommunities = allCommunities
            .Where(c => myCommunityIds.Contains(c.CommunityId))
            .Select(c => new
            {
                communityId = c.CommunityId,
                name = c.Name,
                description = c.Description,
                ownerId = c.OwnerId,
                createdAt = c.CreatedAt
            })
            .ToList();

        return Ok(new
        {
            total = myCommunities.Count,
            comunidades = myCommunities
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

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCommunityForm req)
    {
        try
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (userIdStr == null)
                return Unauthorized();

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

            if (userIdStr == null)
                return Unauthorized();

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