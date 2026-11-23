using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace NexusFlow.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public AuthController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpPost("token")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    public IActionResult GenerateToken([FromBody] TokenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
            return BadRequest(new ProblemDetails { Title = "Bad Request", Detail = "Username is required." });

        var secret = _configuration["JwtSettings:Secret"] ?? "NexusFlow_Super_Secure_Secret_Key_For_Jwt_Generation_2026!";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, request.Username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, request.Role ?? "WorkflowEngineer")
        };

        var expires = DateTime.UtcNow.AddHours(4);
        var token = new JwtSecurityToken(
            issuer: _configuration["JwtSettings:Issuer"] ?? "NexusFlow",
            audience: _configuration["JwtSettings:Audience"] ?? "NexusFlowClients",
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        return Ok(new TokenResponse(new JwtSecurityTokenHandler().WriteToken(token), expires));
    }
}

public record TokenRequest(string Username, string? Role);
public record TokenResponse(string Token, DateTime ExpiresAtUtc);
