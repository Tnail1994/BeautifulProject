using Session.Common.Contracts;
using Session.Common.Implementations;

namespace Session.Core
{
	public abstract class SessionLoopBase : ISessionLoop
	{
		public SessionLoopBase(ISessionKey sessionKey)
		{
			SessionKey = sessionKey;
		}

		protected ISessionKey SessionKey { get; }
		protected string SessionId => SessionKey.SessionId;
		protected abstract void Run();

		public void Start()
		{
			Run();
		}
	}
}