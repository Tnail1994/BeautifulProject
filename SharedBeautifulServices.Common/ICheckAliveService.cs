namespace SharedBeautifulServices.Common
{
	public interface ICheckAliveService : IDisposable
	{
		event Action ConnectionLost;
		void Start();
		void Stop();
	}
}