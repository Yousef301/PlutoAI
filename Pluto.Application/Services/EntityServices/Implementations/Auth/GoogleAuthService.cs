using System.Net.Http.Json;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using Pluto.Application.Services.EntityServices.Interfaces.Auth;
using Pluto.Application.Services.SharedServices.Interfaces;
using Pluto.DAL.Exceptions;
using Pluto.DAL.Exceptions.Base;
using Pluto.DAL.Interfaces.Repositories;
using InvalidConfigurationException = Microsoft.IdentityModel.Protocols.Configuration.InvalidConfigurationException;

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

        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(redirectUri))
            throw new InvalidConfigurationException("Google OAuth client ID or redirect URI is missing.");

        return
            $"https://accounts.google.com/o/oauth2/auth?client_id={clientId}&redirect_uri={redirectUri}&response_type=code&scope=openid%20email%20profile";
    }

    public async Task<string?> HandleGoogleCallbackAsync(string code)
    {
        if (string.IsNullOrEmpty(code))
            throw new InvalidRequestException("Authorization code is missing or invalid.");

        var tokenResponse = await ExchangeCodeForTokensAsync(code);
        if (tokenResponse == null)
            throw new ServiceException("Failed to exchange code for tokens.");

        var payload = await ValidateGoogleTokenAsync(tokenResponse);
        if (payload == null)
            throw new InvalidTokenException("The Google ID token is invalid or expired.");

        var user = await _userRepository.FindOrCreateUserAsync(payload);

        return _tokenGeneratorService.GenerateToken(user);
    }

    private async Task<string?> ExchangeCodeForTokensAsync(string code)
    {
        var clientId = _configuration["ClientId"];
        var clientSecret = _configuration["ClientSecret"];
        var redirectUri = _configuration["RedirectUri"];

        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret) || string.IsNullOrEmpty(redirectUri))
            throw new InvalidConfigurationException(
                "Google OAuth client ID, client secret, or redirect URI is missing.");

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

        try
        {
            var response = await client.SendAsync(tokenRequest);
            if (!response.IsSuccessStatusCode)
            {
                throw new ServiceException("Failed to retrieve tokens from Google.");
            }

            var tokenData = await response.Content.ReadFromJsonAsync<GoogleTokenResponse>();
            if (tokenData == null)
            {
                throw new ServiceException("Invalid response from Google token endpoint.");
            }

            return tokenData.IdToken;
        }
        catch (HttpRequestException ex)
        {
            throw new ServiceException("Error communicating with Google token endpoint.", ex);
        }
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
        catch (InvalidJwtException ex)
        {
            throw new InvalidTokenException("Google ID token is invalid.", ex);
        }
        catch (Exception ex)
        {
            throw new ServiceException("Error validating Google ID token.", ex);
        }
    }
}