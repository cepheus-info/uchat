using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Capture;
using Windows.Media.Core;
using Windows.Media.MediaProperties;
using Windows.Media.Playback;
using Windows.Media.SpeechSynthesis;
using Windows.Storage;
using Windows.Storage.Streams;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace UChat
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private MediaCapture _mediaCapture;
        private StorageFile _recordingFile;
        private MediaPlayer _mediaPlayer = new MediaPlayer();

        private readonly IHttpClientFactory httpClientFactory;

        private Lazy<MainPageViewModel> LazyMainPageViewModel { get; } = new Lazy<MainPageViewModel>(() => App.ServiceProvider.GetRequiredService<MainPageViewModel>());
        public MainPageViewModel MainPageViewModel { get { return LazyMainPageViewModel.Value; } }

        private Lazy<SettingsViewModel> LazySettingsViewModel { get; } = new Lazy<SettingsViewModel>(() => App.ServiceProvider.GetRequiredService<SettingsViewModel>());
        public SettingsViewModel SettingsViewModel { get { return LazySettingsViewModel.Value; } }

        public MainPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;

            this.DataContext = MainPageViewModel;

            #region MediaCapture
            _mediaCapture = new MediaCapture();
            var settings = new MediaCaptureInitializationSettings { StreamingCaptureMode = StreamingCaptureMode.Audio };
            _ = _mediaCapture.InitializeAsync(settings);
            #endregion

            #region HttpClient
            this.httpClientFactory = App.ServiceProvider.GetRequiredService<IHttpClientFactory>();
            #endregion
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
        }

        public async Task<string> SendRequestAsync()
        {
            // await Task.Delay(1000);
            // return "{\"data\": \"What I asked; What UChat answered " + Random.Shared.Next() + "\"}";
            using (var client = this.httpClientFactory.CreateClient("MyHttpClient"))
            {
                using (var content = new MultipartFormDataContent())
                {
                    IBuffer buffer = await FileIO.ReadBufferAsync(_recordingFile);
                    byte[] fileBytes = WindowsRuntimeBufferExtensions.ToArray(buffer);
                    ByteArrayContent fileContent = new ByteArrayContent(fileBytes);

                    content.Add(fileContent, "file", _recordingFile.Name);

                    string api = SettingsViewModel.ApiUrl;

                    HttpResponseMessage response = await client.PostAsync(api, content);

                    response.EnsureSuccessStatusCode();

                    return await response.Content.ReadAsStringAsync();
                }
            }
        }
        public async Task TextToSpeech(string text)
        {
            var synthesizer = new SpeechSynthesizer();
            SpeechSynthesisStream synthesisStream = await synthesizer.SynthesizeTextToStreamAsync(text);

            _mediaPlayer.Source = MediaSource.CreateFromStream(synthesisStream, synthesisStream.ContentType);
            _mediaPlayer.Play();
        }

        public async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var message = (Message)button.Tag;

            if (button.Content.Equals("🔊"))
            {
                if (message.Audio == null)
                {
                    await TextToSpeech(message.Content);
                }
                else
                {
                    //await TextToSpeech(message.Content);
                    _mediaPlayer.Source = await CreateMediaSourceFromStorageFile(message.Audio);
                    _mediaPlayer.Play();
                }
                button.Content = "⏹";
            }
            else
            {
                _mediaPlayer.Source = null;
                _mediaPlayer.PlaybackSession.Position = TimeSpan.Zero;
                button.Content = "🔊";
            }
        }

        public async Task<MediaSource> CreateMediaSourceFromStorageFile(StorageFile file)
        {
            IRandomAccessStreamWithContentType stream = await file.OpenReadAsync();
            return MediaSource.CreateFromStream(stream, contentType: "MP3");
        }

        public async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine($"{nameof(SendButton_Click)}");
            MainPageViewModel.OperationHistory.Add("Sending message...");

            try
            {
                string responseJson = await SendRequestAsync();

                string[] messages = responseJson.Split(';');

                MainPageViewModel.OperationHistory.Add($"Received Message: {responseJson}");

                Message tmpMessage = MainPageViewModel.Messages.Last();
                MainPageViewModel.Messages.Remove(tmpMessage);
                MainPageViewModel.Messages.Add(new Message { Sender = tmpMessage.Sender, Audio = tmpMessage.Audio, Content = messages[0].Trim() });
                MainPageViewModel.Messages.Add(new Message { Sender = "[UChat]", Content = messages[1].Trim() });

                MainPageViewModel.IsRecordingAvailable = false;

                await TextToSpeech(messages[1].Trim());
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                MainPageViewModel.OperationHistory.Add(ex.ToString());
            }
        }

        public async void RecordButton_Click(Object sender, RoutedEventArgs e)
        {
            Debug.WriteLine($"{nameof(RecordButton_Click)}");
            if (!MainPageViewModel.IsRecording)
            {
                MainPageViewModel.RecordingStatus = "Recording...";
                MainPageViewModel.OperationHistory.Add("Starting recording...");
                _recordingFile = await ApplicationData.Current.LocalFolder.CreateFileAsync("Recording.mp3", CreationCollisionOption.GenerateUniqueName);
                await _mediaCapture.StartRecordToStorageFileAsync(MediaEncodingProfile.CreateMp3(AudioEncodingQuality.Auto), _recordingFile);
                MainPageViewModel.IsRecording = true;
            }
            else
            {
                MainPageViewModel.RecordingStatus = "Idle";
                await _mediaCapture.StopRecordAsync();
                var duration = await _recordingFile.Properties.GetMusicPropertiesAsync();
                MainPageViewModel.OperationHistory.Add($"You have recorded a {duration.Duration.TotalSeconds}s message at {_recordingFile.Path}");
                MainPageViewModel.Messages.Add(new Message { Sender = "[Me]", Content = "", Audio = _recordingFile });
                MainPageViewModel.IsRecording = false;
                MainPageViewModel.IsRecordingAvailable = true;
            }
        }
    }
}
