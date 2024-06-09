
using static Microsoft.AspNetCore.Http.TypedResults;

using System.Runtime.CompilerServices;

using Itm8OfficeWeather.Services;

using Microsoft.AspNetCore.Http.HttpResults;

public static class Endpoints
{
    public static WebApplication MapForecastsEndpoints(this WebApplication app)
    {
        app.MapGet("/api/weather/offices", GetOfficeWeatherData)
           .CacheOutput()
           .WithName("OfficeWeather_GetWeatherData")
           .WithTags("OfficeWeather")
           .ProducesProblem(StatusCodes.Status500InternalServerError);

        return app;
    }

    public static async Task<Results<Ok<IEnumerable<OfficeWeather>>, ProblemHttpResult>> GetOfficeWeatherData(IOfficeDataReader officeDataReader, ISmhiForecastsClient forecastsClient, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        IEnumerable<Office> offices;

        try
        {
            offices = await officeDataReader.GetOfficesAsync(cancellationToken);
        }
        catch (Exception exc)
        {
            return Problem(title: "Failed to load offices", detail: exc.Message, statusCode: StatusCodes.Status500InternalServerError);
        }

        List<OfficeWeather> forecasts = new List<OfficeWeather>();

        foreach (var office in offices)
        {
            try
            {
                var foreacast = await forecastsClient.GetForecastAsync(office.Coordinate.Lat, office.Coordinate.Lon, cancellationToken);

                var item = foreacast.TimeSeries.First();

                forecasts.Add(new OfficeWeather(office.Name, office.Location, new GeoCoordinate(office.Coordinate.Lat, office.Coordinate.Lon), item.Temperature, new Precipitation(item.Precipitation.Category, item.Precipitation.Percent), item.WeatherSymbol, item.ValidTime));
            }
            catch (Exception exc)
            {
                return Problem(title: "Failed to retrieve forecast", detail: exc.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        return Ok(forecasts.AsEnumerable());
    }
}

public sealed record OfficeWeather(string Name, string Location, GeoCoordinate Coordinates, double TemperatureC, Precipitation Precipitation, WeatherSymbol WeatherSymbol, DateTimeOffset ValidTime);

public sealed record GeoCoordinate(double Lat, double Lon);

public sealed record Precipitation(PrecipitationCategory Category, int Percent);