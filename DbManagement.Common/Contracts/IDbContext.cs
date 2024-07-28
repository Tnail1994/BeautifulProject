using DbManagement.Common.Implementations;

namespace DbManagement.Common.Contracts
{
	public interface IDbContext
	{
		string Id { get; }
		string TypeNameOfCollectionEntries { get; }
		IEnumerable<object>? GetEntities();
		IEnumerable<TDto> GetEntities<TDto>() where TDto : EntityDto;

		void AddEntity<TDto>(TDto dto) where TDto : EntityDto;
	}
}