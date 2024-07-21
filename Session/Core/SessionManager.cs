using Core.Extensions;

#if DEBUG
using Core.Helpers;
#endif

using Remote.Communication.Common.Client.Contracts;
using Remote.Server.Common.Contracts;
using Session.Common.Contracts;
using Session.Common.Implementations;
using System.Net.Sockets;

namespace Session.Core
{
	public class SessionManager : ISessionManager
	{
		private readonly IAsyncServer _asyncSocketServer;
		private readonly IScopeManager _scopeManager;

#if DEBUG
		private readonly ISessionsService _sessionsService;
#endif


		public SessionManager(IAsyncServer asyncSocketServer, IScopeManager scopeManager
#if DEBUG
			, ISessionsService sessionsService
#endif
		)
		{
			_scopeManager = scopeManager;
#if DEBUG
			_sessionsService = sessionsService;
#endif

			_asyncSocketServer = asyncSocketServer;

			_asyncSocketServer.NewConnectionOccured += OnNewConnectionOccured;
		}

		public async Task StartAsync(CancellationToken cancellationToken)
		{
			await _asyncSocketServer.StartAsync();
		}

		private void OnNewConnectionOccured(TcpClient client)
		{
			this.LogInfo("Starting new session ...", "server");
			StartNewSession(client);
		}

		private void StartNewSession(TcpClient client)
		{
			var session = BuildSession(client);

			session.SessionStopped += OnSessionStopped;

			session.Start();
			this.LogInfo($"New session with Id {session.Id} started.", "server");
		}

		private ISession BuildSession(TcpClient client)
		{
			var scope = _scopeManager.Create();

			if (client == null)
				throw new SessionManagerException("Socket is not set.", 1);

			if (scope == null)
				throw new SessionManagerException("Scope is not set.", 2);

			scope.GetService<IAsyncClientFactory>().Init(client);
			return scope.GetService<ISession>();
		}

		private void OnSessionStopped(object? sender, string e)
		{
			if (sender is not ISession session)
			{
				this.LogFatal(
					$"sender is not ISession. This is fatal. SessionManager cannot remove session. Error Handling failed!",
					"server");
				return;
			}

			_scopeManager.Destroy(session.Id);
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			_asyncSocketServer.NewConnectionOccured -= OnNewConnectionOccured;
			return Task.CompletedTask;
		}

#if DEBUG
		public void SendMessageToRandomClient(object messageObject)
		{
			var randomSession = _sessionsService.GetSessions().MinBy(_ => GuidIdCreator.CreateString());
			randomSession?.SendMessageToClient(messageObject);
		}

		public void SendMessageToAllClients(object messageObject)
		{
			foreach (var session in _sessionsService.GetSessions())
			{
				session.SendMessageToClient(messageObject);
			}
		}
#endif
	}
}