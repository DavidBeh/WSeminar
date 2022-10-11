using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WSeminar.V2G.Simulator.Server.Smard;

namespace WSeminar.V2G.Simulation.Forms;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();

        var host = Host.CreateDefaultBuilder().ConfigureServices(collection =>
        {
            collection.AddLogging(builder => builder.AddConsole());
            collection.AddSingleton<SmardClient>();
            collection.AddTransient<MainForm>();
            collection.AddTransient<HttpClient>(provider => new HttpClient());
            collection.AddSingleton<ScenarioService>();
            collection.AddCacheStack(builder =>
            {
                builder.AddMemoryCacheLayer();
            });
        }).Build();

        host.Services.GetRequiredService<ILogger<object>>().LogError("test");

        var form  = host.Services.GetRequiredService<MainForm>();

        Application.Run(form);
    }
}