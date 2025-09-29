using TravelMemoriesBackend.Contracts.Storage;

namespace TravelMemoriesBackend.ApiClient.TripData
{
    public interface ITripDataClient
    {
        public Task<int> GetTotalTripsCountAsync();

        public Task<List<ImageData>> GetAllTripDataAync(string token);
    }
}
