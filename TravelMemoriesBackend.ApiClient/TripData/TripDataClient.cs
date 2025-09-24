using System.Text.Json;
using TravelMemoriesBackend.Contracts.Storage;

namespace TravelMemoriesBackend.ApiClient.TripData
{
    public class TripDataClient : ITripDataClient
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions jsonSerializerOptions;

        public TripDataClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
            jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
        }

        public async Task<int> GetTotalTripsCountAsync()
        {
            return 30;
        }

        public async Task<List<ImageData>> GetAllTripDataAync(string token)
        {
            string url = $"/ImageUpload/AllTripData";

            var res = await SendRequestAsync<List<ImageData>>(HttpMethod.Get, url, token);
            return res;
        }

        public async Task<TResponse> SendRequestAsync<TResponse>(HttpMethod httpMethod, string url, string accessToken)
        {
            HttpRequestMessage request = new HttpRequestMessage(httpMethod, url);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            HttpResponseMessage response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Failed to fetch all trips data, {response.ReasonPhrase}, status code: {response.StatusCode}");
            }

            var data = JsonSerializer.Deserialize<TResponse>(await response.Content.ReadAsStringAsync(), jsonSerializerOptions);
            return data;

        }
    }
}
