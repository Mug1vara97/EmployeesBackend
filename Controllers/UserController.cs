using System.IdentityModel.Tokens.Jwt;
using EmployerApp.Api.Contracts;
using EmployerApp.Api.Data;
using EmployerApp.Api.Models;
using EmployerApp.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployerApp.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UserController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _dbContext;
    private readonly ITokenGenerator _tokenGenerator;
    private readonly ILoginHashService _loginHashService;

    public UserController(UserManager<ApplicationUser> userManager, ApplicationDbContext dbContext, ITokenGenerator tokenGenerator, ILoginHashService loginHashService)
    {
        _userManager = userManager;
        _dbContext = dbContext;
        _tokenGenerator = tokenGenerator;
        _loginHashService = loginHashService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserRequest request)
    {
        var hashedEmail = _loginHashService.HashLogin(request.Email);
        
        var user = new ApplicationUser
        {
            Email = request.Email,
            UserName = hashedEmail
        };
        
        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        return Created();
    }
    
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] RegisterUserRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        
        if (user is null)
            return Unauthorized();

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!isPasswordValid)
            return Unauthorized();

        var accessToken = _tokenGenerator.GenerateAccessToken(user);
        var refreshToken = _tokenGenerator.GenerateRefreshToken();
        
        user.RefreshTokens.Add(new RefreshToken
        {
            Token = refreshToken,
            User = user
        });
        
        _dbContext.Users.Update(user);
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return Ok(new TokensDto(accessToken, refreshToken));
    }
    
    [Authorize]
    [HttpGet("logout")]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken = default)
    {
        var userIdClaims = User.Claims.First(c => c.Type == "userid");
        
        var userId = Guid.Parse(userIdClaims.Value);
        
        var user = await _userManager.FindByIdAsync(userId.ToString());
        
        if (user is null)
            return Unauthorized();

        await _dbContext.RefreshTokens.Where(t => t.UserId == userId).ExecuteDeleteAsync(cancellationToken);
        return NoContent();
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] TokensDto tokens, CancellationToken cancellationToken = default)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwtSecurityToken = handler.ReadJwtToken(tokens.AccessToken);

        var userIdClaim = jwtSecurityToken.Claims.First(c => c.Type == "userid");
        var userId = Guid.Parse(userIdClaim.Value);
        
        var user = await _userManager.FindByIdAsync(userId.ToString());
        
        if (user is null)
            return Unauthorized();

        var refreshToken = await _dbContext.RefreshTokens.FirstOrDefaultAsync(t => t.UserId == userId && t.Token == tokens.RefreshToken, cancellationToken: cancellationToken);
        
        if (refreshToken is null)
            return Forbid();
        
        _dbContext.RefreshTokens.Remove(refreshToken);

        var newRefreshToken = _tokenGenerator.GenerateRefreshToken();
        var newAccessToken = _tokenGenerator.GenerateAccessToken(user);

        _dbContext.RefreshTokens.Add(new RefreshToken
        {
            Token = newRefreshToken,
            User = user
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Ok(new TokensDto(newAccessToken, newRefreshToken));
    }
}
