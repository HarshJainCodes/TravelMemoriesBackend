using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using TravelMemoriesBackend.ApiClient.TripData;

namespace TravelMemoriesBackend.ApiClient.DependencyConfiguration
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
