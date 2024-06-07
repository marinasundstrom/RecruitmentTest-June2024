using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Itm8OfficeWeather.Services;

public interface ISmhiForecastsClient
{
    Task<Forecast> GetForecastAsync(double lat, double lon, CancellationToken cancellationToken = default);
}

public sealed class SmhiForecastsClient(HttpClient httpClient) : ISmhiForecastsClient
{
    public async Task<Forecast> GetForecastAsync(double lat, double lon, CancellationToken cancellationToken = default)
    {
        var forecastStream = await httpClient.GetStreamAsync($"https://opendata-download-metfcst.smhi.se/api/category/pmp3g/version/2/geotype/point/lon/{Math.Round(lon, 3)}/lat/{Math.Round(lat, 3)}/data.json", cancellationToken);
        var forecastJson = await JsonDocument.ParseAsync(forecastStream, default, cancellationToken);

        var timeSeriesJson = forecastJson.RootElement
            .GetProperty("timeSeries")
            .EnumerateArray()
            .OrderBy(x => x.GetProperty("validTime").GetDateTimeOffset());

        List<ForecastItem> items = new List<ForecastItem>();

        foreach (var timeSeriesItemJson in timeSeriesJson)
        {
            var validTime = timeSeriesItemJson.GetProperty("validTime").GetDateTimeOffset();
            var parametersJson = timeSeriesItemJson.GetProperty("parameters").EnumerateArray();
            var tempParamJson = parametersJson.First(x => x.GetProperty("name").GetString() == "t");
            var precepParamJson = parametersJson.First(x => x.GetProperty("name").GetString() == "spp");
            var precepCatParamJson = parametersJson.First(x => x.GetProperty("name").GetString() == "pcat");
            var wsymb2ParamJson = parametersJson.First(x => x.GetProperty("name").GetString() == "Wsymb2");

            double temp = tempParamJson.GetProperty("values").EnumerateArray().FirstOrDefault().GetDouble();
            int precep = precepParamJson.GetProperty("values").EnumerateArray().FirstOrDefault().GetInt32();
            PrecipitationCategory precepCat = (PrecipitationCategory)precepCatParamJson.GetProperty("values").EnumerateArray().FirstOrDefault().GetInt32();
            WeatherSymbol weatherSymbol = (WeatherSymbol)wsymb2ParamJson.GetProperty("values").EnumerateArray().FirstOrDefault().GetInt32();

            items.Add(new ForecastItem(validTime, temp, new Precipitation(precepCat, precep), weatherSymbol));
        }

        return new Forecast(new GeoCoordinate(lat, lon), items);
    }
}

public sealed record Forecast(GeoCoordinate Coordinate, IEnumerable<ForecastItem> TimeSeries);

public sealed record GeoCoordinate(double Lat, double Lon);

public sealed record ForecastItem(
    DateTimeOffset ValidTime,
    double Temperature,
    Precipitation Precipitation,
    WeatherSymbol WeatherSymbol
);

public sealed record Precipitation(
    PrecipitationCategory Category,
    int Percent
);

public enum PrecipitationCategory
{
    NoPrecipitation = 0,
    Snow,
    SnowAndRain,
    Rain,
    Drizzle,
    FreezingRain,
    FreezingDrizzle
}

public enum WeatherSymbol
{
    [Display(Name = "Clear sky")]
    ClearSky = 1,
    [Display(Name = "Nearly clear sky")]
    NearlyClearSky,
    [Display(Name = "VariableCloudiness")]
    VariableCloudiness,
    [Display(Name = "HalfclearSky")]
    HalfclearSky,
    CloudySky,
    Overcast,
    Fog,
    LightRainShowers,
    ModerateRainShowers,
    HeavyRainShowers,
    Thunderstorm,
    LightSleetShowers,
    ModerateSleetShowers,
    HeavySleetShowers,
    LightSnowShowers,
    ModerateSnowShowers,
    HeavySnowShowers,
    LightRain,
    ModerateRain,
    HeavyRain,
    Thunder,
    LightSleet,
    ModerateSleet,
    HeavySleet,
    LightSnowfall,
    ModerateSnowfall,
    HeavySnowfall
}