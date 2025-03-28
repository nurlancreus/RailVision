﻿using Microsoft.Extensions.Logging;
using RailVision.Application.Abstractions.OverpassAPI;
using RailVision.Application.DTOs.Overpass;
using System.Text.Json;

namespace RailVision.Infrastructure.Services
{
    public class OverpassApiService(IHttpClientFactory httpClientFactory, ILogger<OverpassApiService> logger) : IOverpassApiService
    {
        private readonly string _overpassUrl = "http://overpass-api.de/api/interpreter";
        private readonly ILogger<OverpassApiService> _logger = logger;
        private readonly HttpClient _httpClient = httpClientFactory.CreateClient("OverpassClient");
        private readonly JsonSerializerOptions _jsonSerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public async Task<OverpassResponseDTO> GetRailwaysDataAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching railways data from Overpass API");
            return await SendAsync(GetRailwaysDataQuery(), cancellationToken);
        }

        public async Task<OverpassResponseDTO> GetStationsDataAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching stations data from Overpass API");
            return await SendAsync(GetStationsDataQuery(), cancellationToken);
        }

        public async Task<OverpassResponseDTO> GetTerrainObstaclesDataAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching terrain obstacles data from Overpass API");
            return await SendAsync(GetTerrainObstaclesQuery(), cancellationToken);
        }
        public async Task<OverpassResponseDTO> GetNaturalTerrainObstaclesDataAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching natural terrain obstacles data from Overpass API");
            return await SendAsync(GetNaturalTerrainObstaclesQuery(), cancellationToken);
        }

        public async Task<OverpassResponseDTO> GetManMadeTerrainObstaclesDataAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching man-made terrain obstacles data from Overpass API");
            return await SendAsync(GetManMadeTerrainObstaclesQuery(), cancellationToken);
        }

        public async Task<OverpassResponseDTO> GetPopulationCentersAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching population centers from Overpass API");
            return await SendAsync(GetPopulationCentersQuery(), cancellationToken);
        }

        private async Task<OverpassResponseDTO> SendAsync(string query, CancellationToken cancellationToken = default)
        {
            _httpClient.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));

            var content = new FormUrlEncodedContent([new("data", query)]);

            _logger.LogInformation("Sending request to Overpass API...");
            var response = await _httpClient.PostAsync(_overpassUrl, content, cancellationToken);

            if (!response.IsSuccessStatusCode)
                throw new Exception("Error fetching station data from Overpass API");

            _logger.LogInformation("Received response from Overpass API...");
            var jsonResponse = await response.Content.ReadAsStringAsync(cancellationToken);

            _logger.LogInformation("Deserializing response from Overpass API...");
            var overpassResponse = JsonSerializer.Deserialize<OverpassResponseDTO>(jsonResponse, _jsonSerializerOptions) ?? throw new Exception("Error deserializing station data from Overpass API");

            return overpassResponse;
        }

        private static string GetRailwaysDataQuery() => @"
            [out:json][timeout:60];
            area['name:en'='Azerbaijan']->.a;
            way(area.a)['railway'='rail'];
            way(area.a)['railway'='subway'];
            out geom;
        ";

        //private static string GetRailwaysDataQuery() => @"
        //     [out:json][timeout:60];
        //        area['name:en'='Azerbaijan']->.a;
        //        way(area.a)['railway'='rail'];
        //        way(area.a)['railway'='subway'];
        //        out body;
        //        >;
        //        out skel qt;
        //";

        private static string GetStationsDataQuery() => @"
            [out:json][timeout:60];
            area['name:en'='Azerbaijan']->.a;
            (
              node(area.a)['railway'='station'];
              node(area.a)['railway'='halt'];
              way(area.a)['railway'='station'];
              way(area.a)['railway'='halt'];
            );
            out geom;
        ";

        //private static string GetStationsDataQuery() => @"
        //    [out:json][timeout:60];
        //    area['name:en'='Azerbaijan']->.a;
        //    (
        //      node(area.a)['railway'='station'];
        //      node(area.a)['railway'='halt'];
        //      way(area.a)['railway'='station'];
        //      way(area.a)['railway'='halt'];
        //    );
        //    out body;
        //    >;
        //    out skel qt;
        //";

        private static string GetPopulationCentersQuery() => @"
            [out:json][timeout:60];
            area['name:en'='Azerbaijan']->.a;
            (
              node(area.a)['place'~'city|town|village'];
              way(area.a)['place'~'city|town|village'];
              relation(area.a)['place'~'city|town|village'];
            );
            out geom;
        ";


        private static string GetTerrainObstaclesQuery() => @"
            [out:json][timeout:60];
            area['name:en'='Azerbaijan']->.a;
            (
              node(area.a)['natural'='peak'];
              node(area.a)['natural'='hill'];
              way(area.a)['natural'='cliff'];
              way(area.a)['natural'='wetland'];
              way(area.a)['landuse'='forest'];
              way(area.a)['natural'='water'];
              way(area.a)['barrier'];
              relation(area.a)['boundary'='protected_area'];
            );
            out geom;
        ";

        private static string GetNaturalTerrainObstaclesQuery() => @"
            [out:json][timeout:60];
            area['name:en'='Azerbaijan']->.a;
            (
              node(area.a)['natural'='peak'];
              node(area.a)['natural'='hill'];
              way(area.a)['natural'='cliff'];
              way(area.a)['natural'='wetland'];
              way(area.a)['landuse'='forest'];
              way(area.a)['natural'='water'];
            );
            out geom;
        ";

        private static string GetManMadeTerrainObstaclesQuery() => @"
            [out:json][timeout:60];
            area['name:en'='Azerbaijan']->.a;
            (
              way(area.a)['barrier'];
              relation(area.a)['boundary'='protected_area'];
            );
            out geom;
        ";
    }
}
