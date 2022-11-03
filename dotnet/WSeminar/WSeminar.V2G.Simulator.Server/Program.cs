global using Opt =  Deedle.OptionalValue;
global using OptD =  Deedle.OptionalValue;
using System.Drawing;
using CacheTower.Providers.FileSystem;
using CacheTower.Serializers.SystemTextJson;
using WSeminar.V2G.Simulator.Server.Data;
using WSeminar.V2G.Simulator.Server.Smard;

var builder = WebApplication.CreateBuilder(args);


builder.WebHost.ConfigureKestrel(options =>
{ 
    options.ListenLocalhost(5000);
    //options.ListenLocalhost(5000);
});
// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();
builder.Services.AddTransient<ScenarioService>();



builder.Services.AddHttpClient<SmardClient>();

builder.Services.AddCacheStack(stackBuilder =>
{
    
    stackBuilder.AddMemoryCacheLayer();
    var disableFileCache = true;
    if (!disableFileCache)
    {
        var path = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.InternetCache)), "WSeminar.V2G.Simulator.Server", "Cache");
        try
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        catch (Exception e)
        {
            // ignored
        }
    
        Directory.CreateDirectory(path);
        stackBuilder.AddFileCacheLayer(new FileCacheLayerOptions(path, SystemTextJsonCacheSerializer.Instance));
    }
    
    

});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseHttpLogging();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();