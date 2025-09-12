using Adapters.Inbound.WebApi.Extensions;
using Configurations;
using System.Reflection;



var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxConcurrentConnections = 1000;
    options.Limits.MaxRequestBodySize = 30 * 1024 * 1024; // 30MB
    options.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(30);
    options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(2);
    options.AddServerHeader = false;
});

var configuration = new ConfigurationBuilder()

    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();


builder.Services.ConfigureSwagger("pix-pagador", "v1");
builder.Services.ConfigureMicroservice(configuration);
Console.WriteLine($"Serviço: {Assembly.GetExecutingAssembly().GetName()} Versão: {Assembly.GetExecutingAssembly().GetName().Version}");


var app = builder.Build();
app.UseMicroserviceExtensions();
