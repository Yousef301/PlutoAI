using System.Net.Http.Json;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using Pluto.Application.Services.EntityServices.Interfaces.Auth;
using Pluto.Application.Services.SharedServices.Interfaces;
using Pluto.DAL.Interfaces.Repositories;

namespace Pluto.Application.Services.EntityServices.Implementations.Auth;

public class GoogleAuthService : IGoogleAuthService
{
    private readonly IConfiguration _configuration;
    private readonly IUserRepository _userRepository;
    private readonly ITokenGeneratorService _tokenGeneratorService;

    public GoogleAuthService(
        IUserRepository userRepository,
        ITokenGeneratorService tokenGeneratorService,
        IConfiguration configuration
    )
    {
        _userRepository = userRepository;
        _tokenGeneratorService = tokenGeneratorService;
        _configuration = configuration;
    }


    public string GetGoogleOAuthUrl()
    {
        var clientId = _configuration["ClientId"];
        var redirectUri = _configuration["RedirectUri"];
        return
            $"https://accounts.google.com/o/oauth2/auth?client_id={clientId}&redirect_uri={redirectUri}&response_type=code&scope=openid%20email%20profile";
    }

    public async Task<string?> HandleGoogleCallbackAsync(string code)
    {
        var tokenResponse = await ExchangeCodeForTokensAsync(code);
        if (tokenResponse == null) return null;

        var payload = await ValidateGoogleTokenAsync(tokenResponse);
        if (payload == null) return null;

        var user = await _userRepository.FindOrCreateUserAsync(payload);

        return _tokenGeneratorService.GenerateToken(user);
    }

    private async Task<string?> ExchangeCodeForTokensAsync(string code)
    {
        var clientId = _configuration["ClientId"];
        var clientSecret = _configuration["ClientSecret"];
        var redirectUri = _configuration["RedirectUri"];

        using var client = new HttpClient();
        var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "https://oauth2.googleapis.com/token")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "code", code },
                { "client_id", clientId },
                { "client_secret", clientSecret },
                { "redirect_uri", redirectUri },
                { "grant_type", "authorization_code" }
            })
        };

        var response = await client.SendAsync(tokenRequest);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var tokenData = await response.Content.ReadFromJsonAsync<GoogleTokenResponse>();
        if (tokenData == null)
        {
            return null;
        }

        return tokenData.IdToken;
    }

    private async Task<GoogleJsonWebSignature.Payload?> ValidateGoogleTokenAsync(string idToken)
    {
        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { _configuration["ClientId"] }
            };

            return await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
        }
        catch (Exception ex)
        {
            return null;
        }
    }
}