﻿using Core.Extensions;
using Remote.Communication.Common.Contracts;
using Remote.Communication.Common.Implementations;
using Session.Common.Contracts;
using SharedBeautifulData.Messages.Authorize;
using SharedBeautifulData.Objects;

namespace Session.Services.Authorization
{
	public class AuthenticationService : IAuthenticationService
	{
		private readonly IUsersService _usersService;
		private readonly IAuthenticationSettings _settings;

		public AuthenticationService(IUsersService usersService, IAuthenticationSettings settings)
		{
			_usersService = usersService;
			_settings = settings;
		}

		public async Task<IAuthorizationInfo> Authorize(ICommunicationService communicationService)
		{
			var deviceIdent =
				await communicationService.SendAndReceiveAsync<DeviceIdentReply>(new DeviceIdentRequest());

			if (!string.IsNullOrEmpty(deviceIdent.Ident) &&
			    _usersService.TryGetUserByDeviceIdent(deviceIdent.Ident, out var user) &&
			    user is { IsNotActive: true, StayActive: true } &&
			    CheckLastLoggedInDeviceIdent(user, deviceIdent) &&
			    user.ReactivateCounter <
			    _settings.MaxReactivateAuthenticationCounter)
			{
				// It is valid, that user with this device ident logs in
				var loginRequest = await ReceiveLoginRequest(communicationService);

				user.IsActive = true;
				user.ReactivateCounter++;
				user.StayActive = loginRequest.RequestValue?.StayActive == true;
				_usersService.SetUser(user);

				SendLoginReply(communicationService, true);

				return AuthorizationInfo.Create(user.Name);
			}

			return await Authorize(communicationService, 0, deviceIdent.Ident);
		}

		private static bool CheckLastLoggedInDeviceIdent(User user, DeviceIdentReply deviceIdent)
		{
			if (user.LastLoggedInDeviceIdent == null)
				return true;

			return user.LastLoggedInDeviceIdent.Equals(deviceIdent.Ident);
		}

		private static async Task<LoginRequest> ReceiveLoginRequest(ICommunicationService communicationService)
		{
			return await communicationService.ReceiveAsync<LoginRequest>();
		}

		private async Task<IAuthorizationInfo> Authorize(ICommunicationService communicationService, int attempts,
			string? deviceIdent)
		{
			var loginRequest = await ReceiveLoginRequest(communicationService);

			var requestValueType = loginRequest.RequestValue?.Type;
			var requestValueValue = loginRequest.RequestValue?.Value;

			if (requestValueType?.Equals(LoginRequestType.Username) == true &&
			    !string.IsNullOrEmpty(requestValueValue) &&
			    _usersService.TryGetUserByUsername(requestValueValue, out var user) && user is { IsNotActive: true })
			{
				user.IsActive = true;
				user.ReactivateCounter = 0;
				user.LastLoggedInDeviceIdent = deviceIdent;
				user.StayActive = loginRequest.RequestValue?.StayActive == true;
				_usersService.SetUser(user);
				SendLoginReply(communicationService, true);
				return AuthorizationInfo.Create(user.Name);
			}

			SendLoginReply(communicationService, false);

			if (attempts <= _settings.MaxAuthAttempts)
				return await Authorize(communicationService, attempts + 1, deviceIdent);

			return AuthorizationInfo.Failed;
		}

		private static void SendLoginReply(ICommunicationService communicationService, bool loginSuccess)
		{
			communicationService.SendAsync(new LoginReply
			{
				Success = loginSuccess
			});
		}

		public async Task UnAuthorize(ICommunicationService communicationService, string username)
		{
			try
			{
				var logoutReply = await communicationService.SendAndReceiveAsync<LogoutReply>(new LogoutRequest());

				if (!logoutReply.IsOk)
				{
					this.LogWarning($"Logging out fo user {username} was not successful on client side...");
				}
			}
			catch (CommunicationServiceException communicationServiceException)
			{
				this.LogDebug(
					$"Communication exception code: {communicationServiceException.ErrorCode}; Message: {communicationServiceException.Message}");
			}
			catch (ObjectDisposedException objectDisposedException)
			{
				this.LogDebug($"CommunicationService already disposed. ExMsg: {objectDisposedException.Message}");
			}
			catch (OperationCanceledException)
			{
				this.LogDebug($"Stop receiving logout reply, if request was okay or not.");
			}
			catch (Exception exception)
			{
				this.LogError($"Unknown error inside AuthenticationService: {exception.Message}" +
				              $"\n Stacktrace: {exception.StackTrace}");
			}
			finally
			{
				_usersService.SetUsersActiveState(username, false);
			}
		}
	}
}