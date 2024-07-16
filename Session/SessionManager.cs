﻿using System.Collections.Concurrent;
using System.Net.Sockets;
using Core.Extensions;
using Core.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Remote.Communication.Common.Client.Contracts;
using Remote.Communication.Common.Client.Implementations;
using Remote.Communication.Common.Contracts;
using Remote.Server.Common.Contracts;
using Session.Common.Contracts;
using Session.Common.Contracts.Services;
using Session.Common.Implementations;

namespace Session
{
	public class SessionManager : ISessionManager
	{
		private readonly IAsyncServer _asyncSocketServer;

		private readonly ConcurrentDictionary<string, ISession> _sessions = new();
		private readonly ConcurrentDictionary<string, ISession> _pendingSessions = new();

		private readonly ISessionFactory _sessionFactory;
		private readonly IScopeFactory _scopeFactory;
		private readonly IAsyncClientFactory _asyncClientFactory;

		private readonly AsyncClientSettings _asyncClientSettings;

		public SessionManager(IAsyncServer asyncSocketServer, ISessionFactory sessionFactory,
			IScopeFactory scopeFactory, IAsyncClientFactory asyncClientFactory,
			AsyncClientSettings asyncClientSettings)
		{
			_sessionFactory = sessionFactory;
			_scopeFactory = scopeFactory;
			_asyncSocketServer = asyncSocketServer;
			_asyncClientFactory = asyncClientFactory;

			_asyncClientSettings = asyncClientSettings;

			_asyncSocketServer.NewConnectionOccured += OnNewConnectionOccured;
		}

		public async Task StartAsync(CancellationToken cancellationToken)
		{
			StartServices();
			await StartServer();
		}

		#region Start

		private void StartServices()
		{
			this.LogInfo("Starting services...", "server");
		}

		private async Task StartServer()
		{
			await _asyncSocketServer.StartAsync();
		}

		#endregion

		private void OnNewConnectionOccured(TcpClient client)
		{
			this.LogInfo("Starting new session ...", "server");
			StartNewSession(client);
		}

		private void StartNewSession(TcpClient client)
		{
			var scope = _scopeFactory.Create();

			if (client == null)
				throw new SessionManagerException("Socket is not set.", 1);

			if (scope == null)
				throw new SessionManagerException("Scope is not set.", 2);

			this.LogDebug($"Creating session key.", "server");
			var sessionKey = scope.ServiceProvider.GetRequiredService<ISessionKey>();
			this.LogDebug($"Creating client.", "server");
			var asyncClient = _asyncClientFactory.Create(client, _asyncClientSettings);
			this.LogDebug($"Client {asyncClient.Id} matches to Session {sessionKey.SessionId}", "server");

			var communicationService = scope.ServiceProvider.GetRequiredService<ICommunicationService>();
			communicationService.SetClient(asyncClient);

			var session = _sessionFactory.Create(communicationService, sessionKey);
			session.SessionOnHold += OnSessionOnHold;
			this.LogInfo($"New session with Id {session.Id} created.", "server");

			_sessions.TryAdd(session.Id, session);

			session.Start();

			this.LogInfo($"New session with Id {session.Id} started.", "server");
		}

		private void OnSessionOnHold(object? sender, string e)
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

			session.Stop();

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

			StopServices();
			StopServer();
			return Task.CompletedTask;
		}

		#region Stop

		private void StopServices()
		{
			this.LogInfo("Stopping services...");
		}

		private void StopServer()
		{
			_asyncSocketServer.Stop();
			foreach (var session in _sessions)
			{
				session.Value.Stop();
			}
		}

		#endregion

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