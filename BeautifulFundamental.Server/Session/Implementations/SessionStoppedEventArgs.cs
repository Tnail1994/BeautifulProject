using BeautifulFundamental.Core.Identification;

namespace BeautifulFundamental.Server.Session.Implementations
{
	public class SessionStoppedEventArgs : EventArgs
	{
		public IIdentificationKey IdentificationKey { get; }
		public string Reason { get; }

		private SessionStoppedEventArgs(IIdentificationKey identificationKey, string reason)
		{
			IdentificationKey = identificationKey;
			Reason = reason;
		}

		public static SessionStoppedEventArgs Create(IIdentificationKey identificationKey, string reason)
		{
			return new SessionStoppedEventArgs(identificationKey, reason);
		}
	}
}