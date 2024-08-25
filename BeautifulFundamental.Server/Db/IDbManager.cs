using BeautifulFundamental.Core.Identification;
using BeautifulFundamental.Server.Session.Contracts.Context.Db;

namespace BeautifulFundamental.Server.Db
{
	public interface IDbManager
	{
		IEnumerable<T>? GetEntities<T>(IIdentificationKey? sessionKey = null) where T : EntityDto;
		void SaveChanges<T>(T dto, IIdentificationKey? sessionKey = null) where T : EntityDto;
		IContextCollection? GetContextCollection(string requestedTypeName);
	}
}