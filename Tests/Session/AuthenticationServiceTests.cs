using NSubstitute;
using Remote.Communication.Common.Contracts;
using Session;
using Session.Common.Contracts;
using SharedBeautifulData.Messages.Login;

namespace Tests.Session
{
	public class AuthenticationServiceTests
	{
		private readonly AuthenticationService _authenticationService;
		private readonly IUsersService _userServiceMock;

		public AuthenticationServiceTests()
		{
			_userServiceMock = Substitute.For<IUsersService>();
			_authenticationService = new AuthenticationService(_userServiceMock);
		}

		[Fact]
		public async Task Authorize_WhenUsernameIsEmpty_ReturnsFalse()
		{
			// Arrange
			var communicationService = Substitute.For<ICommunicationService>();
			var loginReply = new LoginReply { Token = string.Empty };
			communicationService.SendAndReceiveAsync<LoginReply>(Arg.Any<LoginRequest>()).Returns(loginReply);

			// Act
			var result = await _authenticationService.Authorize(communicationService);

			// Assert
			Assert.False(result);
		}

		[Fact]
		public async Task Authorize_WhenUsernameDoesNotExist_ReturnsFalse()
		{
			// Arrange
			var communicationService = Substitute.For<ICommunicationService>();
			var loginReply = new LoginReply { Token = "username" };
			communicationService.SendAndReceiveAsync<LoginReply>(Arg.Any<LoginRequest>()).Returns(loginReply);
			_userServiceMock.DoesUsernameExist("username").Returns(false);

			// Act
			var result = await _authenticationService.Authorize(communicationService);

			// Assert
			Assert.False(result);
		}

		[Fact]
		public async Task Authorize_WhenUsernameExists_ReturnsTrue()
		{
			var communicationService = Substitute.For<ICommunicationService>();
			var loginReply = new LoginReply { Token = "username" };
			communicationService.SendAndReceiveAsync<LoginReply>(Arg.Any<LoginRequest>()).Returns(loginReply);
			_userServiceMock.DoesUsernameExist("username").Returns(true);

			var result = await _authenticationService.Authorize(communicationService);

			Assert.True(result);
		}
	}
}