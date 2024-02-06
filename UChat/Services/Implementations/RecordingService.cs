using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UChat.Services.Interfaces;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;

namespace UChat.Services.Implementations
{
    public class RecordingService : IRecordingService
    {
        private readonly MediaCapture _mediaCapture = new MediaCapture();

        public RecordingService()
        {
            var settings = new MediaCaptureInitializationSettings
            {
                StreamingCaptureMode = StreamingCaptureMode.Audio
            };
            _ = _mediaCapture.InitializeAsync(settings);
        }

        public async Task<StorageFile> CreateRecordingFileAsync(string filename)
        {
            return await ApplicationData.Current.LocalFolder.CreateFileAsync(filename, CreationCollisionOption.GenerateUniqueName);
        }

        public async Task<TimeSpan> GetRecordingDurationAsync(StorageFile file)
        {
            var properties = await file.Properties.GetMusicPropertiesAsync();
            return properties.Duration;
        }

        public async Task StartRecordingAsync(StorageFile file)
        {
            await _mediaCapture.StartRecordToStorageFileAsync(MediaEncodingProfile.CreateMp3(AudioEncodingQuality.Auto), file);
        }

        public async Task StopRecordingAsync()
        {
            await _mediaCapture.StopRecordAsync();
        }
    }
}
