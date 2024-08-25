using BeautifulFundamental.Core.Communication.Transformation.Implementations;
using Newtonsoft.Json;

namespace BeautifulFundamental.Core.Communication.Implementations
{
	public interface INetworkMessage;

	public abstract class NetworkMessage<T> : INetworkMessage
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