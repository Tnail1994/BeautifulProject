namespace DbManagement.Common.Contracts
{
	public interface IDbContextSettings
	{
		string ServerAdresse { get; init; }
		int Port { get; init; }
		string DatabaseName { get; init; }
		string UserId { get; init; }
		string Password { get; init; }
	}
}