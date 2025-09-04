using Microsoft.Extensions.Options;
using W3Socket.Core.Sockets.Client;
using Domain.Core.Models.Settings;
using Domain.Core.Ports.Outbound;
using W3Socket.Core.Interfaces;

namespace Adapters.Outbound.TCPAdapter.Configuration
{
    public static class TCPConfiguration
    {
        public static IServiceCollection AddTCPClientAdapter(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<SPASettings>(options =>
            {
                var _settings = configuration.GetSection("AppSettings:SPA");

                options.Dependencia = new Domain.Core.Models.SPA.SPADependencia
                {
                    Agencia = _settings.GetValue<int>("Dependencia:Agencia"),
                    Posto = _settings.GetValue<int>("Dependencia:Posto")
                };

                options.RouterIP = Environment.GetEnvironmentVariable("SPAROUTER_IP")?? _settings.GetValue<string>("RouterIP")!;
             
                var routerPortEnv = Environment.GetEnvironmentVariable("SPAROUTER_PORT");
                options.RouterPort = int.TryParse(routerPortEnv, out var parsedPort)
                    ? parsedPort
                    : _settings.GetValue<int>("RouterPort");

                options.Operador = _settings.GetValue<string>("Operador")!;
                options.ConnectTimeOut = _settings.GetValue<int>("ConnectTimeOut");
                options.SendTimeout = _settings.GetValue<int>("SendTimeout");
                options.ReceiveBufferSize = _settings.GetValue<int>("ReceiveBufferSize");
                options.WithQueue = _settings.GetValue<bool>("WithQueue");
                options.QueueDelay = _settings.GetValue<int>("QueueDelay");

                if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Local")
                {
                    options.RouterIP = "127.0.0.1";
                    options.RouterPort = 30001;
                }

                Console.WriteLine($"SPAROUTER_IP: {options.RouterIP}:{options.RouterPort}");
            });
    
            services.AddSingleton<IClientSocketTCP>(options =>
            {
                var settings = options.GetRequiredService<IOptions<SPASettings>>().Value;
                var socket = new ClientSocketTcp(options, settings.RouterIP, settings.RouterPort, 30);
                socket.Inicialize();

                return socket;
            });

            services.AddTransient<ISPATcpClientServicePort, SPATcpClientService>();

            return services;
        }
    }
}
