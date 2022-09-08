using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Polly;
using WSeminar.V2G.Simulator.Server.Data;
using WSeminar.V2G.Simulator.Server.Smard;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();

var cachePolicy = Policy.CacheAsync(null,null);

builder.Services.AddHttpClient<SmardClient>().AddPolicyHandler().AddTransientHttpErrorPolicy(policyBuilder =>
    policyBuilder.WaitAndRetryAsync(new[]
    {
        TimeSpan.FromSeconds(1),
        TimeSpan.FromSeconds(5),
        TimeSpan.FromSeconds(10)
    }));

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