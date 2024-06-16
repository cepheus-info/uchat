using System;
using System.Threading.Tasks;
using UChat.Services.Interfaces;
using Windows.Storage.Streams;

namespace UChat.Services.Implementations
{
    public class MockApiService : IApiService
    {
        public async Task<string> SendRequestAsync(IBuffer buffer, string name)
        {
            // Simulate network delay
            await Task.Delay(1000);

            // Return a predefined response or simulate based on input parameters
            return $"User said hello via {name}; UChat replied sorry I am a bot";
        }
    }
}
