using Meetme.AuthService.BLL.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Meetme.AuthService.BLL;

public static class DependencyInjection
{
	public static IServiceCollection AddBusinessLogicLayer(this IServiceCollection services,
		IConfiguration configuration)
	{
		services.AddHttpClient();

		services.AddScoped<IAuthService, Services.AuthService>();

		return services;
	}
}
