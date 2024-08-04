using System.Collections.Concurrent;
using Core.Extensions;

#if DEBUG
using Core.Helpers;
using Microsoft.Extensions.Hosting;
#endif

using Remote.Communication.Common.Client.Contracts;
using Remote.Server.Common.Contracts;
using Session.Common.Contracts;
using Session.Common.Implementations;
using System.Net.Sockets;

namespace Session.Core
{
	public class SessionManager : ISessionManager, IHostedService
	{
		private readonly IAsyncServer _asyncSocketServer;
		private readonly IScopeManager _scopeManager;

#if DEBUG
		private readonly ISessionsService _sessionsService;
#endif

		private readonly ConcurrentDictionary<string, string> _sessionIdClientIdMap = new();

		/// <summary>
		/// The ctor of the host. The host must instantiate ISessionService first, because there is a reference to the IDbManager.
		/// The IDbManager must be resolved before the IScopeManager for disposing purposes. Dispose first all scopes via IScopeManager
		/// and then the others. IoC container of .Net disposing order is:
		/// 1. Objects created last are disposed first.
		/// 2. Objects with shorter lifetimes are disposed before objects with longer lifetimes.
		/// </summary>
		public SessionManager(IAsyncServer asyncSocketServer, ISessionsService sessionsService,
			IScopeManager scopeManager)
		{
			_scopeManager = scopeManager;
#if DEBUG
			_sessionsService = sessionsService;
#else
			_ = sessionsService;
#endif

			_asyncSocketServer = asyncSocketServer;

			_asyncSocketServer.NewConnectionOccured += OnNewConnectionOccured;
		}

		public async Task StartAsync(CancellationToken cancellationToken)
		{
			await _asyncSocketServer.StartAsync();
		}

		private void OnNewConnectionOccured(KeyValuePair<string, TcpClient> clientAndId)
		{
			this.LogInfo("Starting new session ...");
			StartNewSession(clientAndId.Key, clientAndId.Value);
		}

		private void StartNewSession(string clientId, TcpClient client)
		{
			var session = BuildSession(client);
			session.SessionStopped += OnSessionStopped;

			session.Start();
			this.LogInfo($"New session with Id {session.Id} started.");

			// Saving client and session ids for disposing purposes
			_sessionIdClientIdMap.TryAdd(session.Id, clientId);
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

		private void OnSessionStopped(object? sender, SessionStoppedEventArgs sessionStoppedEventArgs)
		{
			if (sender is not ISession)
			{
				this.LogFatal(
					$"sender is not ISession. This is fatal. SessionManager cannot remove session. Error Handling failed!");
				return;
			}

			this.LogDebug(
				$"[SessionManager] session stopped, start destroying it {sessionStoppedEventArgs.SessionKey.SessionId}");

			_scopeManager.Destroy(sessionStoppedEventArgs.SessionKey);

			if (_sessionIdClientIdMap.TryRemove(sessionStoppedEventArgs.SessionKey.InstantiatedSessionId,
				    out var clientId))

				_asyncSocketServer.Remove(clientId);
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