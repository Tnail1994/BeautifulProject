using Core.Extensions;

namespace BeautifulMauiClientApplication.Startup
{
	public interface IStartupService
	{
		Task Start();
	}

	public class StartupService(IServiceProvider serviceProvider) : IStartupService
	{
		private readonly IEnumerable<IAutoStartService> _autoStartServices =
			serviceProvider.GetServices<IAutoStartService>();

		public async Task Start()
		{
			var allTasks = new List<Task<StartingResult>>();

			foreach (var service in _autoStartServices)
			{
				allTasks.Add(service.Start());
			}

			var allStartingResults = await Task.WhenAll(allTasks);

			foreach (var startingResult in allStartingResults)
			{
				if (startingResult.Success)
				{
					this.LogInfo($"Started {startingResult.ServiceName} successful");
				}
				else
				{
					this.LogError($"Started {startingResult.ServiceName} failed");
				}
			}
		}
	}
}