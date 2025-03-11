using RailVision.Application.Abstractions;
using RailVision.Application.DTOs.Overpass;
using RailVision.Application.DTOs.Railways;
using System.Text.Json;

namespace RailVision.Infrastructure.Services
{
    public class RailwayService(IHttpClientFactory httpClientFactory) : IRailwayService
    {
        private readonly HttpClient _httpClient = httpClientFactory.CreateClient("OverpassClient");

        public async Task<OverpassResponseDTO> GetRailwayDataAsync(CancellationToken cancellationToken = default)
        {
            // Overpass API URL
            var overpassUrl = "http://overpass-api.de/api/interpreter";

            // Overpass QL query
            var query = @"
            [out:json];
            area['name:en'='Azerbaijan']->.a;
            way(area.a)['railway'='rail'];
            out body;
            >;
            out skel qt;
        ";

            var content = new FormUrlEncodedContent(new[]
            {
            new KeyValuePair<string, string>("data", query)
        });

            var response = await _httpClient.PostAsync(overpassUrl, content, cancellationToken);

            if (!response.IsSuccessStatusCode)
                throw new Exception("Error fetching data from Overpass API");

            var jsonResponse = await response.Content.ReadAsStringAsync(cancellationToken);

            // Deserialize the JSON response into OverpassResponseDTO
            var overpassResponse = JsonSerializer.Deserialize<OverpassResponseDTO>(jsonResponse);

            if (overpassResponse == null)
                throw new Exception("Error deserializing the Overpass API response");

            return overpassResponse;
        }

        public async Task<IEnumerable<RailwayLineDTO>> GetRailwayLinesAsync(CancellationToken cancellationToken = default)
        {
            var railwaysData = await GetRailwayDataAsync(cancellationToken);

            var railwayLines = railwaysData.Elements
                        .Where(e => e.Type == "way" && e.Tags.TryGetValue("Railway", out var value) && value == "rail")
                        .Select(e =>
                        {
                            var coordinates = e.Nodes
                                .Select(nodeId =>
                                {
                                    var node = railwaysData.Elements.FirstOrDefault(el => el.Type == "node" && el.Id == nodeId);
                                    return new CoordinateDTO
                                    {
                                        Latitude = node?.Lat ?? 0,
                                        Longitude = node?.Lon ?? 0
                                    };
                                });

                            return new RailwayLineDTO
                            {
                                Id = e.Id,
                                Coordinates = coordinates
                            };
                        });

            return railwayLines;
        }
    }
}
