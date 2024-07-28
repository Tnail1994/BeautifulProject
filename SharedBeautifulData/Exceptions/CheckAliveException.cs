using Core.Exceptions;

namespace SharedBeautifulData.Exceptions
{
    public class CheckAliveException : BaseException
    {
        public CheckAliveException(string message, int errorCode = -1) : base(message, errorCode)
        {
        }
    }
}