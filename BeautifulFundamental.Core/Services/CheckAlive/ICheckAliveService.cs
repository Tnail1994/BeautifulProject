namespace BeautifulFundamental.Core.Services.CheckAlive
{
	public interface ICheckAliveService
	{
		event Action ConnectionLost;
		void Start();
		void Stop(bool force = false);
	}
}