namespace BeautifulFundamental.Core.Helpers
{
	public class GuidIdCreator
	{
		public static string CreateString()
		{
			return Guid.NewGuid().ToString();
		}
	}
}