using BeautifulFundamental.Core.Communication.Transformation.Implementations;
using BeautifulFundamental.Core.Helpers;

namespace BeautifulFundamental.Core.Communication.Transformation
{
	internal class TransformedObjectWaiter
	{
		public string Id { get; }

		public string Discriminator { get; }
		public TaskCompletionSource<TransformedObject> TaskCompletionSource { get; }

		private TransformedObjectWaiter(string discriminator)
		{
			Id = GuidIdCreator.CreateString();

			TaskCompletionSource = new TaskCompletionSource<TransformedObject>();

			Discriminator = discriminator;
		}

		public static TransformedObjectWaiter Create(string discriminator) =>
			new(discriminator);
	}
}