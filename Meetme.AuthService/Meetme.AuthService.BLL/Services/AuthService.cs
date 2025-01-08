using Meetme.AuthService.BLL.Common;
using Meetme.AuthService.BLL.Exceptions;
using Meetme.AuthService.BLL.Interfaces;
using Microsoft.AspNetCore.WebUtilities;

namespace Meetme.AuthService.BLL.Services;


public class AuthService : IAuthService
{
	private readonly IHttpClientFactory _httpClientFactory;

	public AuthService(IHttpClientFactory httpClientFactory)
	{
		_httpClientFactory = httpClientFactory;
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

	public async Task<string> GetTokensAsync(string code, string? clientId, string? clientSecret, string? redirectUri)
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

		return tokens;
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
}
