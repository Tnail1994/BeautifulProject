namespace BeautifulMauiClientApplication.Startup
{
	public interface IAutoStartService
	{
		Task<StartingResult> Start();
	}
}