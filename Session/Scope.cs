using Microsoft.Extensions.DependencyInjection;
using Session.Common.Contracts;

namespace Session
{
	public class Scope : IScope
	{
		private Scope(string id, IServiceScope serviceScope)
		{
			Id = id;
			ServiceScope = serviceScope;
		}

		public string Id { get; }
		public IServiceScope ServiceScope { get; }

		public T GetService<T>() where T : class
		{
			return ServiceScope.ServiceProvider.GetRequiredService<T>();
		}

		public static Scope Create(string id, IServiceScope scope)
		{
			return new Scope(id, scope);
		}

		public void Dispose()
		{
			ServiceScope.Dispose();
		}
	}
}