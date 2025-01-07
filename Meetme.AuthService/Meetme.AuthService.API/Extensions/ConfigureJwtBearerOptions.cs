using Meetme.AuthService.API.Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace Meetme.AuthService.API.Extensions;

public class ConfigureJwtBearerOptions : IConfigureNamedOptions<JwtBearerOptions>
{
	private readonly AuthKeys _authKeys;

	public ConfigureJwtBearerOptions(IOptions<AuthKeys> authKeysAccessor)
	{
		_authKeys = authKeysAccessor.Value;
	}

	public void Configure(string? name, JwtBearerOptions options)
	{
		options.Authority = ConfigurationKeys.Authority;
		options.Audience = _authKeys.Audience;
		options.TokenValidationParameters = new TokenValidationParameters
		{
			NameClaimType = ClaimTypes.NameIdentifier
		};
	}

	public void Configure(JwtBearerOptions options) => Configure(Options.DefaultName, options);
}
