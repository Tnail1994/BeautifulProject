using BeautifulFundamental.Core.Exceptions;

namespace BeautifulFundamental.Server.Session.Implementations
{
	public class SessionManagerException : BaseException
	{
		public SessionManagerException(string message, int errorCode) : base(message, errorCode)
		{
		}
	}
}