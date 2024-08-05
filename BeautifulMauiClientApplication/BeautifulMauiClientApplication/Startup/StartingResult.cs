namespace BeautifulMauiClientApplication.Startup
{
	public class StartingResult(bool success, string serviceName)
	{
		public bool Success { get; set; } = success;
		public string ServiceName { get; set; } = serviceName;

		public static StartingResult Create(bool success, string serviceName)
		{
			return new StartingResult(success, serviceName);
		}
	}
}