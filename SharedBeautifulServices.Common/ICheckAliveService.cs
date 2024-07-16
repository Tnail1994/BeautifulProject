namespace SharedBeautifulServices.Common
{
	public interface ICheckAliveService
	{
		event Action ConnectionLost;
		void Start();
		void Stop();
	}
}