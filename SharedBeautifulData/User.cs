namespace SharedBeautifulData
{
	public class User : Entity
	{
		public User()
		{
		}

		public User(string username)
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