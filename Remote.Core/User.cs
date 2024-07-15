namespace Remote.Core
{
	public class User
	{
		public User(string username, string password)
		{
			Username = username;
			Password = password;
		}

		public static User Create(string username, string password)
		{
			return new User(username, password);
		}

		public string Username { get; set; }
		public string Password { get; set; }
	}
}