﻿using System.Security.Claims;
using Pluto.Application.DTOs.Auth;

namespace Pluto.Application.Services.EntityServices.Interfaces.Auth;

public interface IGoogleAuthService
{
    string GetGoogleOAuthUrl();
    Task<TokenDto> HandleGoogleCallbackAsync(string code);
}