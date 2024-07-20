using Session.Common.Contracts;

namespace Session.Services.Authorization
{
	internal class AuthorizationInfo : IAuthorizationInfo
	{
		private AuthorizationInfo(string username)
		{
			Username = username;
			IsAuthorized = true;
		}

		private AuthorizationInfo(bool isAuthorized = false)
		{
			Username = string.Empty;
			IsAuthorized = isAuthorized;
		}

		public bool IsAuthorized { get; }
		public string Username { get; }
		public static IAuthorizationInfo Failed => new AuthorizationInfo();
		public static IAuthorizationInfo Create(string username) => new AuthorizationInfo(username);
	}
}