using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TravelMemoriesBackend.Contracts.Storage;

namespace TravelMemoriesBackend.ApiClient.TripData
{
    public interface ITripDataClient
    {
        Task<int> GetTotalTripsCountAsync();

        Task<List<ImageData>> GetAllTripDataAync(string token);
    }

    public class TripDataClient : ITripDataClient
    {
        private readonly HttpClient _httpClient;

        private readonly JsonSerializerOptions jsonSerializerOptions;

        public TripDataClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
            jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<int> GetTotalTripsCountAsync()
        {
            return 30;
        }

        public async Task<List<ImageData>> GetAllTripDataAync(string token)
        {
            string url = "/ImageUpload/AllTripData";
            return await SendRequestAsync<List<ImageData>>(HttpMethod.Get, url, token);
        }

        public async Task<TResponse> SendRequestAsync<TResponse>(HttpMethod httpMethod, string url, string accessToken)
        {
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage(httpMethod, url);
            httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            HttpResponseMessage httpResponseMessage = await _httpClient.SendAsync(httpRequestMessage);
            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Failed to fetch all trips data, {httpResponseMessage.ReasonPhrase}, status code: {httpResponseMessage.StatusCode}");
            }

            return JsonSerializer.Deserialize<TResponse>(await httpResponseMessage.Content.ReadAsStringAsync(), jsonSerializerOptions);
        }
    }
}
