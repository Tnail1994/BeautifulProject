using DbManagement.Common.Implementations;

namespace DbManagement.Common.Contracts
{
	public interface IDbContext
	{
		string Id { get; }
		string TypeNameOfCollectionEntries { get; }
		IEnumerable<object>? GetEntities();

		void AddEntity<TDto>(TDto dto) where TDto : EntityDto;
	}
}