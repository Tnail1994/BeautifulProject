using CoreImplementations;

namespace Remote.Communication.Common.Transformation.Implementations
{
	public class TransformException : BaseException
	{
		public TransformException(string message, int errorCode) : base(message, errorCode)
		{
		}
	}
}