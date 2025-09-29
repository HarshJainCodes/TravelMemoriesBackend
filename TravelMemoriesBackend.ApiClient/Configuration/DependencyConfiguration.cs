using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelMemoriesBackend.ApiClient.TripData;

namespace TravelMemoriesBackend.ApiClient.Configuration
{
    public static class DependencyConfiguration
    {
        public static IServiceCollection AddApiClients(this IServiceCollection services, string url)
        {
            services.AddHttpClient<ITripDataClient, TripDataClient>(delegate (HttpClient client)
            {
                client.BaseAddress = new Uri(url);
            });

            return services;
        }
    }
}
