using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using UChat.Services.Interfaces;
using Windows.Devices.Enumeration;
using Windows.Media.Audio;
using Windows.Media.Capture;
using Windows.Media.Devices;
using Windows.Storage;
using Windows.Media;
using Windows.Storage.Streams;
using WinRT;
using System.Runtime.InteropServices.WindowsRuntime;

namespace UChat.Services.Implementations
{
    public class AudioGraphRecordingService : IRecordingService
    {
        private AudioGraph _audioGraph;
        private AudioFileOutputNode _fileOutputNode;
        private AudioFrameOutputNode _frameOutputNode;

        public StorageFile RecordingFile { get; private set; }

        // Observable stream for audio data
        private Subject<float[]> _audioDataStream = new();

        public IObservable<float[]> AudioDataStream => _audioDataStream.AsObservable();

        public async Task InitializeRecordingAsync(string filename)
        {
            RecordingFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(filename, CreationCollisionOption.GenerateUniqueName);

            var audioGraphSettings = new AudioGraphSettings(Windows.Media.Render.AudioRenderCategory.Media);
            var createGraphResult = await AudioGraph.CreateAsync(audioGraphSettings);

            if (createGraphResult.Status != AudioGraphCreationStatus.Success)
            {
                throw new Exception("Unable to create audio graph.");
            }

            _audioGraph = createGraphResult.Graph;

            var createFileOutputResult = await _audioGraph.CreateFileOutputNodeAsync(RecordingFile);

            if (createFileOutputResult.Status != AudioFileNodeCreationStatus.Success)
            {
                throw new Exception("Unable to create file output node.");
            }

            _fileOutputNode = createFileOutputResult.FileOutputNode;
        }

        public async Task StartRecordingAsync()
        {
            if (_audioGraph == null || _fileOutputNode == null)
            {
                throw new InvalidOperationException("AudioGraph is not initialized.");
            }

            var microphone = await DeviceInformation.CreateFromIdAsync(MediaDevice.GetDefaultAudioCaptureId(AudioDeviceRole.Default));
            var deviceInputNodeResult = await _audioGraph.CreateDeviceInputNodeAsync(MediaCategory.Other, _audioGraph.EncodingProperties, microphone);

            if (deviceInputNodeResult.Status != AudioDeviceNodeCreationStatus.Success)
            {
                throw new Exception("Unable to create device input node.");
            }

            // Create the AudioFrameOutputNode
            _frameOutputNode = _audioGraph.CreateFrameOutputNode();

            var deviceInputNode = deviceInputNodeResult.DeviceInputNode;
            //deviceInputNode.AddOutgoingConnection(_fileOutputNode);
            deviceInputNode.AddOutgoingConnection(_frameOutputNode);

            // Listen to the QuantumStarted event
            _audioGraph.QuantumStarted += AudioGraph_QuantumStarted;

            _audioGraph.Start();
        }

        public async Task StopRecordingAsync()
        {
            if (_audioGraph != null)
            {
                _audioGraph.Stop();
                await _fileOutputNode.FinalizeAsync();
            }
        }

        private async Task CleanupAsync()
        {
            if (_frameOutputNode != null)
            {
                _frameOutputNode.Dispose();
                _frameOutputNode = null;
            }
            if (_fileOutputNode != null)
            {
                _fileOutputNode.Dispose();
                _fileOutputNode = null;
            }
            if (_audioGraph != null)
            {
                _audioGraph.Dispose();
                _audioGraph = null;
            }
            if (RecordingFile != null)
            {
                await RecordingFile.DeleteAsync();
                RecordingFile = null;
            }
        }

        private void AudioGraph_QuantumStarted(AudioGraph sender, object args)
        {
            // Process audio data on each quantum
            AudioFrame frame = _frameOutputNode.GetFrame();
            ProcessAudioData(frame);
        }

        // Method to process and publish audio data
        private void ProcessAudioData(AudioFrame frame)
        {
            // Process the frame to extract audio data (e.g., for visualization)
            // This is a simplified example. Actual implementation will depend on your audio processing logic.
            float[] audioData = ExtractAudioData(frame);
            _audioDataStream.OnNext(audioData);
        }

        private float[] ExtractAudioData(AudioFrame frame)
        {
            using (AudioBuffer buffer = frame.LockBuffer(AudioBufferAccessMode.Read))
            {
                // Access the underlying IBuffer of the AudioBuffer
                IBuffer ibuffer = Windows.Storage.Streams.Buffer.CreateCopyFromMemoryBuffer(buffer);
                if (ibuffer == null) return new float[0]; // or handle this scenario appropriately
                using (var reader = Windows.Storage.Streams.DataReader.FromBuffer(ibuffer))
                {
                    byte[] dataInBytes = new byte[reader.UnconsumedBufferLength];
                    reader.ReadBytes(dataInBytes);

                    // Assuming 16-bit PCM audio
                    int sampleCount = dataInBytes.Length / 2; // 2 bytes per sample for 16-bit audio
                    float[] audioData = new float[sampleCount];

                    for (int i = 0; i < sampleCount; i++)
                    {
                        // Convert two bytes to one short (little endian)
                        short sample = (short)(dataInBytes[i * 2] | dataInBytes[i * 2 + 1] << 8);
                        // Convert to float
                        audioData[i] = sample / 32768f; // Divide by max short value
                    }

                    return audioData;
                }
            }
        }

    }
}
