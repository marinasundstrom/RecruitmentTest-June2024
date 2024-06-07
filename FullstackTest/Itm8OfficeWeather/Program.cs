using Microsoft.FluentUI.AspNetCore.Components;
using Itm8OfficeWeather.Components;
using Itm8OfficeWeather.Client;
using Itm8OfficeWeather.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddFluentUIComponents();

builder.Services.AddHttpClient();

builder.Services.AddScoped<IOfficeDataReader, OfficeDataReader>();
builder.Services.AddScoped<ISmhiForecastsClient, SmhiForecastsClient>();

builder.Services.AddOutputCache(options =>
{
    options.AddBasePolicy(builder =>
        builder.Expire(TimeSpan.FromSeconds(10))); ;
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddOpenApiDocument(options =>
{
    options.Title = "Itm8 Office Weather API";
});

var app = builder.Build();

app.UseOpenApi(options =>
{
    options.DocumentName = "Itm8 Office Weather API";
    options.Path = "/openapi/{documentName}/openapi.yaml";
});

app.UseSwaggerUi(options =>
{
    options.DocumentTitle = "Itm8 Office Weather API";
    options.Path = "/openapi";
    options.DocumentPath = "/openapi/{documentName}/openapi.yaml";
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.UseOutputCache();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(WeatherPage).Assembly);

app.MapForecastsEndpoints();

app.Run();
