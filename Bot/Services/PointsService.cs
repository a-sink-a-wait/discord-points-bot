using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Bot.Services
{
    public class PointsService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public PointsService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<bool> IsPlayerTimedOut(string playerId, string source)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var playerTimedOut = await httpClient.GetAsync($"{_configuration["QueryBaseEndpoint"]}timeout/{source}/{playerId}?code={_configuration["QueryKey"]}");

            var timedOut = await playerTimedOut.Content.ReadAsStringAsync();
            return Boolean.Parse(timedOut);
        }
    }

    public class WarService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public WarService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<int> GetPointsFromThreshold(string sourcePlayer, string targetPlayer)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var url = $"{_configuration["WarBaseEndpoint"]}GetWarCounter?code={_configuration["WarQuery"]}";

            var result = await httpClient.GetAsync(url);
            var content = await result.Content.ReadAsStringAsync();

            return Int32.Parse(content);
        }
    }
}