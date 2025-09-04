using Adapters.Inbound.HttpAdapters.Configuration;
using Services.Configuration;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);
var configuration = new ConfigurationBuilder()
    
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

builder.Services.ConfigureServices(configuration);
Console.WriteLine($"Serviço: {Assembly.GetExecutingAssembly().GetName()} Versão: {Assembly.GetExecutingAssembly().GetName().Version}");

var app = builder.Build();
app.UseAPIExtensions();
app.Run();