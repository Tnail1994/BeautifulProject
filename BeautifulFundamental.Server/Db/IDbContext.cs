namespace BeautifulFundamental.Server.Db
{
	public interface IDbContext
	{
		string Id { get; }
		string TypeNameOfCollectionEntries { get; }
		IEnumerable<object> GetEntities();

		void AddEntity<TDto>(TDto dto) where TDto : EntityDto;
	}
}