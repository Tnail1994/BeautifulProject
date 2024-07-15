namespace Tests.TestObjects
{
	public class TestObject(string mockObj)
	{
		public static TestObject Create(string mockObj) => new(mockObj);

		public string MockObj { get; } = mockObj;
	}
}