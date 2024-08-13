using DbManagement.Common.Contracts;
using DbManagement.Common.Implementations;
using NSubstitute;
using Session.Common.Contracts;
using Session.Contexts;
using Session.Core;
using Session.Services;
using Users;

namespace Tests.UsersManagement
{
	public class SessionsServiceTests
	{
		private readonly IDbManager _dbManagerMock = Substitute.For<IDbManager>();
		private SessionsService? _sessionsService;

		[Fact]
		public void WhenServiceInstantiated_ThenShouldCallDbManagers_GetEntitiesMethod()
		{
			_sessionsService = new SessionsService(_dbManagerMock);
			_dbManagerMock.Received(1).GetEntities<SessionInfoDto>();
		}

		[Fact]
		public void WhenSaveSessionInfoCalled_ThenShouldCallDbManagers_SaveChangesMethod()
		{
			_sessionsService = new SessionsService(_dbManagerMock);
			_sessionsService.SaveSessionInfo(SessionInfo.Empty);
			_dbManagerMock.Received(1).SaveChanges(Arg.Any<EntityDto>());
		}

		[Fact]
		public void WhenTryGetSessionInfoCalled_ThenShouldReturnFalse()
		{
			_sessionsService = new SessionsService(_dbManagerMock);
			var result = _sessionsService.TryGetPendingSessionInfo("test", out _);
			Assert.False(result);
		}

		[Fact]
		public void
			WhenTryGetSessionInfoCalled_WhenAddedSessionWithTestIdAndDbManagerReturnsEntities_ThenShouldReturnTrue_WithTestIdAsSessionId()
		{
			_dbManagerMock.GetEntities<SessionInfoDto>().Returns(new List<SessionInfoDto>
			{
				new SessionInfoDto("testId", "mockName", 3, true)
			});
			_sessionsService = new SessionsService(_dbManagerMock);

			var sessionMock = Substitute.For<ISession>();
			sessionMock.Id.Returns("testId");
			var sessionInfoMock = Substitute.For<ISessionInfo>();
			_sessionsService.TryAdd(sessionMock, sessionInfoMock);

			var result = _sessionsService.TryGetPendingSessionInfo("mockName", out var sessionInfo);
			Assert.True(result);
			Assert.Equal("testId", sessionInfo.Id);
		}
	}

	public class UsersServiceTests
	{
		private readonly IDbManager _dbManagerMock;
		private readonly UsersService _usersService;

		public UsersServiceTests()
		{
			_dbManagerMock = Substitute.For<IDbManager>();
			_usersService = new UsersService(_dbManagerMock);
		}

		[Fact]
		public void DoesUsernameExist_WhenEntitiesAreNull_ReturnsFalse()
		{
			_dbManagerMock.GetEntities<UserDto>().Returns((IEnumerable<UserDto>)null!);

			var result = _usersService.DoesUsernameExist("username");

			Assert.False(result);
		}

		[Fact]
		public void DoesUsernameExist_WhenEntitiesAreEmpty_ReturnsFalse()
		{
			_dbManagerMock.GetEntities<UserDto>().Returns(new List<UserDto>());

			var result = _usersService.DoesUsernameExist("username");

			Assert.False(result);
		}

		[Fact]
		public void DoesUsernameExist_GetEntitiesWithTestName_ReturnsTrue()
		{
			_dbManagerMock.GetEntities<UserDto>().Returns(new List<UserDto>
			{
				new UserDto("test")
			});

			var result = _usersService.DoesUsernameExist("test");

			Assert.True(result);
		}
	}
}