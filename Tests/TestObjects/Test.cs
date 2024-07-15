namespace Tests.TestObjects
{
	public class Test
	{
		private Test(string mockObj)
		{
			MockObj = mockObj;
		}

		public static Test Create(string mockObj) => new(mockObj);

		public string MockObj { get; }
	}
}