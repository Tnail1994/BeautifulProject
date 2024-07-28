using Core.Exceptions;

namespace Remote.Communication.Common.Implementations
{
	public class CommunicationServiceException : BaseException
	{
		public CommunicationServiceException(string message, int errorCode = -1) : base(message, errorCode)
		{
		}
	}
}