using Core.Extensions;
using Remote.Communication.Common.Contracts;
using SharedBeautifulServices.Common;

namespace Remote.Communication
{
	public class ConnectionService : IConnectionService
	{
		private readonly ICommunicationService _communicationService;
		private readonly ICheckAliveService _checkAliveService;
		public event Action<string>? ConnectionLost;
		public event Action? Reconnected;

		public ConnectionService(ICommunicationService communicationService, ICheckAliveService checkAliveService)
		{
			_communicationService = communicationService;
			_checkAliveService = checkAliveService;

			_communicationService.ConnectionLost += OnConnectionLost;
			_checkAliveService.ConnectionLost += OnConnectionLost;
		}

		public void Start()
		{
			_communicationService.Start();
			_checkAliveService.Start();
		}

		public void Stop()
		{
			_communicationService.Stop();
			_checkAliveService.Stop();
		}

		private void OnConnectionLost(object? sender, string reason)
		{
			InvokeOnConnectionLost(reason);
		}

		private void InvokeOnConnectionLost(string reason)
		{
			ConnectionLost?.Invoke(reason);
			this.LogDebug("Try to reconnect");
			// Todo
		}

		private void OnConnectionLost()
		{
			InvokeOnConnectionLost("Check alive did not receive answer.");
		}

		public void Dispose()
		{
			_communicationService.Dispose();
			_checkAliveService.Dispose();
		}
	}
}