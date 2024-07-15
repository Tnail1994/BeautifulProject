namespace Tests.TestObjects
{
	public class Test(string mockObj)
	{
		public static Test Create(string mockObj) => new(mockObj);

		public string MockObj { get; } = mockObj;
	}
}