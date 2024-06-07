using System.Runtime.CompilerServices;
using System.Text.Json;
using Itm8OfficeWeather.Services;

public static class Endpoints
{
    public static WebApplication MapForecastsEndpoints(this WebApplication app)
    {
        app.MapGet("/api/weather/offices", GetOfficeWeatherData)
           .CacheOutput()
           .WithName("OfficeWeather_GetWeatherData")
           .WithTags("OfficeWeather");

        return app;
    }

    public static async IAsyncEnumerable<OfficeWeather> GetOfficeWeatherData(IOfficeDataReader officeDataReader, ISmhiForecastsClient forecastsClient, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var offices = officeDataReader.GetOfficesAsync(cancellationToken);

        await foreach (var office in offices)
        {
            var foreacast = await forecastsClient.GetForecastAsync(office.Coordinate.Lat, office.Coordinate.Lon, cancellationToken);

            var item = foreacast.TimeSeries.First();

            yield return new OfficeWeather(office.Name, office.Location, new GeoCoordinate(office.Coordinate.Lat, office.Coordinate.Lon), item.Temperature, new Precipitation(item.Precipitation.Category, item.Precipitation.Percent), item.WeatherSymbol, item.ValidTime);
        }
    }
}

public sealed record OfficeWeather(string Name, string Location, GeoCoordinate Coordinates, double TemperatureC, Precipitation Precipitation, WeatherSymbol WeatherSymbol, DateTimeOffset ValidTime);

public sealed record GeoCoordinate(double Lat, double Lon);

public sealed record Precipitation(PrecipitationCategory Category, int Percent);