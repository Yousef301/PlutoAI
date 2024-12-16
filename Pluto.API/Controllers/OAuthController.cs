using Microsoft.AspNetCore.Mvc;
using Pluto.Application.DTOs.Auth;
using Pluto.Application.Services;

namespace Pluto.API.Controllers;

[ApiController]
[Route("api/oauth")]
public class OAuthController : ControllerBase
{
    private readonly IServiceManager _serviceManager;

    public OAuthController(IServiceManager serviceManager)
    {
        _serviceManager = serviceManager;
    }

    [HttpGet("google-login")]
    public IActionResult RedirectToGoogle()
    {
        var redirectUrl = _serviceManager.GoogleOAuthService.GetOAuthUrl();
        return Redirect(redirectUrl);
    }

    [HttpGet("google-callback")]
    public async Task<IActionResult> GoogleCallback([FromQuery] string code)
    {
        if (string.IsNullOrEmpty(code))
        {
            return Content(@"
            <script>
                window.opener.postMessage({ type: 'google-auth-error' }, '*');
                window.close();
            </script>
        ", "text/html");
        }

        var tokens = await _serviceManager.GoogleOAuthService
            .HandleCallbackAsync(code);
        if (tokens.AccessToken == null)
        {
            return Content(@"
            <script>
                window.opener.postMessage({ type: 'google-auth-error' }, '*');
                window.close();
            </script>
        ", "text/html");
        }

        _serviceManager.AuthenticationService
            .SetTokenInsideCookie(new TokenDto(tokens.AccessToken, tokens.RefreshToken), HttpContext);

        return Content(@"
        <script>
            window.opener.postMessage({ type: 'google-auth-success' }, '*');
            window.close();
        </script>
    ", "text/html");
    }
}