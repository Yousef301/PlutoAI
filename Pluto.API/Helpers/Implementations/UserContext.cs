using System.Security.Claims;
using Pluto.API.Helpers.Interfaces;

namespace Pluto.API.Helpers.Implementations;

public class UserContext : IUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private string GetClaimValue(string claimType)
    {
        var claimValue = _httpContextAccessor.HttpContext?.User.FindFirstValue(claimType);
        if (claimValue == null)
        {
            throw new UnauthorizedAccessException();
        }

        return claimValue;
    }

    public int Id => Int32.Parse(GetClaimValue("id"));

    public string FullName => GetClaimValue("fullname");

    public string Email => GetClaimValue(ClaimTypes.Email);
}