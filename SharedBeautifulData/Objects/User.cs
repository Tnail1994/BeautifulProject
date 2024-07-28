using SharedBeautifulData.Contracts;

namespace SharedBeautifulData.Objects
{
	public class User : IEntity
	{
		private User(string name, bool isActive)
		{
			Name = name;
			IsActive = isActive;
		}

		public static User Create(string username, bool isActive)
		{
			return new User(username, isActive);
		}

		public string Name { get; set; }
		public bool IsActive { get; set; }
	}
}