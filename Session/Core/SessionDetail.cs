using Session.Common.Contracts;
using Session.Common.Implementations;

namespace Session.Core
{
	public abstract class SessionDetail : ISessionDetail
	{
		public SessionDetail(ISessionKey sessionKey)
		{
			SessionKey = sessionKey;
		}

		public event EventHandler<DetailsChangedArgs>? DetailsChanged;
		public ISessionKey SessionKey { get; }

		public string SessionId => SessionKey.SessionId;
		public abstract IEntryDto Convert();
		public string TypeName => GetType().Name;


		protected void TriggerUpdate()
		{
			DetailsChanged?.Invoke(this, new DetailsChangedArgs(SessionKey, TypeName));
		}
	}
}