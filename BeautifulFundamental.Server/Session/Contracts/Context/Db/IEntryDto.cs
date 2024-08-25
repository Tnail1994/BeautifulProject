using BeautifulFundamental.Core.Identification;

namespace BeautifulFundamental.Server.Session.Contracts.Context.Db
{
	public interface IEntryDto
	{
		string TypeName { get; }
		ISessionDetail Convert(IIdentificationKey identificationKey);
		void Update(ISessionDetail sessionDetail);
	}
}