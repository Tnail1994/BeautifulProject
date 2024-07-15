using CoreImplementations;

namespace Remote.Core.Transformation
{
	public class TransformException : BaseException
	{
		public TransformException(string message, int errorCode) : base(message, errorCode)
		{
		}
	}
}