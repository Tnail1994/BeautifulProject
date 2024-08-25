using BeautifulFundamental.Core;

namespace BeautifulFundamental.Server.UserManagement
{
	public class User : IEntity
	{
		private User(string username, bool isActive, bool stayActive, string? lastLoggedInDeviceIdent,
			int reactivateCounter)
		{
			Name = username;
			IsActive = isActive;
			LastLoggedInDeviceIdent = lastLoggedInDeviceIdent;
			StayActive = stayActive;
			ReactivateCounter = reactivateCounter;
		}

		public string Name { get; set; }
		public bool IsActive { get; set; }
		public bool StayActive { get; set; }
		public string? LastLoggedInDeviceIdent { get; set; }
		public int ReactivateCounter { get; set; }

		public bool IsNotActive => !IsActive;

		public static User Create(string username, bool isActive, bool stayActive,
			string? lastLoggedInDeviceIdent, int reactivateCounter)
		{
			return new User(username, isActive, stayActive, lastLoggedInDeviceIdent, reactivateCounter);
		}
	}
}