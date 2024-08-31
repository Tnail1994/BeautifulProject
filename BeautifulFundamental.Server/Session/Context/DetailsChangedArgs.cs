using BeautifulFundamental.Core.Identification;

namespace BeautifulFundamental.Server.Session.Context
{
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