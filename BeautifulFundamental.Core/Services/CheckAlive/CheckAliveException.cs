using BeautifulFundamental.Core.Exceptions;

namespace BeautifulFundamental.Core.Services.CheckAlive
{
	public class CheckAliveException : BaseException
	{
		public CheckAliveException(string message, int errorCode = -1) : base(message, errorCode)
		{
		}
	}
}