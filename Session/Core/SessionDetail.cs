using Session.Common.Contracts;

namespace Session.Core
{
	public abstract class SessionDetail : ISessionDetail
	{
		public SessionDetail(string sessionId)
		{
			SessionId = sessionId;
		}

		public string SessionId { get; set; }
	}
}