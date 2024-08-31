using BeautifulFundamental.Core.Identification;
using Microsoft.Extensions.DependencyInjection;

namespace BeautifulFundamental.Server.Session.Scope
{
	public interface IScope
	{
		string Id { get; }
		IServiceScope ServiceScope { get; }
		T GetService<T>() where T : class;
	}

	public class Scope : IScope
	{
		private readonly IIdentificationKey _identificationKey;

		private Scope(IServiceScope serviceScope, IIdentificationKey identificationKey)
		{
			_identificationKey = identificationKey;
			ServiceScope = serviceScope;
		}

		public IServiceScope ServiceScope { get; }
		public string Id => _identificationKey.SessionId;

		public T GetService<T>() where T : class
		{
			return ServiceScope.ServiceProvider.GetRequiredService<T>();
		}

		public static Scope Create(IServiceScope scope)
		{
			var sessionKey = scope.ServiceProvider.GetRequiredService<IIdentificationKey>();
			return new Scope(scope, sessionKey);
		}

		public void Dispose()
		{
			ServiceScope.Dispose();
		}
	}
}