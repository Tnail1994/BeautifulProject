using System.Net.Sockets;
using BeautifulServerApplication;
using BeautifulServerApplication.Session;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Remote.Server.Common.Contracts;

namespace Tests.BeautifulServerApplication.Session
{
	public class SessionManagerTests
	{
		private readonly SessionManager _sessionManager;
		private readonly IAsyncSocketServer _asyncSocketServerMock;
		private readonly ISessionFactory _sessionFactoryMock;
		private readonly IScopeFactory _scopeFactoryMock;
		private readonly CancellationTokenSource _cancelledTokenSource;

		public SessionManagerTests()
		{
			_asyncSocketServerMock = Substitute.For<IAsyncSocketServer>();
			_sessionFactoryMock = Substitute.For<ISessionFactory>();
			_scopeFactoryMock = Substitute.For<IScopeFactory>();

			_cancelledTokenSource = new CancellationTokenSource();
			_sessionManager = new SessionManager(_asyncSocketServerMock, _sessionFactoryMock, _scopeFactoryMock);
		}

		[Fact]
		public async void WhenStartingSessionManager_ThenAsyncSocketServerShouldStartAsWell()
		{
			await _sessionManager.StartAsync(_cancelledTokenSource.Token);
			await _asyncSocketServerMock.Received(1).StartAsync(Arg.Any<int>(), Arg.Any<int>());
			await _cancelledTokenSource.CancelAsync();
		}

		[Fact]
		public void WhenNewConnectionOccured_ThenSessionManagerShouldCreateAndStartNewSession()
		{
			var dummySocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

			var session = Substitute.For<ISession>();
			_sessionFactoryMock.Create().Returns(session);

			_asyncSocketServerMock.NewConnectionOccured += Raise.Event<Action<Socket>>(dummySocket);
			_sessionFactoryMock.Received(1).AddSocket(dummySocket);
			_sessionFactoryMock.Received(1).AddScope(Arg.Any<IServiceScope>());

			session.Received(1).Start();
		}

		[Fact]
		public void WhenStoppingSessionManager_ThenAsyncSocketServerShouldStopWell()
		{
			_sessionManager.StopAsync(_cancelledTokenSource.Token);
			_asyncSocketServerMock.Received(1).Stop();
		}
	}
}