using DbManagement.Common.Contracts;
using DbManagement.Contexts;
using NSubstitute;
using Session.Services;

namespace Tests.Session.Services
{
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