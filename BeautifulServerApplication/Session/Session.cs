using CoreHelpers;
using Microsoft.Extensions.DependencyInjection;
using Remote.Core.Communication;

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
			try
			{
				var communicationService = _scopedServiceProvider.GetRequiredService<ICommunicationService>();
				communicationService.Start();
			}
			catch (NullReferenceException nullReferenceException)
			{
				// When no client is set, the communication service cannot be started.
			}
			catch (Exception e)
			{
				// Todo: Log exception and improve error handling.
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