namespace Meetme.AuthService.BLL.Common;

public class AuthQueryValues
{
	public const string ResponseType = "code";
	public const string Scope = "openid profile email offline_access";
	public const string AuthorizationCodeGrantType = "authorization_code";
	public const string RefreshTokenGrantType = "refresh_token";
	public const string LogoutRedirectUri = "https://localhost:7126";
}
