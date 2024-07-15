using CoreHelpers;
using Microsoft.Extensions.DependencyInjection;
using Remote.Core.Communication;
using Serilog;

namespace BeautifulServerApplication.Session
{
	public interface ISession
	{
		string Id { get; }

		void Start();
	}

	internal class Session : ISession
	{
		private readonly ICommunicationService _communicationService;

		private Session(ICommunicationService communicationService)
		{
			Id = GuidIdCreator.CreateString();

			_communicationService = communicationService;
		}

		public string Id { get; }

		public void Start()
		{
			// From here the session can be used to communicate with the client.
			// All what happens here, should happen parallel to the main thread.
			// So beware of writing to the console or doing other blocking operations.
			// Need to define an own logging system for this session overall.

			StartCommunicationService();
		}

		private void StartCommunicationService()
		{
			try
			{
				_communicationService.Start();
			}
			catch (NullReferenceException nullReferenceException)
			{
				Log.Error($"Cannot start communication for this session: {Id}" +
				          $"Possible no client is set to the communication service. Check <cs_setClient>. Result = {_communicationService.IsClientSet}" +
				          $"{nullReferenceException.Message}");

				if (!_communicationService.IsClientSet)
				{
					// Todo retry to set a client
				}
			}
			catch (Exception ex)
			{
				Log.Fatal($"!!! Unexpected {Id}" +
				          $"{ex.Message}");
			}
		}

		#region Factory

		public static ISession Create(ICommunicationService communicationService)
		{
			return new Session(communicationService);
		}

		#endregion
	}
}