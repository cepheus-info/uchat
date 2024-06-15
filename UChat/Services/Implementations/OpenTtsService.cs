using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UChat.Services.Interfaces;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage.Streams;

namespace UChat.Services.Implementations
{
    [TextToSpeech(tag: "OpenTTS")]
    public class OpenTtsService : ITextToSpeech
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ISettings _settings;
        private readonly MediaPlayer _mediaPlayer = new();

        public OpenTtsService(IHttpClientFactory httpClientFactory, ISettings settings, MediaPlayer mediaPlayer)
        {
            _httpClientFactory = httpClientFactory;
            _settings = settings;
        }

        public async Task Speak(string text)
        {
            using var client = _httpClientFactory.CreateClient(_settings.HttpClientName);
            var response = await client.PostAsync("http://your-open-tts-server/synthesize", new StringContent(text));

            response.EnsureSuccessStatusCode();

            var audioData = await response.Content.ReadAsByteArrayAsync();

            #region Play the Audio Data
            // Create an InMemoryRandomAccessStream and write the audio data to it
            using var stream = new InMemoryRandomAccessStream();
            using (var dataWriter = new DataWriter(stream.GetOutputStreamAt(0)))
            {
                dataWriter.WriteBytes(audioData);
                await dataWriter.StoreAsync();
            }

            // Create a MediaSource from the stream
            var mediaSource = MediaSource.CreateFromStream(stream, "audio/wav");

            //Play the audio data
            _mediaPlayer.Source = mediaSource;
            _mediaPlayer.Play();
            #endregion
        }
    }
}
