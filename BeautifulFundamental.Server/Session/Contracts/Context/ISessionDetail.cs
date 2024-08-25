using BeautifulFundamental.Core.Identification;
using BeautifulFundamental.Server.Session.Contracts.Context.Db;

namespace BeautifulFundamental.Server.Session.Contracts.Context
{
	public interface ISessionDetail
	{
		event EventHandler<DetailsChangedArgs> DetailsChanged;
		string SessionId { get; }
		string TypeName { get; }
		IEntryDto Convert();
	}

	public class DetailsChangedArgs
	{
		public DetailsChangedArgs(IIdentificationKey identificationKey, string detailsTypeName)
		{
			IdentificationKey = identificationKey;
			DetailsTypeName = detailsTypeName;
		}

		public string SessionId => IdentificationKey.SessionId;
		public IIdentificationKey IdentificationKey { get; }
		public string DetailsTypeName { get; }
	}
}