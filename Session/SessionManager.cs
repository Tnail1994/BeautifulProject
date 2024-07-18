using System.Collections.Concurrent;
using System.Net.Sockets;
using Core.Extensions;
using Core.Helpers;
using Remote.Communication.Common.Client.Contracts;
using Remote.Server.Common.Contracts;
using Session.Common.Contracts;
using Session.Common.Implementations;

namespace Session
{
	public class SessionManager : ISessionManager
	{
		private readonly IAsyncServer _asyncSocketServer;

		private readonly ConcurrentDictionary<string, ISession> _sessions = new();
		private readonly ConcurrentDictionary<string, ISession> _pendingSessions = new();

		private readonly IScopeManager _scopeManager;

		public SessionManager(IAsyncServer asyncSocketServer, IScopeManager scopeManager)
		{
			_scopeManager = scopeManager;
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
			this.LogInfo($"New session with Id {session.Id} created.", "server");

			_sessions.TryAdd(session.Id, session);

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

			var removeResult = _sessions.TryRemove(session.Id, out var pendingSession);

			if (!removeResult)
			{
				this.LogError($"Cannot remove session with Id {session.Id} from dictionary.", "server");
				return;
			}

			if (pendingSession == null)
			{
				this.LogWarning($"Cannot pend session.", "server");
				return;
			}

			this.LogDebug($"Pending session {pendingSession.Id}, for possibly restart this session.",
				"server");
			_pendingSessions.TryAdd(pendingSession.Id, pendingSession);
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			_asyncSocketServer.NewConnectionOccured -= OnNewConnectionOccured;

			foreach (var session in _sessions)
			{
				session.Value.Stop();
			}

			_sessions.Clear();
			_pendingSessions.Clear();

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