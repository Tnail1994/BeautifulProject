using NSubstitute;
using Remote.Server.Common.Contracts;
using System.Net.Sockets;
using Remote.Communication.Common.Client.Contracts;
using Remote.Communication.Common.Contracts;
using Session.Common.Contracts;
using Session.Common.Implementations;
using Session.Core;

namespace Tests.Session
{
	public class SessionManagerTests
	{
		private readonly ISessionManager _sessionManager;
		private readonly IAsyncServer _asyncSocketServerMock;
		private readonly IScopeManager _scopeManagerMock;
		private readonly CancellationTokenSource _cancelledTokenSource;

		private IScope? _scopeMock;
		private readonly IAsyncClientFactory _asyncClientMock;
		private readonly IAuthenticationService _authenticationServiceMock;

		public SessionManagerTests()
		{
			_asyncSocketServerMock = Substitute.For<IAsyncServer>();
			_scopeManagerMock = Substitute.For<IScopeManager>();
			_asyncClientMock = Substitute.For<IAsyncClientFactory>();
			_authenticationServiceMock = Substitute.For<IAuthenticationService>();

			_cancelledTokenSource = new CancellationTokenSource();

			_sessionManager = new SessionManager(_asyncSocketServerMock, _scopeManagerMock, _authenticationServiceMock);
		}

		private void BaseProviderMocking()
		{
			_scopeMock = Substitute.For<IScope>();
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

			_scopeManagerMock.Create().Returns(_scopeMock);

			_scopeMock?.GetService<ISession>().Returns(session);
			_scopeMock?.GetService<IAsyncClientFactory>().Returns(_asyncClientMock);

			RaiseNewConnectionEvent(dummySocket);

			_scopeManagerMock.Received(1).Create();
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
		public void WhenNewConnectionOccuredWithNullSocket_ThenShouldThrowSessionManagerException()
		{
			BaseProviderMocking();

			Assert.Throws<SessionManagerException>(() => RaiseNewConnectionEvent(null));
		}

		[Fact]
		public void WhenNewConnectionOccuredWithNullScope_ThenShouldThrowSessionManagerException()
		{
			var dummySocket = CreateDummySocket();

			_scopeManagerMock.Create().Returns(null as IScope);
			var communicationServiceMock = Substitute.For<ICommunicationService>();
			_authenticationServiceMock.Authorize(communicationServiceMock).Returns(Task.FromResult(true));
			// We're assuming an InvalidOperationException because the Substitute framework mocks
			// ServiceProvider. But cannot resolve ICommunicationService from ServiceProvider.
			Assert.Throws<SessionManagerException>(() => RaiseNewConnectionEvent(dummySocket));
		}
	}
}