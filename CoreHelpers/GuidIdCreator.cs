namespace CoreHelpers
{
	public class GuidIdCreator
	{
		public static string CreateString()
		{
			return Guid.NewGuid().ToString();
		}
	}
}