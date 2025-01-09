namespace Meetme.AuthService.BLL.Exceptions;

public class TokenRetrievalException : Exception
{
	public TokenRetrievalException()
	{ }

	public TokenRetrievalException(string message) : base(message)
	{ }

	public TokenRetrievalException(string message, Exception innerException) : base(message, innerException)
	{ }
}
