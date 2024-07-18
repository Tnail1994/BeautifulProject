namespace Session.Common.Contracts
{
	public interface IScopeManager
	{
		IScope Create();
		void Destroy(string id);
	}
}