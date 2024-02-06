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
        Task<StorageFile> CreateRecordingFileAsync(string filename);

        Task StartRecordingAsync(StorageFile file);

        Task StopRecordingAsync();

        Task<TimeSpan> GetRecordingDurationAsync(StorageFile file);
    }
}
