using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using UChat.Services.Interfaces;
using Windows.Storage.Streams;

namespace UChat.Services.Implementations
{
    public class ApiService : IApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ISettings _settings;

        public ApiService(IHttpClientFactory httpClientFactory, ISettings settings)
        {
            _httpClientFactory = httpClientFactory;
            _settings = settings;
        }

        public async Task<string> SendRequestAsync(IBuffer buffer, string name)
        {
            using (var client = _httpClientFactory.CreateClient(_settings.HttpClientName))
            {
                using (var content = new MultipartFormDataContent())
                {
                    byte[] fileBytes = buffer.ToArray();
                    var fileContent = new ByteArrayContent(fileBytes);

                    content.Add(fileContent, name: "file", fileName: name);

                    var response = await client.PostAsync(_settings.ApiUrl, content);

                    response.EnsureSuccessStatusCode();

                    return await response.Content.ReadAsStringAsync();
                }
            }
        }
    }
}
