using BeautifulFundamental.Core.Communication.Implementations;
using BeautifulFundamental.Core.Identification;
using BeautifulFundamental.Core.MessageHandling;
using BeautifulFundamental.Core.Messages.Authorize;
using BeautifulFundamental.Server.UserManagement;

namespace BeautifulFundamental.Server.Session.Services.UserRegistration
{
	public interface IUserRegistrationService
	{
		void Start();
	}

	public class UserRegistrationService : IUserRegistrationService, IDisposable
	{
		private string? _subscribeId;

		private readonly IAutoSynchronizedMessageHandler _autoSynchronizedMessageHandler;
		private readonly IUsersService _usersService;

		public UserRegistrationService(IIdentificationKey identificationKey,
			IAutoSynchronizedMessageHandler autoSynchronizedMessageHandler, IUsersService usersService)
		{
			_autoSynchronizedMessageHandler = autoSynchronizedMessageHandler;
			_usersService = usersService;

			IdentificationKey = identificationKey;
		}

		protected IIdentificationKey IdentificationKey { get; }
		protected string SessionId => IdentificationKey.SessionId;

		protected INetworkMessage OnRegistrationRequestReceived(INetworkMessage message)
		{
			if (message is RegistrationRequest { RegistrationRequestValue: not null } registrationRequest)
			{
				var userExists =
					_usersService.TryGetUserByUsername(registrationRequest.RegistrationRequestValue.Name, out _);

				// todo if user not exists, then we can create and add new user

				return RegistrationReply.Create(!userExists);
			}

			return RegistrationReply.Create(false, "Unexpected Error occured. Please try again later.");
		}


		public void Dispose()
		{
			if (_subscribeId != null)
				_autoSynchronizedMessageHandler.Unsubscribe(_subscribeId);
		}

		public void Start()
		{
			_subscribeId =
				_autoSynchronizedMessageHandler.Subscribe<RegistrationRequest>(OnRegistrationRequestReceived);
		}
	}
}