using System.Net.Sockets;
using Microsoft.Extensions.DependencyInjection;
using Remote.Core.Communication;

namespace BeautifulServerApplication.Session
{
	public interface ISessionFactory
	{
		void AddScope(IServiceScope scope);
		void AddSocket(Socket socket);
		ISession Create();
	}

	internal class SessionFactory(IAsyncClientFactory asyncClientFactory) : ISessionFactory
	{
		private IServiceScope? _scope;
		private Socket? _socket;

		public void AddScope(IServiceScope scope)
		{
			_scope = scope;
		}

		public void AddSocket(Socket socket)
		{
			_socket = socket;
		}

		public ISession Create()
		{
			if (_socket == null)
				throw new InvalidOperationException("Socket is not set.");

			if (_scope == null)
				throw new InvalidOperationException("Scope is not set.");

			var asyncClient = asyncClientFactory.Create(_socket);
			var communicationService = _scope.ServiceProvider.GetRequiredService<ICommunicationService>();
			communicationService.SetClient(asyncClient);
			var session = Session.Create(_scope.ServiceProvider);

			Clear();

			return session;
		}

		private void Clear()
		{
			_scope = null;
			_socket = null;
		}
	}
}