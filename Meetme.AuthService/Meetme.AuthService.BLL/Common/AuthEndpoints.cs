namespace Meetme.AuthService.BLL.Common;

public static class AuthEndpoints
{
	public const string BaseUrl = "https://dev-m7hh71yj06j0cmir.us.auth0.com";
	public const string Authorize = $"{BaseUrl}/authorize";
	public const string Token = $"{BaseUrl}/oauth/token";
	public const string Logout = $"{BaseUrl}/v2/logout";
}
