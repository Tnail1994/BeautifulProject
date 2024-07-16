using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Remote.Server.Common.Contracts;
using System.Net.Sockets;
using Remote.Communication.Common.Client.Contracts;
using Session;
using Session.Common.Contracts;
using Session.Common.Implementations;
using Remote.Communication.Common.Contracts;
using Session.Common.Contracts.Services;
using SharedBeautifulServices.Common;

namespace Tests.Session
{
	public class SessionManagerTests
	{
		private readonly ISessionManager _sessionManager;
		private readonly IAsyncServer _asyncSocketServerMock;
		private readonly ISessionFactory _sessionFactoryMock;
		private readonly IScopeFactory _scopeFactoryMock;
		private readonly CancellationTokenSource _cancelledTokenSource;
		private readonly IAsyncClientFactory _asyncClientFactoryMock;
		private readonly ISessionKey _sessionKeyMock;
		private readonly ICheckAliveService _checkAliveService;
		private IServiceScope? _scopeMock;
		private IServiceProvider? _serviceProviderMock;

		public SessionManagerTests()
		{
			_asyncSocketServerMock = Substitute.For<IAsyncServer>();
			_sessionFactoryMock = Substitute.For<ISessionFactory>();
			_scopeFactoryMock = Substitute.For<IScopeFactory>();
			_asyncClientFactoryMock = Substitute.For<IAsyncClientFactory>();
			_sessionKeyMock = Substitute.For<ISessionKey>();
			_checkAliveService = Substitute.For<ICheckAliveService>();

			_cancelledTokenSource = new CancellationTokenSource();

			_sessionManager = new SessionManager(_asyncSocketServerMock, _sessionFactoryMock, _scopeFactoryMock,
				_asyncClientFactoryMock);
		}

		private void BaseProviderMocking()
		{
			_serviceProviderMock = Substitute.For<IServiceProvider>();
			_scopeMock = Substitute.For<IServiceScope>();
			_scopeMock.ServiceProvider.Returns(_serviceProviderMock);
		}

		[Fact]
		public async void WhenStartingSessionManager_ThenAsyncSocketServerShouldStartAsWell()
		{
			BaseProviderMocking();

			await _sessionManager.StartAsync(_cancelledTokenSource.Token);
			await _asyncSocketServerMock.Received(1).StartAsync();
			await _cancelledTokenSource.CancelAsync();
		}

		[Fact]
		public void WhenNewConnectionOccured_ThenSessionManagerShouldCreateAndStartNewSession()
		{
			BaseProviderMocking();

			var dummySocket = CreateDummySocket();
			var session = Substitute.For<ISession>();
			var communicationServiceMock = Substitute.For<ICommunicationService>();
			_scopeFactoryMock.Create().Returns(_scopeMock);

			_serviceProviderMock?.GetService<ICommunicationService>().Returns(communicationServiceMock);
			_serviceProviderMock?.GetService<ISessionKey>().Returns(_sessionKeyMock);
			_serviceProviderMock?.GetService<ICheckAliveService>().Returns(_checkAliveService);
			_sessionFactoryMock.Create(communicationServiceMock, _sessionKeyMock, _checkAliveService).Returns(session);

			RaiseNewConnectionEvent(dummySocket);

			_scopeFactoryMock.Received(1).Create();
			_asyncClientFactoryMock.Received(1).Create(Arg.Any<TcpClient>());
			session.Received(1).Start();
		}

		private void RaiseNewConnectionEvent(TcpClient? dummySocket)
		{
			_asyncSocketServerMock.NewConnectionOccured += Raise.Event<Action<TcpClient>>(dummySocket);
		}

		private static TcpClient CreateDummySocket()
		{
			var dummySocket = new TcpClient();
			return dummySocket;
		}

		[Fact]
		public void WhenStoppingSessionManager_ThenAsyncSocketServerShouldStopWell()
		{
			BaseProviderMocking();

			_sessionManager.StopAsync(_cancelledTokenSource.Token);
			_asyncSocketServerMock.Received(1).Stop();
		}

		// Assert exceptions for not set socket and scope
		[Fact]
		public void WhenNewConnectionOccuredWithNullSocket_ThenShouldThrowSessionManagerException()
		{
			BaseProviderMocking();

			Assert.Throws<SessionManagerException>(() => RaiseNewConnectionEvent(null));
		}

		[Fact]
		public void WhenNewConnectionOccuredWithNullScope_ThenShouldThrowSessionManagerException()
		{
			var dummySocket = CreateDummySocket();

			_scopeFactoryMock.Create().Returns(null as IServiceScope);
			// We're assuming an InvalidOperationException because the Substitute framework mocks
			// ServiceProvider. But cannot resolve ICommunicationService from ServiceProvider.
			Assert.Throws<SessionManagerException>(() => RaiseNewConnectionEvent(dummySocket));
		}
	}
}