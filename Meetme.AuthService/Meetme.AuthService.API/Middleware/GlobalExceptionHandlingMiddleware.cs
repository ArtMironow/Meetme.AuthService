using Meetme.AuthService.API.Errors;
using System.Net;

namespace Meetme.AuthService.API.Middleware;

public class GlobalExceptionHandlingMiddleware : IMiddleware
{
	public async Task InvokeAsync(HttpContext context, RequestDelegate next)
	{
		try
		{
			await next(context);
		}
		catch (Exception ex)
		{
			var statusCode = ex switch
			{
				ArgumentNullException => HttpStatusCode.BadRequest,
				_ => HttpStatusCode.InternalServerError
			};

			context.Response.StatusCode = (int)statusCode;

			var errorDetails = new ErrorDetails
			{
				ErrorTitle = "Server error",
				ErrorMessage = ex.Message
			};

			await context.Response.WriteAsJsonAsync(errorDetails);
		}
	}
}
