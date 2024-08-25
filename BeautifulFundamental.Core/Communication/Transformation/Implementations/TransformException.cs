using BeautifulFundamental.Core.Exceptions;

namespace BeautifulFundamental.Core.Communication.Transformation.Implementations
{
	public class TransformException : BaseException
	{
		public TransformException(string message, int errorCode) : base(message, errorCode)
		{
		}
	}
}