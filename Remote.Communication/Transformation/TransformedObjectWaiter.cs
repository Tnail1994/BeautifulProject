using CoreHelpers;
using Remote.Communication.Common.Transformation.Implementations;

namespace Remote.Communication.Transformation
{
	internal class TransformedObjectWaiter
	{
		public string Id { get; }

		public string Discriminator { get; }
		public TaskCompletionSource<TransformedObject> TaskCompletionSource { get; }
		public bool IsPermanent { get; }

		private TransformedObjectWaiter(string discriminator, bool isPermanent)
		{
			Id = GuidIdCreator.CreateString();

			TaskCompletionSource = new TaskCompletionSource<TransformedObject>();

			Discriminator = discriminator;
			IsPermanent = isPermanent;
		}

		public static TransformedObjectWaiter Create(string discriminator, bool isPermanent = false) =>
			new(discriminator, isPermanent);
	}
}