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
using Windows.Foundation;
using MathNet.Numerics.IntegralTransforms;
using System.Numerics;
using Windows.Media.MediaProperties;

namespace UChat.Services.Implementations
{
    public class AudioGraphRecordingService : IRecordingService
    {
        private AudioGraph _audioGraph;
        private AudioFileOutputNode _fileOutputNode;
        private AudioFrameOutputNode _frameOutputNode;

        public StorageFile RecordingFile { get; private set; }

        #region AudioFrame
        private Subject<AudioFrame> _audioFrameStream = new Subject<AudioFrame>();
        private IObservable<AudioFrame> _throttledAudioFrameStream;
        #endregion

        #region Observable stream for audio data
        private Subject<float[]> _audioDataStream = new();
        public IObservable<float[]> AudioDataStream => _audioDataStream.AsObservable();
        #endregion

        public async Task InitializeRecordingAsync(string filename)
        {
            #region AudioFrame stream
            // Throttle the stream to process an audio frame every 100 milliseconds, for example
            _throttledAudioFrameStream = _audioFrameStream.Sample(TimeSpan.FromMilliseconds(100));

            _throttledAudioFrameStream.Subscribe(async frame =>
            {
                float[] audioData = ExtractAudioData(frame);
                float[] frequencyData = await PerformFFTAsync(audioData);
                _audioDataStream.OnNext(frequencyData);
            });
            #endregion

            RecordingFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(filename, CreationCollisionOption.GenerateUniqueName);
            var audioGraphSettings = new AudioGraphSettings(Windows.Media.Render.AudioRenderCategory.Media)
            {
                EncodingProperties = AudioEncodingProperties.CreatePcm(44100, 1, 16), // 48 kHz, stereo, 16-bit PCM
                QuantumSizeSelectionMode = QuantumSizeSelectionMode.LowestLatency, // Performances vs. latency trade-off
                DesiredSamplesPerQuantum = 480 // Example value, adjust based on your needs
            };
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
            deviceInputNode.AddOutgoingConnection(_fileOutputNode);
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
            AudioFrame frame = _frameOutputNode.GetFrame();
            _audioFrameStream.OnNext(frame);
        }

        private float[] ExtractAudioData(AudioFrame frame)
        {
            using (AudioBuffer buffer = frame.LockBuffer(AudioBufferAccessMode.Read))
            using (IMemoryBufferReference reference = buffer.CreateReference())
            {
                // Cast the IMemoryBufferReference to IMemoryBufferByteAccess using As<T>
                var byteAccess = reference.As<IMemoryBufferByteAccess>();

                unsafe
                {
                    byte* dataInBytes;
                    uint capacity;
                    byteAccess.GetBuffer(out dataInBytes, out capacity);

                    // Assuming 16-bit PCM audio
                    int sampleCount = (int)capacity / 2; // 2 bytes per sample for 16-bit audio
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

        public async Task<float[]> PerformFFTAsync(float[] audioData)
        {
            return await Task.Run(() =>
            {
                // Ensure the number of samples is a power of two for FFT
                int n = audioData.Length;
                Complex[] complexSamples = new Complex[n];

                // Convert audio samples to complex numbers (real part is the sample, imaginary part is 0)
                for (int i = 0; i < n; i++)
                {
                    complexSamples[i] = new Complex(audioData[i], 0);
                }

                // Apply FFT
                Fourier.Forward(complexSamples, FourierOptions.Matlab);

                // Extract frequency magnitudes
                float[] magnitudes = complexSamples.Select(c => (float)c.Magnitude).ToArray();

                return magnitudes;
            });
        }

    }
}
