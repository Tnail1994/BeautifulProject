using CoreHelpers;
using CoreImplementations;
using Remote.Core.Communication;

namespace BeautifulServerApplication.Session
{
	public interface ISession
	{
		event EventHandler<string>? SessionOnHold;
		string Id { get; }

		void Start();
		void Stop();

#if DEBUG
		void SendMessageToClient(object message);
#endif
	}

	internal class Session : ISession, IDisposable
	{
		private readonly ICommunicationService _communicationService;

		private Session(ICommunicationService communicationService)
		{
			Id = GuidIdCreator.CreateString();

			_communicationService = communicationService;
			_communicationService.ConnectionLost += OnConnectionLost;
		}

		public string Id { get; }
		public event EventHandler<string>? SessionOnHold;

		public void Start()
		{
			this.LogDebug($"Starting session {Id}");

			// From here the session can be used to communicate with the client.
			// All what happens here, should happen parallel to the main thread.
			// So beware of writing to the console or doing other blocking operations.
			// Need to define an own logging system for this session overall.

			StartCommunicationService();
		}

		public void Stop()
		{
			this.LogDebug($"Stopping session {Id}");

			Dispose();
		}


		private void StartCommunicationService()
		{
			try
			{
				_communicationService.Start();
			}
			catch (NullReferenceException nullReferenceException)
			{
				this.LogError($"Cannot start communication for this session: {Id}" +
				              $"Possible no client is set to the communication service. Check <cs_setClient>. Result = {_communicationService.IsClientSet}" +
				              $"{nullReferenceException.Message}");

				if (!_communicationService.IsClientSet)
				{
					// Todo retry to set a client
				}
			}
			catch (Exception ex)
			{
				this.LogFatal($"!!! Unexpected {Id}" +
				              $"{ex.Message}");
			}
		}

		#region Factory

		public static ISession Create(ICommunicationService communicationService)
		{
			return new Session(communicationService);
		}

		#endregion

		private void OnConnectionLost(object? sender, string reason)
		{
			SessionOnHold?.Invoke(this, $"Connection lost: {reason}");
		}

		public void Dispose()
		{
			_communicationService.Dispose();
		}


#if DEBUG
		public void SendMessageToClient(object message)
		{
			_communicationService.SendAsync(message);
		}
#endif
	}
}