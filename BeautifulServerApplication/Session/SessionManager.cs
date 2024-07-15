using Microsoft.Extensions.Hosting;
using Remote.Server.Common.Contracts;
using Serilog;
using System.Collections.Concurrent;
using System.Net.Sockets;

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

	public SessionManager(IAsyncSocketServer asyncSocketServer, ISessionFactory sessionFactory,
		IScopeFactory scopeFactory)
	{
		_sessionFactory = sessionFactory;
		_scopeFactory = scopeFactory;
		_asyncSocketServer = asyncSocketServer;

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

		_sessionFactory.AddScope(scope);
		_sessionFactory.AddSocket(socket);

		var session = _sessionFactory.Create();

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
	}

	#endregion
}