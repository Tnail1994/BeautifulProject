using BeautifulFundamental.Core.Communication;
using BeautifulFundamental.Core.Communication.Client;
using BeautifulFundamental.Core.Communication.Implementations;
using BeautifulFundamental.Core.Communication.Transformation;
using BeautifulFundamental.Core.Identification;
using BeautifulFundamental.Core.Messages.Authorize;
using BeautifulFundamental.Core.Messages.CheckAlive;
using BeautifulFundamental.Core.Messages.RandomTestData;
using BeautifulFundamental.Core.Services.CheckAlive;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BeautifulFundamental.Core
{
	public static class FundamentalApplicationBuilder
	{
		public static IHostBuilder UseBeautifulFundamentalCore(this IHostBuilder hostBuilder, bool scoped = false)
		{
			hostBuilder.ConfigureServices((hostContext, services) =>
			{
				RegisterBeautifulFundamentalCore(services, scoped);
			});

			return hostBuilder;
		}

		/// <summary>
		/// Using scoped flag means, adding following services as scoped
		/// IIdentificationKey, IAsyncClientFactory, ICommunicationService, IConnectionService,ICheckAliveService
		/// </summary>
		/// <param name="services"></param>
		/// <param name="scoped"></param>
		public static void RegisterBeautifulFundamentalCore(IServiceCollection services, bool scoped = false)
		{
			services.AddTransient<INetworkMessage, CheckAliveRequest>();
			services.AddTransient<INetworkMessage, CheckAliveReply>();
			services.AddTransient<INetworkMessage, LoginReply>();
			services.AddTransient<INetworkMessage, LoginRequest>();
			services.AddTransient<INetworkMessage, RandomDataRequest>();
			services.AddTransient<INetworkMessage, RandomDataReply>();
			services.AddTransient<INetworkMessage, DeviceIdentRequest>();
			services.AddTransient<INetworkMessage, DeviceIdentReply>();
			services.AddTransient<INetworkMessage, LogoutRequest>();
			services.AddTransient<INetworkMessage, LogoutReply>();

			if (scoped)
			{
				services.AddScoped<IIdentificationKey, IdentificationKey>();
				services.AddScoped<IAsyncClientFactory, AsyncClientFactory>();
				services.AddScoped(provider =>
					provider.GetRequiredService<IAsyncClientFactory>().Create());

				services.AddScoped<ICommunicationService, CommunicationService>();
				services.AddScoped<IConnectionService, ConnectionService>();
				services.AddScoped<ICheckAliveService, CheckAliveService>();
			}
			else
			{
				services.AddSingleton<IIdentificationKey, IdentificationKey>();
				services.AddSingleton<IAsyncClientFactory, AsyncClientFactory>();
				services.AddSingleton(provider =>
					provider.GetRequiredService<IAsyncClientFactory>().Create());

				services.AddSingleton<ICommunicationService, CommunicationService>();
				services.AddSingleton<IConnectionService, ConnectionService>();
				services.AddSingleton<ICheckAliveService, CheckAliveService>();
			}

			services.AddSingleton<ITransformerService, TransformerService>();
		}
	}
}