namespace SharedBeautifulData.Objects
{
	public class User : Entity
	{
		public User()
		{
		}

		private User(string username)
		{
			Username = username;
		}

		public static User Create(string username)
		{
			return new User(username);
		}

		public string Username { get; set; } = null!;
	}
}