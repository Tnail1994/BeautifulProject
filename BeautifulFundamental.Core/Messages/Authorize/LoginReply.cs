using BeautifulFundamental.Core.Communication.Implementations;
using Newtonsoft.Json;

namespace BeautifulFundamental.Core.Messages.Authorize
{
	public class LoginResult
	{
		public bool Success { get; set; }
		public bool CanRetry { get; set; }
	}

	public class LoginReply : NetworkMessage<LoginResult>
	{
		[JsonIgnore]
		public LoginResult? LoginResult
		{
			get => MessageObject;
			set => MessageObject = value;
		}

		[JsonIgnore]
		public bool Success
		{
			get => LoginResult is { Success: true };
			set
			{
				if (LoginResult == null)
					return;

				LoginResult.Success = value;
			}
		}

		[JsonIgnore]
		public bool CanRetry
		{
			get => LoginResult is { CanRetry: true };
			set
			{
				if (LoginResult == null)
					return;

				LoginResult.CanRetry = value;
			}
		}
	}
}