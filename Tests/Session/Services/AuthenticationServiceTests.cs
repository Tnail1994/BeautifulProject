using NSubstitute;
using Remote.Communication.Common.Contracts;
using Session.Common.Contracts;
using Session.Services.Authorization;
using SharedBeautifulData.Messages.Login;

namespace Tests.Session.Services
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
			var communicationService = Substitute.For<ICommunicationService>();
			var loginReply = new LoginReply { Token = string.Empty };
			communicationService.SendAndReceiveAsync<LoginReply>(Arg.Any<LoginRequest>()).Returns(loginReply);

			var result = await _authenticationService.Authorize(communicationService);

			Assert.False(result.IsAuthorized);
		}

		[Fact]
		public async Task Authorize_WhenUsernameDoesNotExist_ReturnsFalse()
		{
			var communicationService = Substitute.For<ICommunicationService>();
			var loginReply = new LoginReply { Token = "username" };
			communicationService.SendAndReceiveAsync<LoginReply>(Arg.Any<LoginRequest>()).Returns(loginReply);
			_userServiceMock.DoesUsernameExist("username").Returns(false);

			var result = await _authenticationService.Authorize(communicationService);

			Assert.False(result.IsAuthorized);
		}

		[Fact]
		public async Task Authorize_WhenUsernameExists_ReturnsTrue()
		{
			var communicationService = Substitute.For<ICommunicationService>();
			var loginReply = new LoginReply { Token = "username" };
			communicationService.SendAndReceiveAsync<LoginReply>(Arg.Any<LoginRequest>()).Returns(loginReply);
			_userServiceMock.DoesUsernameExist("username").Returns(true);

			var result = await _authenticationService.Authorize(communicationService);

			Assert.True(result.IsAuthorized);
		}
	}
}