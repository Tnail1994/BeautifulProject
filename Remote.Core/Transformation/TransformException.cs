namespace Remote.Core.Transformation
{
	public class TransformException : Exception
	{
		public TransformException(string message, int errorCode) : base(message)
		{
			ErrorCode = errorCode;
		}

		public int ErrorCode { get; }
	}
}