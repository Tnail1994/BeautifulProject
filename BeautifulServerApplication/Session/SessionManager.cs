using System.Collections.Concurrent;
using System.Net.Sockets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Remote.Server.Common.Contracts;
using Serilog;

namespace BeautifulServerApplication.Session;

internal interface ISessionManager
{
}

internal class SessionManager : ISessionManager, IHostedService
{
	private readonly IServiceProvider _serviceProvider;
	private readonly IAsyncSocketServer _asyncSocketServer;

	private readonly ConcurrentDictionary<string, ISession> _sessions = new();

	private CancellationToken _cancellationToken;

	public SessionManager(IServiceProvider serviceProvider)
	{
		_serviceProvider = serviceProvider;

		_asyncSocketServer = _serviceProvider.GetRequiredService<IAsyncSocketServer>();
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
		Task.Factory.StartNew(() =>
		{
			var scope = _serviceProvider.CreateScope();
			var sessionFactory = _serviceProvider.GetRequiredService<ISessionFactory>();

			sessionFactory.AddScope(scope);
			sessionFactory.AddSocket(socket);

			var session = sessionFactory.Create();

			Log.Information($"New session with Id {session.Id} created.");

			_sessions.TryAdd(session.Id, session);

			session.Start();

			Log.Information($"New session with Id {session.Id} started.");
		}, _cancellationToken);
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