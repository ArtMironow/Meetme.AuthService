using Meetme.AuthService.BLL.Common;
using Meetme.AuthService.BLL.Exceptions;
using Meetme.AuthService.BLL.Interfaces;
using Meetme.AuthService.BLL.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;

namespace Meetme.AuthService.BLL.Services;


public class AuthService : IAuthService
{
	private readonly IHttpClientFactory _httpClientFactory;
	private readonly IHttpContextAccessor _httpContextAccessor;

	public AuthService(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
	{
		_httpClientFactory = httpClientFactory;
		_httpContextAccessor = httpContextAccessor;
	}

	public string GetAuthUrl(string? audience, string? clientId, string? redirectUri)
	{
		var authUrl = QueryHelpers.AddQueryString(AuthEndpoints.Authorize, new Dictionary<string, string?>
		{
			{ AuthQueryKeys.Audience, audience },
			{ AuthQueryKeys.Scope, AuthQueryValues.Scope },
			{ AuthQueryKeys.ResponseType, AuthQueryValues.ResponseType },
			{ AuthQueryKeys.ClientId, clientId },
			{ AuthQueryKeys.RedirectUri, redirectUri },
		});

		return authUrl;
	}

	public async Task<string> GetRefreshTokenAsync(string refreshToken, string? clientId, string? clientSecret)
	{
		var payload = new Dictionary<string, string?>
		{
			{ AuthQueryKeys.GrantType, AuthQueryValues.RefreshTokenGrantType },
			{ AuthQueryKeys.ClientId, clientId },
			{ AuthQueryKeys.ClientSecret, clientSecret },
			{ AuthQueryKeys.RefreshToken, refreshToken }
		};

		var httpClient = _httpClientFactory.CreateClient();

		var response = await httpClient.PostAsync(AuthEndpoints.Token, new FormUrlEncodedContent(payload));

		if (!response.IsSuccessStatusCode)
		{
			throw new TokenRetrievalException("Failed to refresh token.");
		}

		return await response.Content.ReadAsStringAsync();
	}

	public async Task GetTokensAsync(string code, string? clientId, string? clientSecret, string? redirectUri)
	{
		if (string.IsNullOrEmpty(code))
		{
			throw new ArgumentNullException(nameof(code), "Authorization code is missing.");
		}

		var tokens = await ExchangeCodeForTokensAsync(code, clientId, clientSecret, redirectUri);

		if (tokens == null)
		{
			throw new TokenRetrievalException("Failed to exchange authorization code for tokens.");
		}

		var tokensModel = JsonConvert.DeserializeObject<TokensModel>(tokens);

		SetTokensIndideCookie(tokensModel!);
	}

	private async Task<string?> ExchangeCodeForTokensAsync(string code, string? clientId, string? clientSecret, string? redirectUri)
	{
		var payload = new Dictionary<string, string?>
		{
			{ AuthQueryKeys.GrantType, AuthQueryValues.AuthorizationCodeGrantType },
			{ AuthQueryKeys.ClientId, clientId },
			{ AuthQueryKeys.ClientSecret, clientSecret },
			{ AuthQueryKeys.Code, code },
			{ AuthQueryKeys.RedirectUri, redirectUri }
		};

		var httpClient = _httpClientFactory.CreateClient();
		var response = await httpClient.PostAsync(AuthEndpoints.Token, new FormUrlEncodedContent(payload));

		if (!response.IsSuccessStatusCode)
		{
			return null;
		}

		var responseContent = await response.Content.ReadAsStringAsync();

		return responseContent;
	}

	public string GetLogoutUrl(string? clientId)
	{
		var logoutUrl = QueryHelpers.AddQueryString(AuthEndpoints.Logout, new Dictionary<string, string?>
		{
			{ AuthQueryKeys.ClientId, clientId },
			{ AuthQueryKeys.ReturnTo, AuthQueryValues.LogoutRedirectUri },
		});

		return logoutUrl;
	}

	private void SetTokensIndideCookie(TokensModel tokens)
	{
		_httpContextAccessor.HttpContext.Response.Cookies.Append(CookieKeys.AccessTokenName, tokens.AccessToken,
			new CookieOptions
			{
				Expires = DateTimeOffset.UtcNow.AddSeconds(tokens.ExpiresIn),
				HttpOnly = true,
				IsEssential = true,
			});

		_httpContextAccessor.HttpContext.Response.Cookies.Append(CookieKeys.IdTokenName, tokens.IdToken,
			new CookieOptions
			{
				Expires = DateTimeOffset.UtcNow.AddSeconds(CookieKeys.IdTokenExpiresInSeconds),
				HttpOnly = true,
				IsEssential = true,
			});

		_httpContextAccessor.HttpContext.Response.Cookies.Append(CookieKeys.RefreshTokenName, tokens.RefreshToken,
			new CookieOptions
			{
				HttpOnly = true,
				IsEssential = true,
			});
	}
}
