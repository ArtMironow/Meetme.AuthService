﻿using Meetme.AuthService.API.Common;
using Meetme.AuthService.API.Models;
using Meetme.AuthService.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Meetme.AuthService.API.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
	private readonly AuthKeys _authKeys;
	private readonly IAuthService _authService;

	public AuthController(IOptions<AuthKeys> authKeysAccessor, IAuthService authService)
	{
		_authKeys = authKeysAccessor.Value;
		_authService = authService;
	}

	[HttpGet(EndpointRoutes.Login)]
	public AuthUrlResponse Login()
	{
		return new AuthUrlResponse
		{
			AuthUrl = _authService.GetAuthUrl(_authKeys.Audience, _authKeys.ClientId, _authKeys.RedirectUri)
		};
	}

	[HttpGet(EndpointRoutes.Callback)]
	public Task<string> Callback([FromQuery] string code)
	{
		return _authService.GetTokensAsync(code, _authKeys.ClientId, _authKeys.ClientSecret, _authKeys.RedirectUri);
	}

	[HttpPost(EndpointRoutes.RefreshToken)]
	public Task<string> RefreshToken([FromBody] string refreshToken)
	{
		return _authService.GetRefreshTokenAsync(refreshToken, _authKeys.ClientId, _authKeys.ClientSecret);
	}

	[HttpGet(EndpointRoutes.Logout)]
	[Authorize]
	public LogoutUrlResponse Logout()
	{
		return new LogoutUrlResponse
		{
			LogoutUrl = _authService.GetLogoutUrl(_authKeys.ClientId)
		};
	}
}
