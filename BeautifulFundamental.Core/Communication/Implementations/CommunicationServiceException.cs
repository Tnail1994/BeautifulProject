using BeautifulFundamental.Core.Exceptions;

namespace BeautifulFundamental.Core.Communication.Implementations
{
	public class CommunicationServiceException : BaseException
	{
		public CommunicationServiceException(string message, int errorCode = -1) : base(message, errorCode)
		{
		}
	}
}