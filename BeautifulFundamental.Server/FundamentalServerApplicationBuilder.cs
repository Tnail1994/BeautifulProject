using BeautifulFundamental.Core;
using BeautifulFundamental.Server.Core;
using BeautifulFundamental.Server.Db;
using BeautifulFundamental.Server.Session.Context;
using BeautifulFundamental.Server.Session.Context.Db;
using BeautifulFundamental.Server.Session.Core;
using BeautifulFundamental.Server.Session.Scope;
using BeautifulFundamental.Server.Session.Services;
using BeautifulFundamental.Server.Session.Services.Authorization;
using BeautifulFundamental.Server.UserManagement;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BeautifulFundamental.Server
{
	public static class FundamentalServerApplicationBuilder
	{
		public static IHostBuilder UseBeautifulFundamentalServer(this IHostBuilder hostBuilder)
		{
			hostBuilder
				.UseBeautifulFundamentalCore(true)
				.ConfigureServices((_, services) => { RegisterBeautifulFundamentalServer(services); });

			return hostBuilder;
		}

		public static void RegisterBeautifulFundamentalServer(IServiceCollection services)
		{
			services.AddMemoryCache();
			services.AddHostedService<SessionManager>();

			services.AddSingleton<UsersDbContext>();
			services.AddSingleton<SessionsDbContext>();
			services.AddSingleton<ISessionDataProvider, SessionsDbContext>();

			services.AddSingleton<IDbContext>(sp => sp.GetRequiredService<UsersDbContext>());
			services.AddSingleton<IDbContext>(sp => sp.GetRequiredService<SessionsDbContext>());

			services.AddSingleton<IScopeManager, ScopeManager>();
			services.AddSingleton<IAsyncServer, AsyncServer>();
			services.AddSingleton<IDbManager, DbManager>();
			services.AddSingleton<IDbContextResolver, DbContextResolver>();
			services.AddSingleton<IAuthenticationService, AuthenticationService>();
			services.AddSingleton<IUsersService, UsersService>();
			services.AddSingleton<ISessionsService, SessionsService>();

			services.AddSingleton<ISessionContextManager, SessionContextManager>();

			services.AddScoped<ISession, BeautifulFundamental.Server.Session.Core.Session>();

			services.AddScoped<ISessionContext, SessionContext>();
			services.AddScoped<ISessionDetailsManager, SessionDetailsManager>();

			// Make it lazy, because the details needed to be initialized. This happens, will
			// build and start the session. If the session is ready, then the loop will be needed.
			services.AddTransient<Lazy<ISessionLoop>>(provider =>
				new Lazy<ISessionLoop>(provider.GetRequiredService<ISessionLoop>));
		}

		public static void CreateAndSetupConfig(IServiceCollection services)
		{
			var config = FundamentalApplicationBuilder.CreateAndSetupConfig(services);

			services.AddSingleton<IAsyncServerSettings>(_ =>
				config.GetSection(nameof(AsyncServerSettings)).Get<AsyncServerSettings>() ??
				AsyncServerSettings.Default);
			services.AddSingleton<IDbSettings>(_ =>
				config.GetSection(nameof(DbSettings)).Get<DbSettings>() ??
				DbSettings.Default);
			services.AddSingleton<IDbContextSettings>(_ =>
				config.GetSection(nameof(DbContextSettings)).Get<DbContextSettings>() ??
				DbContextSettings.Default);
			services.AddSingleton<IAuthenticationSettings>(_ =>
				config.GetSection(nameof(AuthenticationSettings)).Get<AuthenticationSettings>() ??
				AuthenticationSettings.Default);
		}
	}
}