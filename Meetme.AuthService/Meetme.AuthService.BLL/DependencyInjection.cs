using Meetme.AuthService.BLL.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Meetme.AuthService.BLL;

public static class DependencyInjection
{
	public static IServiceCollection AddBusinessLogicLayer(this IServiceCollection services)
	{
		services.AddHttpClient();

		services.AddScoped<IAuthService, Services.AuthService>();

		return services;
	}
}
