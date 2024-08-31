using BeautifulFundamental.Core.Communication.Implementations;
using BeautifulFundamental.Core.Identification;
using BeautifulFundamental.Core.MessageHandling;
using BeautifulFundamental.Core.Messages.Authorize;
using BeautifulFundamental.Server.UserManagement;

namespace BeautifulFundamental.Server.Session.Core
{
	public abstract class SessionLoopBase : ISessionLoop, IDisposable
	{
		private readonly IAutoSynchronizedMessageHandler _autoSynchronizedMessageHandler;
		private readonly IUsersService _usersService;
		private readonly string _subscibeId;

		public SessionLoopBase(IIdentificationKey identificationKey,
			IAutoSynchronizedMessageHandler autoSynchronizedMessageHandler, IUsersService usersService)
		{
			_autoSynchronizedMessageHandler = autoSynchronizedMessageHandler;
			_usersService = usersService;
			_subscibeId = _autoSynchronizedMessageHandler.Subscribe<RegistrationRequest>(OnRegistrationRequestReceived);
			IdentificationKey = identificationKey;
		}

		protected virtual INetworkMessage OnRegistrationRequestReceived(INetworkMessage message)
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

		protected IIdentificationKey IdentificationKey { get; }
		protected string SessionId => IdentificationKey.SessionId;
		protected abstract void Run();

		public void Start()
		{
			Run();
		}

		public virtual void Dispose()
		{
			_autoSynchronizedMessageHandler.Unsubscribe(_subscibeId);
		}
	}
}