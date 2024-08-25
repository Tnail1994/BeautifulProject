using System.Reflection;
using BeautifulFundamental.Core.Communication.Implementations;
using BeautifulFundamental.Core.Communication.Transformation.Implementations;
using BeautifulFundamental.Core.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace BeautifulFundamental.Core.Communication.Transformation
{
	public class TransformerService : ITransformerService
	{
		private readonly Dictionary<string, MethodInfo> _methodCache = new Dictionary<string, MethodInfo>();

		public TransformerService(IServiceProvider serviceProvider)
		{
			RegisterAllBaseMessagesTransformMethod(serviceProvider);
		}

		private void RegisterAllBaseMessagesTransformMethod(IServiceProvider serviceProvider)
		{
			var baseMessageTypes = serviceProvider.GetServices<INetworkMessage>()
				.Select(m => m.GetType())
				.Where(t => t.BaseType?.IsGenericType == true &&
				            t.BaseType.GetGenericTypeDefinition() == typeof(NetworkMessage<>));

			this.LogDebug("Registering BaseMessageTypes: **");

			foreach (var type in baseMessageTypes)
			{
				var typeName = type.Name;
				this.LogDebug($"**Registering {typeName}");
				var methodInfo = type.GetMethod("Transform",
					BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

				if (methodInfo == null)
				{
					this.LogError($"No methodInfo (No Transform-Method) found for {typeName}.");
					continue;
				}

				_methodCache[typeName] = methodInfo;
			}
		}

		public TransformedObject Transform(string json)
		{
			this.LogInfo($"Start transforming object: {json}");

			var discriminator = FindDiscriminator(json);

			if (string.IsNullOrEmpty(discriminator))
			{
				var message = $"No discriminator: {string.IsNullOrEmpty(discriminator)}. Errorcode 1";
				this.LogError($"{message}");
				throw new TransformException(message, 1);
			}

			if (!_methodCache.TryGetValue(discriminator, out var method))
			{
				var message = $"Transform method not found for type: {discriminator}. Errorcode 2";
				this.LogError($"{message}");
				throw new TransformException(message, 2);
			}

			var invokeResult = method.Invoke(null, new object[] { json });

			if (invokeResult == null)
			{
				var message = $"Invoke result is null for type: {discriminator}. Errorcode 3";
				this.LogError($"{message}");
				throw new TransformException(message, 3);
			}

			return TransformedObject.Create(invokeResult, discriminator);
		}

		private static string? FindDiscriminator(string json)
		{
			var jObject = JObject.Parse(json);
			var discriminator = jObject["$type"]?.ToString().Split(',')[0].Split('.').Last();
			return discriminator;
		}

		public void Dispose()
		{
			_methodCache.Clear();
		}
	}
}