using RailVision.Application.Abstractions;
using RailVision.Application.DTOs.Overpass;
using System.Text.Json;

namespace RailVision.Infrastructure.Services
{
    public class OverpassApiService(IHttpClientFactory httpClientFactory) : IOverpassApiService
    {
        private readonly string _overpassUrl = "http://overpass-api.de/api/interpreter";
        private readonly HttpClient _httpClient = httpClientFactory.CreateClient("OverpassClient");
        private readonly JsonSerializerOptions _jsonSerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };
        public async Task<OverpassResponseDTO> GetRailwaysDataAsync(CancellationToken cancellationToken = default) => await SendAsync(GetRailwaysDataRequestQuery(), cancellationToken);

        public async Task<OverpassResponseDTO> GetStationsDataAsync(CancellationToken cancellationToken = default) => await SendAsync(GetStationsDataRequestQuery(), cancellationToken);

        private async Task<OverpassResponseDTO> SendAsync(string query, CancellationToken cancellationToken = default)
        {
            var content = new FormUrlEncodedContent([new("data", query)]);

            var response = await _httpClient.PostAsync(_overpassUrl, content, cancellationToken);

            if (!response.IsSuccessStatusCode)
                throw new Exception("Error fetching station data from Overpass API");

            var jsonResponse = await response.Content.ReadAsStringAsync(cancellationToken);

            var overpassResponse = JsonSerializer.Deserialize<OverpassResponseDTO>(jsonResponse, _jsonSerializerOptions) ?? throw new Exception("Error deserializing station data from Overpass API");

            return overpassResponse;
        }

        private static string GetRailwaysDataRequestQuery() => @"
            [out:json];
            area['name:en'='Azerbaijan']->.a;
            way(area.a)['railway'='rail'];
            out body;
            >;
            out skel qt;
        ";

        private static string GetStationsDataRequestQuery() => @"
            [out:json];
            area['name:en'='Azerbaijan']->.a;
            (
              node(area.a)['railway'='station'];
              node(area.a)['railway'='halt'];
              way(area.a)['railway'='station'];
              way(area.a)['railway'='halt'];
            );
            out body;
            >;
            out skel qt;
        ";

    }
}
