using Newtonsoft.Json;
using Remote.Communication.Common.Transformation.Implementations;

namespace Remote.Communication.Common.Implementations
{
	public interface INetworkMessage
	{
	}

	public interface IRequestMessage : INetworkMessage
	{
	}

	public interface IReplyMessage : INetworkMessage
	{
	}

	public abstract class NetworkMessage<T>
	{
		public T? MessageObject { get; set; }


		public static NetworkMessage<T>? Transform(string jsonString) =>
			JsonConvert.DeserializeObject<NetworkMessage<T>>(jsonString, JsonConfig.Settings);

		public override string ToString()
		{
			return $"Message with Objekt type {typeof(T).Name}";
		}
	}
}