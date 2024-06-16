using System;
using System.Diagnostics;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using UChat.Services.Interfaces;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;

namespace UChat.Services.Implementations
{
    public class RecordingService : IRecordingService
    {
        // Add the AudioDataStream property signature
        public IObservable<float[]> AudioDataStream => new Subject<float[]>();

        private readonly MediaCapture _mediaCapture = new();
        private bool _isInitialized = false;

        public StorageFile RecordingFile { get; private set; }

        public RecordingService()
        {
            
        }

        public async Task InitializeRecordingAsync(string filename)
        {
            if(!_isInitialized)
            {
                try
                {
                    var settings = new MediaCaptureInitializationSettings
                    {
                        StreamingCaptureMode = StreamingCaptureMode.Audio
                    };
                    await _mediaCapture.InitializeAsync(settings);
                    _isInitialized = true;
                }
                catch (Exception exception)
                {
                    Debug.WriteLine(exception.Message);
                    return;
                }
            }
            RecordingFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(filename, CreationCollisionOption.GenerateUniqueName);
        }

        public async Task StartRecordingAsync()
        {
            if (RecordingFile == null)
            {
                throw new InvalidOperationException("Recording file must be initialized first.");
            }
            try
            {
                await _mediaCapture.StartRecordToStorageFileAsync(MediaEncodingProfile.CreateMp3(AudioEncodingQuality.Auto), RecordingFile);
            }
            catch (System.Exception exception)
            {
                Debug.WriteLine(exception.Message);
                // Cleanup if starting the recording fails
                await CleanupRecordingAsync();
                throw; // Re-throw the exception to notify the caller
            }
        }

        public async Task StopRecordingAsync()
        {
            try
            {
                await _mediaCapture.StopRecordAsync();
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.Message);
                // Cleanup if stopping the recording fails
                await CleanupRecordingAsync();
                throw; // Optional: Decide whether to re-throw based on your error handling policy
            }
        }

        private async Task CleanupRecordingAsync()
        {
            if (RecordingFile != null)
            {
                try
                {
                    await RecordingFile.DeleteAsync();
                }
                catch (Exception exception)
                {
                    // Log or handle the error if the file deletion fails
                    Debug.WriteLine(exception.Message);
                }
                finally
                {
                    RecordingFile = null; // Ensure the reference is cleared
                }
            }
        }
    }
}
