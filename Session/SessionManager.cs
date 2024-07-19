using System.Collections.Concurrent;
using System.Net.Sockets;
using Core.Extensions;
using Core.Helpers;
using DbManagement.Common.Contracts;
using Remote.Communication.Common.Client.Contracts;
using Remote.Server.Common.Contracts;
using Session.Common.Contracts;
using Session.Common.Implementations;
using SharedBeautifulData;

namespace Session
{
	public class SessionManager : ISessionManager
	{
		private readonly IAsyncServer _asyncSocketServer;

		private readonly ConcurrentDictionary<string, ISession> _sessions = new();

		private readonly IScopeManager _scopeManager;
		private readonly IDbManager _dbManager;

		public SessionManager(IAsyncServer asyncSocketServer, IScopeManager scopeManager, IDbManager dbManager)
		{
			_scopeManager = scopeManager;
			_dbManager = dbManager;
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

			// Todo authorize
			if (_dbManager.GetEntities<User>() != null)
			{
			}

			session.SessionStopped += OnSessionStopped;
			this.LogInfo($"New session with Id {session.Id} created.", "server");

			_sessions.TryAdd(session.Id, session);

			// todo; Check if the session is a pending session registered in the database
			// todo; if so, then reestablish the session with the provided context

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

			var removeResult = _sessions.TryRemove(session.Id, out _);

			if (!removeResult)
			{
				this.LogError($"Cannot remove session with Id {session.Id} from dictionary.", "server");
				return;
			}

			HandleStoppedSession(session);
		}

		private void HandleStoppedSession(ISession session)
		{
			// todo; Here we determine some state and safe it to the database

			// After that state saving handle, we can dispose the scope
			_scopeManager.Destroy(session.Id);
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			_asyncSocketServer.NewConnectionOccured -= OnNewConnectionOccured;

			_sessions.Clear();

			return Task.CompletedTask;
		}

#if DEBUG
		public void SendMessageToRandomClient(object messageObject)
		{
			var randomSession = _sessions.Values.MinBy(_ => GuidIdCreator.CreateString());
			randomSession?.SendMessageToClient(messageObject);
		}

		public void SendMessageToAllClients(object messageObject)
		{
			foreach (var session in _sessions)
			{
				session.Value.SendMessageToClient(messageObject);
			}
		}
#endif
	}
}