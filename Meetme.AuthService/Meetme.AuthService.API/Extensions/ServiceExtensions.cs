using Meetme.AuthService.API.Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;

namespace Meetme.AuthService.API.Extensions;

public static class ServiceExtensions
{
	public static void ConfigureCors(this IServiceCollection services) =>
		services.AddCors(options =>
		{
			options.AddDefaultPolicy(builder =>
				builder.AllowAnyOrigin()
					.AllowAnyMethod()
					.AllowAnyHeader());
		});

	public static void ConfigureAuth(this IServiceCollection services, IConfiguration configuration)
	{
		services.Configure<AuthKeys>(configuration.GetSection(ConfigurationKeys.AuthKeysSection));

		services.AddSingleton<IConfigureOptions<JwtBearerOptions>, ConfigureJwtBearerOptions>();

		services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
	}
}
