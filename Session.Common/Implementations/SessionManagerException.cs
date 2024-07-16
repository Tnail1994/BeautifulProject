using Core.Exceptions;

namespace Session.Common.Implementations
{
	public class SessionManagerException : BaseException
	{
		public SessionManagerException(string message, int errorCode) : base(message, errorCode)
		{
		}
	}
}