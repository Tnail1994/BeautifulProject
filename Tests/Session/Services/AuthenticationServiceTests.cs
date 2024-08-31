using BeautifulFundamental.Core.Communication;
using BeautifulFundamental.Core.Messages.Authorize;
using BeautifulFundamental.Server.Session.Services.Authorization;
using BeautifulFundamental.Server.UserManagement;
using NSubstitute;

namespace Tests.Session.Services
{
	public class AuthenticationServiceTests
	{
		private readonly AuthenticationService _authenticationService;
		private readonly IUsersService _userServiceMock;

		public AuthenticationServiceTests()
		{
			_userServiceMock = Substitute.For<IUsersService>();
			var authenticationSettingsMock = new AuthenticationSettings
			{
				AuthTimeoutInMinutes = 0,
				MaxAuthAttempts = 0,
				MaxReactivateAuthenticationCounter = 1
			};

			_authenticationService =
				new AuthenticationService(_userServiceMock, authenticationSettingsMock);
		}

		[Fact]
		public async Task
			Authorize_IfIdentIsEmpty_AndEmptyRequestValue_ThenCallReceiveForLoginRequest_AndReturnsFalse()
		{
			var communicationService = Substitute.For<ICommunicationService>();

			var deviceIdentReply = new DeviceIdentReply
			{
				Ident = string.Empty
			};

			var loginRequest = new LoginRequest
			{
				RequestValue = null
			};

			communicationService.SendAndReceiveAsync<DeviceIdentReply>(Arg.Any<DeviceIdentRequest>())
				.Returns(Task.FromResult(deviceIdentReply));
			communicationService.ReceiveAsync<LoginRequest>()
				.Returns(Task.FromResult(loginRequest));

			var result = await _authenticationService.Authorize(communicationService);
			await communicationService.Received().ReceiveAsync<LoginRequest>();
			Assert.False(result.IsAuthorized);
		}


		[Theory]
		[InlineData(LoginRequestType.DeviceIdent, "", false, false, null, 0, false)]
		[InlineData(LoginRequestType.DeviceIdent, "existing", false, false, null, 0, false)]
		[InlineData(LoginRequestType.DeviceIdent, "existing", true, false, null, 0, true)]
		[InlineData(LoginRequestType.DeviceIdent, "existing", true, true, null, 0, false)]
		[InlineData(LoginRequestType.DeviceIdent, "existing", true, false, "otherIdent", 0, false)]
		[InlineData(LoginRequestType.DeviceIdent, "existing", true, false, null, 1, false)]
		public async Task
			Authorize_IfIdentIsNotEmpty_AndUserExists_ThenCallReceiveForLoginRequest_AndReturnsFalse(
				LoginRequestType type, string value, bool stayActive, bool isActive, string? lastDeviceIdent,
				int reactivateCounter,
				bool result)
		{
			var communicationService = Substitute.For<ICommunicationService>();

			var deviceIdent = "notEmpty";
			var deviceIdentReply = new DeviceIdentReply
			{
				Ident = deviceIdent
			};

			var loginRequest = new LoginRequest
			{
				RequestValue = new LoginRequestValue
				{
					Type = type,
					Value = value,
					StayActive = stayActive
				}
			};

			communicationService.SendAndReceiveAsync<DeviceIdentReply>(Arg.Any<DeviceIdentRequest>())
				.Returns(Task.FromResult(deviceIdentReply));
			communicationService.ReceiveAsync<LoginRequest>()
				.Returns(Task.FromResult(loginRequest));

			var userMock = User.Create(value, isActive, stayActive, lastDeviceIdent, reactivateCounter);
			_userServiceMock.TryGetUserByDeviceIdent(deviceIdent, out _).Returns(x =>
			{
				x[1] = userMock;
				return true;
			});

			var authRes = await _authenticationService.Authorize(communicationService);
			await communicationService.Received().ReceiveAsync<LoginRequest>();
			Assert.Equal(result, authRes.IsAuthorized);
		}

		[Fact]
		public async Task Authorize_IfIdentIsEmpty_AndUsernameDoesNotExist_ReturnsFalse()
		{
			var communicationService = Substitute.For<ICommunicationService>();

			var deviceIdentReply = new DeviceIdentReply
			{
				Ident = string.Empty
			};

			var loginRequest = new LoginRequest
			{
				RequestValue = new LoginRequestValue
				{
					Type = LoginRequestType.Username,
					Value = "notExisting"
				}
			};

			communicationService.SendAndReceiveAsync<DeviceIdentReply>(Arg.Any<DeviceIdentRequest>())
				.Returns(Task.FromResult(deviceIdentReply));
			communicationService.ReceiveAsync<LoginRequest>()
				.Returns(Task.FromResult(loginRequest));

			var result = await _authenticationService.Authorize(communicationService);

			Assert.False(result.IsAuthorized);
		}

		[Fact]
		public async Task Authorize_WhenUsernameExists_ReturnsTrue()
		{
			var communicationService = Substitute.For<ICommunicationService>();

			var deviceIdentReply = new DeviceIdentReply
			{
				Ident = string.Empty
			};

			var loginRequest = new LoginRequest
			{
				RequestValue = new LoginRequestValue
				{
					Type = LoginRequestType.Username,
					Value = "existing"
				}
			};

			var userMock = User.Create("existing", false, false, null, 0);
			_userServiceMock.TryGetUserByUsername("existing", out _).Returns(x =>
			{
				x[1] = userMock;
				return true;
			});

			communicationService.SendAndReceiveAsync<DeviceIdentReply>(Arg.Any<DeviceIdentRequest>())
				.Returns(Task.FromResult(deviceIdentReply));
			communicationService.ReceiveAsync<LoginRequest>()
				.Returns(Task.FromResult(loginRequest));

			var result = await _authenticationService.Authorize(communicationService);

			Assert.True(result.IsAuthorized);
		}
	}
}