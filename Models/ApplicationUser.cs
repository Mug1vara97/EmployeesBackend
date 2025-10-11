using Microsoft.AspNetCore.Identity;

namespace EmployerApp.Api.Models;

public class ApplicationUser : IdentityUser<Guid>
{
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}


