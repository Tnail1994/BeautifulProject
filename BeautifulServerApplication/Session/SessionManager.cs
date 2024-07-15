using Microsoft.Extensions.Hosting;
using Remote.Core.Communication;
using Remote.Server.Common.Contracts;
using Serilog;
using System.Collections.Concurrent;
using System.Net.Sockets;
using Microsoft.Extensions.DependencyInjection;
using Remote.Core.Communication.Client;

namespace BeautifulServerApplication.Session;

internal interface ISessionManager
{
}

internal class SessionManager : ISessionManager, IHostedService
{
	private readonly IAsyncSocketServer _asyncSocketServer;

	private readonly ConcurrentDictionary<string, ISession> _sessions = new();

	private CancellationToken _cancellationToken;
	private readonly ISessionFactory _sessionFactory;
	private readonly IScopeFactory _scopeFactory;
	private readonly IAsyncClientFactory _asyncClientFactory;

	public SessionManager(IAsyncSocketServer asyncSocketServer, ISessionFactory sessionFactory,
		IScopeFactory scopeFactory, IAsyncClientFactory asyncClientFactory)
	{
		_sessionFactory = sessionFactory;
		_scopeFactory = scopeFactory;
		_asyncSocketServer = asyncSocketServer;
		_asyncClientFactory = asyncClientFactory;

		_asyncSocketServer.NewConnectionOccured += OnNewConnectionOccured;
	}

	public async Task StartAsync(CancellationToken cancellationToken)
	{
		_cancellationToken = cancellationToken;

		StartServices();
		await StartServer();
	}

	#region Start

	private void StartServices()
	{
		Log.Information("Starting services...");
	}

	private async Task StartServer()
	{
		await _asyncSocketServer.StartAsync();
	}

	#endregion

	private void OnNewConnectionOccured(Socket socket)
	{
		Log.Information("Starting new session ...");
		StartNewSession(socket);
	}

	private void StartNewSession(Socket socket)
	{
		var scope = _scopeFactory.Create();

		if (socket == null)
			throw new InvalidOperationException("[SessionManager] Socket is not set.");

		if (scope == null)
			throw new InvalidOperationException("[SessionManager] Scope is not set.");

		var asyncClient = _asyncClientFactory.Create(socket);
		var communicationService = scope.ServiceProvider.GetRequiredService<ICommunicationService>();
		communicationService.SetClient(asyncClient);

		var session = _sessionFactory.Create(communicationService);

		Log.Information($"New session with Id {session.Id} created.");

		_sessions.TryAdd(session.Id, session);

		session.Start();

		Log.Information($"New session with Id {session.Id} started.");
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
		Log.Information("Stopping services...");
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
}