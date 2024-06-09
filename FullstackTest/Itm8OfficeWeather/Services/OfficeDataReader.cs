using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Itm8OfficeWeather.Services;

public interface IOfficeDataReader
{
    Task<IEnumerable<Office>> GetOfficesAsync(CancellationToken cancellationToken = default);
}

public sealed class OfficeDataReader : IOfficeDataReader
{
    public async Task<IEnumerable<Office>> GetOfficesAsync(CancellationToken cancellationToken = default)
    {
        List<Office> offices = new List<Office>();

        using var fileStream = File.OpenRead("./itm8-offices.json");
        var officeLocationJson = await JsonDocument.ParseAsync(fileStream, default, cancellationToken);

        foreach (var officeJson in officeLocationJson.RootElement.EnumerateArray())
        {
            string name = officeJson.GetProperty("name").GetString()!;
            string location = officeJson.GetProperty("location").GetString()!;

            var coordinates = officeJson.GetProperty("coordinates");
            double lat = coordinates.GetProperty("lat").GetDouble()!;
            double lon = coordinates.GetProperty("lon").GetDouble()!;

            offices.Add(new Office(name, location, new GeoCoordinate(lat, lon)));
        }

        return offices;
    }
}

public sealed record Office(string Name, string Location, GeoCoordinate Coordinate);