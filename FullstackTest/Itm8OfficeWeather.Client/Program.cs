using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.FluentUI.AspNetCore.Components;

using Itm8OfficeWeather.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddFluentUIComponents();

builder.Services.AddHttpClient(nameof(IOfficeWeatherClient), http =>
{
        http.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
})
.AddTypedClient<IOfficeWeatherClient>((http, sp) => new OfficeWeatherClient(http));

await builder.Build().RunAsync();
