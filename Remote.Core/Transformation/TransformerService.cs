﻿using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Remote.Core.Communication;
using System.Reflection;
using Serilog;

namespace Remote.Core.Transformation
{
	public interface ITransformerService
	{
		TransformedObject Transform(string jsonString);
	}

	public class TransformerService : ITransformerService
	{
		private readonly Dictionary<string, Type> _typeMap = new Dictionary<string, Type>();
		private readonly Dictionary<string, MethodInfo> _methodCache = new Dictionary<string, MethodInfo>();

		public TransformerService(IServiceProvider serviceProvider)
		{
			RegisterAllBaseMessages(serviceProvider);
		}

		private void RegisterAllBaseMessages(IServiceProvider serviceProvider)
		{
			var baseMessageTypes = serviceProvider.GetServices<IBaseMessage>()
				.Select(m => m.GetType())
				.Where(t => t.BaseType?.IsGenericType == true &&
				            t.BaseType.GetGenericTypeDefinition() == typeof(BaseMessage<>));

			Log.Debug("Registering BaseMessageTypes: **");

			foreach (var type in baseMessageTypes)
			{
				var typeName = type.Name;
				Log.Debug($"**Registering {typeName}");
				_typeMap[typeName] = type;
				var methodInfo = type.GetMethod("Transform",
					BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

				if (methodInfo == null)
				{
					Log.Error($"No methodInfo (No Transform-Method) found for {typeName}.");
					continue;
				}

				_methodCache[typeName] = methodInfo;
			}
		}

		public TransformedObject Transform(string json)
		{
			Log.Information($"Start transforming object: {json}");

			var discriminator = FindDiscriminator(json);

			if (string.IsNullOrEmpty(discriminator) || !_typeMap.TryGetValue(discriminator, out var type))
			{
				var message = $"No type registered for discriminator: {discriminator}" +
				              $"no discriminator: {string.IsNullOrEmpty(discriminator)}. Errorcode 1";
				Log.Error(message);
				throw new TransformException(message, 1);
			}

			if (!_methodCache.TryGetValue(discriminator, out var method))
			{
				var message = $"Transform method not found for type: {discriminator}. Errorcode 2";
				Log.Error(message);
				throw new TransformException(message, 2);
			}

			var invokeResult = method.Invoke(null, new object[] { json });

			if (invokeResult == null)
			{
				var message = $"Invoke result is null for type: {discriminator}. Errorcode 3";
				Log.Error(message);
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
	}
}