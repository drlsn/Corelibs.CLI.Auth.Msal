using Common.Basic.Collections;
using Corelibs.CLI.Auth.Msal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Corelibs.CLI.Auth.Msal
{
    public static class CLIAuthMsalExtensions
    {
        public static IHttpClientBuilder AddHttpClient(
            this IServiceCollection services, string name, IConfiguration configuration)
        {
            return services.AddHttpClient(name, client =>
            {
                var baseAddress = configuration.GetSection("ApiUrl").Value;
                client.BaseAddress = new Uri(baseAddress);
            });
        }

        public static IServiceCollection AddMsalAuthentication(
           this IServiceCollection services,
           ConfigurationManager configuration)
        {
            var assembly = Assembly.GetCallingAssembly();
            var assemblyName = assembly.GetName().Name;

            var configBuilder = CreateConfigBuilder(assembly, out var streams);
            configuration.AddConfiguration(configBuilder.Build());
            streams.ForEach(s => s.Dispose());
            
            services.AddSingleton<ISecureStorage>(new SecureStorage(assemblyName));
            services.AddSingleton<IPCAWrapper, PCAWrapper>();
            services.AddSingleton<ISignInManager, PCASignInManager>();
            services.AddTransient<AuthorizationMessageHandler>();

            services.AddTransient<IAuthUser, MsalNativeAuthUser>();
            services.AddTransient<ISignInRedirector, MsalNativeSignInRedirector>();

            return services;
        }

        private static ConfigurationBuilder CreateConfigBuilder(Assembly assembly, out Stream[] streams)
        {
            var configBuilder = new ConfigurationBuilder();

            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var resourceNames = assembly.GetManifestResourceNames();

            configBuilder.AddStream(assembly, "appsettings.json", resourceNames, out var stream);
            configBuilder.AddStream(assembly, $"appsettings.{environment}.json", resourceNames, out var streamDev);

            streams = new[] { stream, streamDev };

            return configBuilder;
        }

        private static void AddStream(
            this ConfigurationBuilder configBuilder, Assembly assembly, string name, string[] resourceNames, out Stream stream)
        {
            stream = null;

            var resourceName = resourceNames.FirstOrDefault(n => n.EndsWith(name, StringComparison.OrdinalIgnoreCase));
            if (resourceName is null)
                return;

            stream = assembly.GetManifestResourceStream(resourceName);
            if (stream is null)
                return;

            configBuilder.AddJsonStream(stream);
        }
    }
}
