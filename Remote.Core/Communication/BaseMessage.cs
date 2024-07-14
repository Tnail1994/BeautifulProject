using Newtonsoft.Json;
using Remote.Core.Implementations;

namespace Remote.Core.Communication
{
	public interface IBaseMessage
	{
	}

	public abstract class BaseMessage<T> : IBaseMessage
	{
		public T? MessageObject { get; set; }

		public static BaseMessage<T>? Transform(string jsonString) =>
			JsonConvert.DeserializeObject<BaseMessage<T>>(jsonString, JsonConfig.Settings);

		public override string ToString()
		{
			return $"Message with Objekt type {typeof(T).Name}";
		}
	}
}