namespace CoreImplementations
{
	public class BaseException : Exception
	{
		public BaseException(string message, int errorCode = -1) : base(message)
		{
			ErrorCode = errorCode;
		}

		public int ErrorCode { get; }
	}
}