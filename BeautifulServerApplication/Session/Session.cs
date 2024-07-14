using CoreHelpers;
using Microsoft.Extensions.DependencyInjection;
using Remote.Core.Communication;
using Serilog;

namespace BeautifulServerApplication.Session
{
	internal interface ISession
	{
		string Id { get; }

		void Start();
	}

	internal class Session : ISession
	{
		private readonly IServiceProvider _scopedServiceProvider;

		private Session(IServiceProvider scopedServiceProvider)
		{
			Id = GuidIdCreator.CreateString();

			_scopedServiceProvider = scopedServiceProvider;
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
			var communicationService = _scopedServiceProvider.GetRequiredService<ICommunicationService>();

			try
			{
				communicationService.Start();
			}
			catch (NullReferenceException nullReferenceException)
			{
				Log.Error($"Cannot start communication for this session: {Id}" +
				          $"Possible no client is set to the communication service. Check <cs_setClient>. Result = {communicationService.IsClientSet}" +
				          $"{nullReferenceException.Message}");

				if (!communicationService.IsClientSet)
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

		public static ISession Create(IServiceProvider scopedServiceProvider)
		{
			return new Session(scopedServiceProvider);
		}

		#endregion
	}
}