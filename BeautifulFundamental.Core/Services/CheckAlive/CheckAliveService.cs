using BeautifulFundamental.Core.Communication;
using BeautifulFundamental.Core.Extensions;
using BeautifulFundamental.Core.Identification;
using BeautifulFundamental.Core.Messages.CheckAlive;

namespace BeautifulFundamental.Core.Services.CheckAlive
{
	public interface ICheckAliveService
	{
		event Action ConnectionLost;
		void Start();
		void Stop(bool force = false);
	}

	public class CheckAliveService : ICheckAliveService, IDisposable
	{
		private const int MinFrequencyInSeconds = 20;

		private readonly ICheckAliveSettings _settings;
		private readonly ICommunicationService _communicationService;
		private readonly IIdentificationKey _identificationKey;
		private CancellationTokenSource _cts = new();

		private bool _running;
		private Task? _replyTask;
		private Task? _sendTask;

		public CheckAliveService(ICheckAliveSettings settings, ICommunicationService communicationService,
			IIdentificationKey identificationKey)
		{
			_settings = settings;
			_communicationService = communicationService;
			_identificationKey = identificationKey;
		}


		public event Action? ConnectionLost;

		public void Start()
		{
			if (_running)
			{
				this.LogVerbose("CheckAliveService allready running", _identificationKey.SessionId);
				return;
			}

			if (!_settings.Enabled)
				return;

			if (_settings.FrequencyInSeconds < MinFrequencyInSeconds)
				throw new CheckAliveException("FrequencyInSeconds must be at least 20", 0);

			_communicationService.ConnectionLost += OnConnectionLost;

			switch (_settings.Mode)
			{
				case 0:
					_replyTask = Task.Run(ReplyCheckAlive, _cts.Token);
					break;
				case 1:
					_sendTask = Task.Run(SendCheckAlive, _cts.Token);
					break;
				default:
					throw new CheckAliveException("Invalid mode", 1);
			}

			_running = true;
		}


		private async void ReplyCheckAlive()
		{
			try
			{
				var checkAliveReplyMessage = new CheckAliveReply() { Success = true };

				while (!_cts.IsCancellationRequested)
				{
					var checkAliveMessage = await ReceiveAndSendAsync(checkAliveReplyMessage);

					if (!checkAliveMessage.Success)
						ConnectionLost?.Invoke();
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

		private Task<CheckAliveRequest> ReceiveAndSendAsync(CheckAliveReply checkAliveReplyMessage)
		{
			return _communicationService.ReceiveAndSendAsync<CheckAliveRequest>(checkAliveReplyMessage);
		}

		private async void SendCheckAlive()
		{
			try
			{
				var checkAliveMessage = new CheckAliveRequest() { Success = true };

				while (!_cts.IsCancellationRequested)
				{
					var checkAliveReplyMessage = await SendAndReceiveAsync(checkAliveMessage);

					if (!checkAliveReplyMessage.Success)
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

		private Task<CheckAliveReply> SendAndReceiveAsync(CheckAliveRequest checkAliveMessage)
		{
			return _communicationService.SendAndReceiveAsync<CheckAliveReply>(checkAliveMessage);
		}

		private void OnConnectionLost(object? sender, string e)
		{
			Stop();
		}

		public void Stop(bool force = false)
		{
			if (!_running)
			{
				this.LogVerbose("CheckAliveService not running", _identificationKey.SessionId);
				return;
			}

			_communicationService.ConnectionLost -= OnConnectionLost;
			_cts.Cancel();
			_cts = new CancellationTokenSource();

			_running = false;
		}

		public void Dispose()
		{
			Stop();
			_cts.Dispose();

			if (_replyTask != null && (_replyTask.IsCompleted || _replyTask.IsCanceled || _replyTask.IsFaulted))
			{
				_replyTask.Dispose();
				_replyTask = null;
			}

			if (_sendTask != null && (_sendTask.IsCompleted || _sendTask.IsCanceled || _sendTask.IsFaulted))
			{
				_sendTask.Dispose();
				_sendTask = null;
			}
		}
	}
}