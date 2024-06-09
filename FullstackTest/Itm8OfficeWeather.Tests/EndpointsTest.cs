using Itm8OfficeWeather.Client;
using Itm8OfficeWeather.Services;

using NSubstitute.ExceptionExtensions;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Itm8OfficeWeather.Tests;

public class EndpointsTest
{
    [Fact]
    public async Task ShouldReturnOkWhenSuccessful()
    {
        // Arrange

        IEnumerable<Office> offices = [new Office("", "", new Services.GeoCoordinate(0, 0))];

        var officeDataReader = Substitute.For<IOfficeDataReader>();
        officeDataReader.GetOfficesAsync(default).ReturnsForAnyArgs(offices);

        Forecast forecast = new Forecast(new Services.GeoCoordinate(0, 0),
            [new ForecastItem(DateTime.UtcNow, 14.2, new Services.Precipitation(Services.PrecipitationCategory.NoPrecipitation, -9), Services.WeatherSymbol.ClearSky)]);

        var smhiForecastsClient = Substitute.For<ISmhiForecastsClient>();
        smhiForecastsClient.GetForecastAsync(0, 0, default).ReturnsForAnyArgs(forecast);

        // Act

        var result = await Endpoints.GetOfficeWeatherData(officeDataReader, smhiForecastsClient, default);

        // Assert

        result.Result.ShouldBeOfType<Ok<IEnumerable<OfficeWeather>>>();
    }

    [Fact]
    public async Task ShouldReturnProblemHttpResultWhenOfficesFailed()
    {
        // Arrange

        IEnumerable<Office> offices = [new Office("", "", new Services.GeoCoordinate(0, 0))];

        var officeDataReader = Substitute.For<IOfficeDataReader>();
        officeDataReader.GetOfficesAsync(default).Throws<IOException>();

        var smhiForecastsClient = Substitute.For<ISmhiForecastsClient>();

        // Act

        var result = await Endpoints.GetOfficeWeatherData(officeDataReader, smhiForecastsClient, default);

        // Assert

        result.Result.ShouldBeOfType<ProblemHttpResult>();
    }

    [Fact]
    public async Task ShouldReturnProblemHttpResultWhenForecastsFailed()
    {
        // Arrange

        IEnumerable<Office> offices = [new Office("", "", new Services.GeoCoordinate(0, 0))];

        var officeDataReader = Substitute.For<IOfficeDataReader>();
        officeDataReader.GetOfficesAsync(default).ReturnsForAnyArgs(offices);

        var smhiForecastsClient = Substitute.For<ISmhiForecastsClient>();
        smhiForecastsClient.GetForecastAsync(0, 0, default).Throws<Exception>();

        // Act

        var result = await Endpoints.GetOfficeWeatherData(officeDataReader, smhiForecastsClient, default);

        // Assert

        result.Result.ShouldBeOfType<ProblemHttpResult>();
    }
}
