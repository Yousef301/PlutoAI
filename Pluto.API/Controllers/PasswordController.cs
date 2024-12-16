﻿using Microsoft.AspNetCore.Mvc;
using Pluto.Application.DTOs.Auth;
using Pluto.Application.Services;

namespace Pluto.API.Controllers;

[ApiController]
[Route("api/password")]
public class PasswordController : ControllerBase
{
    private readonly IServiceManager _serviceManager;

    public PasswordController(IServiceManager serviceManager)
    {
        _serviceManager = serviceManager;
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] SendPasswordResetRequest request)
    {
        await _serviceManager.PasswordService
            .SendPasswordResetEmail(request);

        return Ok();
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        await _serviceManager.PasswordService
            .ResetPassword(request);

        return Ok();
    }
}