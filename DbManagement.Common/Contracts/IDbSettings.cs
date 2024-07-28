namespace DbManagement.Common.Contracts
{
	public interface IDbSettings
	{
		int CachingTimeInSeconds { get; init; }
	}
}