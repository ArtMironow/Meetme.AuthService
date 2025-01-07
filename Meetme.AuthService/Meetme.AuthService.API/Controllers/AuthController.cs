using Meetme.AuthService.API.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Meetme.AuthService.API.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
	private readonly IHttpClientFactory _httpClientFactory;
	private readonly AuthKeys _authKeys;

	public AuthController(IHttpClientFactory httpClientFactory, IOptions<AuthKeys> authKeysAccessor)
	{
		_httpClientFactory = httpClientFactory;
		_authKeys = authKeysAccessor.Value;
	}

	[HttpGet("login")]
	public string? Login()
	{
		var authUrl = QueryHelpers.AddQueryString(AuthEndpoints.Authorize, new Dictionary<string, string>
		{
			{ AuthQueryKeys.Audience, _authKeys.Audience },
			{ AuthQueryKeys.Scope, AuthQueryValues.Scope },
			{ AuthQueryKeys.ResponseType, AuthQueryValues.ResponseType },
			{ AuthQueryKeys.ClientId, _authKeys.ClientId },
			{ AuthQueryKeys.RedirectUri, _authKeys.RedirectUri },
		});

		return authUrl;
	}

	[HttpGet("callback")]
	public async Task<IActionResult> Callback([FromQuery] string code)
	{
		if (string.IsNullOrEmpty(code))
		{
			return BadRequest("Authorization code is missing.");
		}

		var tokens = await ExchangeCodeForTokens(code);
		if (tokens == null)
		{
			return StatusCode(500, "Failed to exchange authorization code for tokens.");
		}

		var tokensResponse = JsonConvert.SerializeObject(tokens);

		return Ok(tokensResponse);
	}

	private async Task<object?> ExchangeCodeForTokens(string code)
	{
		var payload = new Dictionary<string, string>
		{
			{ AuthQueryKeys.GrantType, AuthQueryValues.AuthorizationCodeGrantType },
			{ AuthQueryKeys.ClientId, _authKeys.ClientId },
			{ AuthQueryKeys.ClientSecret, _authKeys.ClientSecret },
			{ AuthQueryKeys.Code, code },
			{ AuthQueryKeys.RedirectUri, _authKeys.RedirectUri }
		};

		var httpClient = _httpClientFactory.CreateClient();
		var response = await httpClient.PostAsync(AuthEndpoints.Token, new FormUrlEncodedContent(payload));

		if (!response.IsSuccessStatusCode)
		{
			return null;
		}

		var responseContent = await response.Content.ReadAsStringAsync();
		return JsonConvert.DeserializeObject<object>(responseContent);
	}

	[HttpPost("refresh-token")]
	public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
	{
		var payload = new Dictionary<string, string>
		{
			{ AuthQueryKeys.GrantType, AuthQueryValues.RefreshTokenGrantType },
			{ AuthQueryKeys.ClientId, _authKeys.ClientId },
			{ AuthQueryKeys.ClientSecret, _authKeys.ClientSecret },
			{ AuthQueryKeys.RefreshToken, refreshToken }
		};

		var httpClient = _httpClientFactory.CreateClient();
		var response = await httpClient.PostAsync(AuthEndpoints.Token, new FormUrlEncodedContent(payload));

		if (!response.IsSuccessStatusCode)
		{
			return StatusCode(500, "Failed to refresh token.");
		}

		var responseContent = await response.Content.ReadAsStringAsync();

		return Ok(responseContent);
	}


	[Authorize]
	[HttpGet("logout")]
	public string Logout()
	{
		var logoutUrl = QueryHelpers.AddQueryString(AuthEndpoints.Logout, new Dictionary<string, string>
		{
			{ AuthQueryKeys.ClientId, _authKeys.ClientId },
			{ AuthQueryKeys.ReturnTo, AuthQueryValues.LogoutRedirectUri },
		});

		return logoutUrl;
	}
}