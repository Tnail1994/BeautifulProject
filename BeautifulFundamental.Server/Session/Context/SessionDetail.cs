using BeautifulFundamental.Core.Identification;
using BeautifulFundamental.Server.Session.Contracts.Context;
using BeautifulFundamental.Server.Session.Contracts.Context.Db;

namespace BeautifulFundamental.Server.Session.Context
{
	public abstract class SessionDetail : ISessionDetail
	{
		public SessionDetail(IIdentificationKey identificationKey)
		{
			IdentificationKey = identificationKey;
		}

		public event EventHandler<DetailsChangedArgs>? DetailsChanged;
		public IIdentificationKey IdentificationKey { get; }

		public string SessionId => IdentificationKey.SessionId;
		public abstract IEntryDto Convert();
		public string TypeName => GetType().Name;


		protected void TriggerUpdate()
		{
			DetailsChanged?.Invoke(this, new DetailsChangedArgs(IdentificationKey, TypeName));
		}
	}
}