using CoreImplementations;

namespace BeautifulServerApplication.Session
{
	public class SessionManagerException(string message, int errorCode) : BaseException(message, errorCode);
}