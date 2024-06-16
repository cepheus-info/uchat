using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace UChat.Services.Interfaces
{
    public interface IRecordingService
    {
        // Add the AudioDataStream property signature
        IObservable<float[]> AudioDataStream { get; }

        StorageFile RecordingFile { get; }

        Task InitializeRecordingAsync(string filename);

        Task StartRecordingAsync();

        Task StopRecordingAsync();

        public async Task<TimeSpan> GetRecordingDurationAsync()
        {
            if (RecordingFile == null)
            {
                throw new InvalidOperationException("Recording file must be initialized first.");
            }
            var properties = await RecordingFile.Properties.GetMusicPropertiesAsync();
            return properties.Duration;
        }
    }
}
