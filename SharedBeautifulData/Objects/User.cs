using SharedBeautifulData.Contracts;

namespace SharedBeautifulData.Objects
{
	public class User : IEntity
	{
		public User()
		{
		}

		private User(string name)
		{
			Name = name;
		}

		public static User Create(string username)
		{
			return new User(username);
		}

		public string Name { get; set; } = null!;
	}
}