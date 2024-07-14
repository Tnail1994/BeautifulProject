using CoreHelpers;

namespace Remote.Core.Transformation;

public class TransformedObject
{
	public object Object { get; }
	public string Discriminator { get; }
	public string Id { get; }

	private TransformedObject(object obj, string discriminator)
	{
		Id = GuidIdCreator.CreateString();

		Object = obj;
		Discriminator = discriminator;
	}

	public static TransformedObject Create(object obj, string discriminator) => new(obj, discriminator);
}