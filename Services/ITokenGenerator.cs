using EmployerApp.Api.Models;

namespace EmployerApp.Api.Services;

public interface ITokenGenerator
{
    public string GenerateAccessToken(ApplicationUser user);
    public string GenerateRefreshToken();
}
