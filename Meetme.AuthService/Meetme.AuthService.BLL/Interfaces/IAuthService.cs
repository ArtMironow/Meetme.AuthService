namespace Meetme.AuthService.BLL.Interfaces;

public interface IAuthService
{
	string GetAuthUrl(string? audience, string? clientId, string? redirectUri);
	string GetLogoutUrl(string? clientId);
	Task<string> GetRefreshTokenAsync(string refreshToken, string? clientId, string? clientSecret);
	Task<string> GetTokensAsync(string code, string? clientId, string? clientSecret, string? redirectUri);
}
