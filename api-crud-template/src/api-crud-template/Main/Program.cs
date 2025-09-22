using Adapters.Inbound.API.Endpoints;
using Adapters.Inbound.API.Middlewares;
using Configurations;
using Serilog;

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

try
{
    Log.Information("Iniciando aplicação CrudTemplate");

    // Registrar todas as dependências
    builder.Services.ConfigureDependencyInjection(configuration);

    var app = builder.Build();
    app.UseAPIExtensions(); 

}
catch (Exception ex)
{
    Log.Fatal(ex, "Erro fatal na inicialização da aplicação");
    throw;
}
finally
{
    await Log.CloseAndFlushAsync();
}