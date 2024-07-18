using Core.Extensions;
using Remote.Communication.Common.Contracts;
using SharedBeautifulData;
using SharedBeautifulServices.Common;

namespace SharedBeautifulServices
{
	public class CheckAliveService : ICheckAliveService, IDisposable
	{
		private const int MinFrequencyInSeconds = 20;

		private readonly ICheckAliveSettings _settings;
		private readonly ICommunicationService _communicationService;
		private readonly CancellationTokenSource _cts = new();

		public CheckAliveService(ICheckAliveSettings settings, ICommunicationService communicationService)
		{
			_settings = settings;
			_communicationService = communicationService;
		}


		public event Action? ConnectionLost;

		public void Start()
		{
			if (!_settings.Enabled)
				return;

			if (_settings.FrequencyInSeconds < MinFrequencyInSeconds)
				throw new CheckAliveException("FrequencyInSeconds must be at least 20", 0);

			_communicationService.ConnectionLost += OnConnectionLost;
			_cts.TryReset();

			switch (_settings.Mode)
			{
				case 0:
					Task.Run(ReplyCheckAlive);
					break;
				case 1:
					Task.Run(SendCheckAlive);
					break;
				default:
					throw new CheckAliveException("Invalid mode", 1);
			}
		}


		private async void ReplyCheckAlive()
		{
			try
			{
				var checkAliveReplyMessage = new CheckAliveReplyMessage() { MessageObject = true };

				while (!_cts.IsCancellationRequested)
				{
					var checkAliveMessage = await _communicationService.ReceiveAsync<CheckAliveMessage>(_cts.Token);

					if (!checkAliveMessage.MessageObject)
						ConnectionLost?.Invoke();

					_communicationService.SendAsync(checkAliveReplyMessage);
				}
			}
			catch (OperationCanceledException oce)
			{
				this.LogDebug($"ReplyCheckAlive cancelled: {oce.Message}");
			}
			catch (Exception ex) when (!_cts.Token.IsCancellationRequested)
			{
				this.LogFatal($"!!! Unexpected error in SendCheckAlive loop: {ex.Message}+" +
				              $"Stacktrace: {ex.StackTrace}");
			}
		}

		private async void SendCheckAlive()
		{
			try
			{
				var checkAliveMessage = new CheckAliveMessage() { MessageObject = true };

				while (!_cts.IsCancellationRequested)
				{
					_communicationService.SendAsync(checkAliveMessage);
					var checkAliveReplyMessage =
						await _communicationService.ReceiveAsync<CheckAliveReplyMessage>(_cts.Token);

					if (!checkAliveReplyMessage.MessageObject)
						ConnectionLost?.Invoke();

					await Task.Delay(_settings.FrequencyInSeconds * 1000, _cts.Token);
				}
			}
			catch (OperationCanceledException oce)
			{
				this.LogDebug($"SendCheckAlive cancelled: {oce.Message}");
			}
			catch (Exception ex) when (!_cts.Token.IsCancellationRequested)
			{
				this.LogFatal($"!!! Unexpected error in SendCheckAlive loop: {ex.Message}+" +
				              $"Stacktrace: {ex.StackTrace}");
			}
		}

		private void OnConnectionLost(object? sender, string e)
		{
			Stop();
		}

		public void Stop()
		{
			_communicationService.ConnectionLost -= OnConnectionLost;
			_cts.Cancel();
		}

		public void Dispose()
		{
			Stop();
			_cts.Dispose();
		}
	}
}