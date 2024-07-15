using System.Net.Sockets;
using BeautifulServerApplication;
using BeautifulServerApplication.Session;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Remote.Core.Communication;
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
		private readonly IAsyncClientFactory _asyncClientFactoryMock;
		private readonly IServiceScope _scopeMock;
		private readonly IServiceProvider _serviceProviderMock;

		public SessionManagerTests()
		{
			_asyncSocketServerMock = Substitute.For<IAsyncSocketServer>();
			_sessionFactoryMock = Substitute.For<ISessionFactory>();
			_scopeFactoryMock = Substitute.For<IScopeFactory>();
			_asyncClientFactoryMock = Substitute.For<IAsyncClientFactory>();

			_serviceProviderMock = Substitute.For<IServiceProvider>();
			_scopeMock = Substitute.For<IServiceScope>();
			_scopeMock.ServiceProvider.Returns(_serviceProviderMock);

			_cancelledTokenSource = new CancellationTokenSource();
			_sessionManager = new SessionManager(_asyncSocketServerMock, _sessionFactoryMock, _scopeFactoryMock,
				_asyncClientFactoryMock);
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
			var dummySocket = CreateDummySocket();
			var session = Substitute.For<ISession>();
			var communicationServiceMock = Substitute.For<ICommunicationService>();
			_scopeFactoryMock.Create().Returns(_scopeMock);

			_serviceProviderMock.GetService<ICommunicationService>().Returns(communicationServiceMock);
			_sessionFactoryMock.Create(communicationServiceMock).Returns(session);

			_asyncSocketServerMock.NewConnectionOccured += Raise.Event<Action<Socket>>(dummySocket);

			_scopeFactoryMock.Received(1).Create();
			_asyncClientFactoryMock.Received(1).Create(Arg.Any<Socket>());
			session.Received(1).Start();
		}

		private static Socket CreateDummySocket()
		{
			var dummySocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			return dummySocket;
		}

		[Fact]
		public void WhenStoppingSessionManager_ThenAsyncSocketServerShouldStopWell()
		{
			_sessionManager.StopAsync(_cancelledTokenSource.Token);
			_asyncSocketServerMock.Received(1).Stop();
		}

		// Assert exceptions for not set socket and scope
		[Fact]
		public void WhenNewConnectionOccuredWithNullSocket_ThenShouldThrowNullOperationException()
		{
			Assert.Throws<NullReferenceException>(() =>
				_asyncSocketServerMock.NewConnectionOccured += Raise.Event<Action<Socket>>(null));
		}

		[Fact]
		public void WhenNewConnectionOccuredWithNullScope_ThenShouldThrowInvalidOperationException()
		{
			var dummySocket = CreateDummySocket();

			// We're assuming an InvalidOperationException because the Substitute framework mocks
			// ServiceProvider. But cannot resolve ICommunicationService from ServiceProvider.
			Assert.Throws<InvalidOperationException>(() =>
				_asyncSocketServerMock.NewConnectionOccured += Raise.Event<Action<Socket>>(dummySocket));
		}
	}
}