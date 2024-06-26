﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using UChat.Models;
using UChat.Services;
using UChat.Services.Interfaces;
using Windows.Storage;

namespace UChat.ViewModels
{
    /// <summary>
    /// ViewModel for the MainPage. It handles the business logic for recording and sending audio messages.
    /// </summary>
    public class MainPageViewModel : ObservableRecipient
    {
        #region IsDebugMode
        private bool _isDebugMode;
        public bool IsDebugMode
        {
            get => _isDebugMode;
            private set
            {
                _isDebugMode = value;
                OnPropertyChanged(nameof(IsDebugMode));
            }
        }
        #endregion

        #region bool IsRecordingAvailable
        private bool _isRecordedFileAvailable = false;
        /// <summary>
        /// Gets or Sets a value indicating whether a recorded file is available.
        /// </summary>
        public bool IsRecordedFileAvailable
        {
            get => _isRecordedFileAvailable;
            set
            {
                _isRecordedFileAvailable = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanSend));
            }
        }
        #endregion

        #region bool IsRecording
        private bool _isRecording = false;
        /// <summary>
        /// Gets or sets a value indicating whether the app is currently recording.
        /// </summary>
        public bool IsRecording
        {
            get => _isRecording;
            set
            {
                _isRecording = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsRecording));
                OnPropertyChanged(nameof(IsNotRecording));
                OnPropertyChanged(nameof(RecordButtonImage));
                OnPropertyChanged(nameof(IsReleaseToSendVisible));
                OnPropertyChanged(nameof(IsReleaseToCancelVisible));
                OnPropertyChanged(nameof(CanSend));
            }
        }
        #endregion

        #region bool IsNotRecording
        /// <summary>
        /// Gets a value indicating whether the app is not currently recording.
        /// </summary>
        public bool IsNotRecording => !IsRecording;
        #endregion

        #region bool IsCancelAction
        private bool _isCancelAction = false;
        /// <summary>
        /// Gets or sets a value indicating whether the current action is intended to be canceled.
        /// </summary>
        public bool IsCancelAction
        {
            get => _isCancelAction;
            set
            {
                if (_isCancelAction != value)
                {
                    _isCancelAction = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsCancelAction));
                    // You can also trigger other property changes if needed
                    OnPropertyChanged(nameof(IsReleaseToSendVisible));
                    OnPropertyChanged(nameof(IsReleaseToCancelVisible));
                }
            }
        }
        #endregion

        /// <summary>
        /// Gets a value indicating whether a message can be sent.
        /// </summary>
        public bool CanSend => !IsRecording && IsRecordedFileAvailable;

        /// <summary>
        /// ReleaseToSend is Visible
        /// </summary>
        public bool IsReleaseToSendVisible => IsRecording && !IsCancelAction;

        /// <summary>
        /// ReleaseToCancel is Visible
        /// </summary>
        public bool IsReleaseToCancelVisible => IsRecording && IsCancelAction;

        public double CanvasWidth => IsReleaseToSendVisible ? 400.0 : 150.0;

        /// <summary>
        /// Gets the image for the record button
        /// </summary>
        public string RecordButtonImage => IsRecording ? "ms-appx:///Assets/RecordButtonPressed.png" : "ms-appx:///Assets/RecordButton.png";

        #region Visualize the audio waveform
        private ObservableCollection<WaveformPoint> _waveformPoints = new();
        public ObservableCollection<WaveformPoint> WaveformPoints
        {
            get => _waveformPoints;
            set
            {
                _waveformPoints = value;
                OnPropertyChanged(nameof(WaveformPoints));
            }
        }
        #endregion

        #region ObservableCollection<Operation> OperationHistory
        private ObservableCollection<Operation> _operationHistory = new();

        /// <summary>
        /// Gets or sets the operation history.
        /// </summary>
        public ObservableCollection<Operation> OperationHistory
        {
            get => this._operationHistory;
            set
            {
                this._operationHistory = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region DebugMessages
        private string _debugMessages;
        public string DebugMessages
        {
            get => _debugMessages;
            set
            {
                _debugMessages = value;
                OnPropertyChanged();
            }
        }

        private void OperationHistory_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            DebugMessages = string.Join(Environment.NewLine, OperationHistory.Select(op => $"{op.TimeStamp} {op.Description}"));
        }
        #endregion

        /// <summary>
        /// Gets the collection of messages.
        /// </summary>
        public ObservableCollection<Message> Messages { get; } = new ObservableCollection<Message>();

        private readonly ISettings _settings;
        private readonly IRecordingService _recordingService;
        private IDisposable _audioDataStreamSubscription;
        private readonly IApiService _apiService;
        private readonly TextToSpeechContext _textToSpeech;
        private readonly IAudioPlayer _audioPlayer;

        /// <summary>
        /// Gets the command for recording.
        /// </summary>
        public IAsyncRelayCommand StartRecordingCommand { get; }

        /// <summary>
        /// Gets the command for stopping the recording.
        /// </summary>
        public IAsyncRelayCommand StopRecordingCommand { get; }

        /// <summary>
        /// Cancel the recording.
        /// </summary>
        public IAsyncRelayCommand CancelRecordingCommand { get; }

        /// <summary>
        /// Gets the command for sending a message.
        /// </summary>
        public IAsyncRelayCommand SendCommand { get; }

        /// <summary>
        /// Gets the command for playing audio.
        /// </summary>
        public IRelayCommand PlayAudioCommand { get; }

        public MainPageViewModel(
            ISettings settings,
            IRecordingService recordingService,
            IApiService apiService,
            TextToSpeechContext textToSpeech,
            IAudioPlayer audioPlayer)
        {
            _settings = settings;
            _recordingService = recordingService;

            IsDebugMode = _settings.IsDebugMode;
            OperationHistory.CollectionChanged += OperationHistory_CollectionChanged;
            // Subscribe to the PropertyChangedMessage<bool> to update the IsDebugMode property
            Messenger.Register<PropertyChangedMessage<bool>>(this, (r, m) => IsDebugMode = m.NewValue);

            // Subscribe to the Unhandled ExceptionMessage to log the exception
            Messenger.Register<ExceptionMessage>(this, (r, m) =>
            {
                OperationHistory.Add(new Operation(m.TimeStamp, m.Message));
            });

            #region StartRecordingCommand
            StartRecordingCommand = new AsyncRelayCommand(async () => await StartRecordingAsync());
            #endregion

            #region StopRecordingCommand
            StopRecordingCommand = new AsyncRelayCommand(async () => await StopRecordingAsync());
            #endregion

            #region CancelRecordingCommand
            CancelRecordingCommand = new AsyncRelayCommand(async () => await CancelRecordingAsync());
            #endregion

            #region SendCommand
            SendCommand = new AsyncRelayCommand(async () => await SendMessageAsync());
            _apiService = apiService;
            _textToSpeech = textToSpeech;
            #endregion

            #region PlayAudioCommand
            PlayAudioCommand = new RelayCommand<Message>(async (message) => await PlayAudioAsync(message));
            _audioPlayer = audioPlayer;
            #endregion
        }

        private async Task StartRecordingAsync()
        {
            #region Subscribe to the AudioDataStream
            // Subscribe to the audio data stream
            _audioDataStreamSubscription = _recordingService.AudioDataStream
                .Sample(TimeSpan.FromMilliseconds(100)) // Example of backpressure, adjust as needed
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe(audioData =>
                {
                    // Update the UI based on audioData, e.g., visualize the audio waveform
                    VisualizeAudioData(audioData);
                });
            #endregion

            OperationHistory.Add(new Operation(DateTime.Now, "Starting recording..."));
            await _recordingService.InitializeRecordingAsync("Recording.mp3");
            await _recordingService.StartRecordingAsync();
            IsRecording = true;
        }

        private void VisualizeAudioData(float[] audioData)
        {
            if (audioData.Length == 0)
            {
                return;
            }

            // Example: Normalize and add audio data points to WaveformPoints
            // Clear the existing points
            WaveformPoints.Clear();

            int targetPointCount = (int)Math.Ceiling(CanvasWidth / 4);
            // Calculate the number of audio samples to group together for each point
            int sampleInterval = (int)Math.Ceiling((double)audioData.Length / targetPointCount);

            double maxSampleValue = audioData.Max(Math.Abs);

            for (int i = 0; i < targetPointCount; i++)
            {
                // Calculate the start index of the current group of samples
                int startIndex = i * sampleInterval;
                // Ensure we don't exceed the bounds of the audioData array
                int endIndex = Math.Min(startIndex + sampleInterval, audioData.Length);

                // Calculate the average or peak value of the current group of samples
                double sampleValue = 0;
                for (int j = startIndex; j < endIndex; j++)
                {
                    sampleValue += Math.Abs(audioData[j]);
                }
                sampleValue /= (endIndex - startIndex);

                // Normalize the sample value to fit the visualization range
                double normalizedSample = (sampleValue / maxSampleValue) * 100;
                double canvasLeft = i * (CanvasWidth / targetPointCount) + 2; // Add offset for the border
                double canvasTop = (100 - normalizedSample) / 2;

                // Add the normalized sample to the collection
                WaveformPoints.Add(new WaveformPoint(normalizedSample, canvasLeft, canvasTop));
            }
        }

        private async Task StopRecordingAsync()
        {
            await _recordingService.StopRecordingAsync();
            var duration = await _recordingService.GetRecordingDurationAsync();
            var durationText = duration.TotalSeconds > 0 ? $"{duration.TotalSeconds}s " : "";
            OperationHistory.Add(new Operation
            {
                TimeStamp = DateTime.Now,
                Description = $"You have recorded a {durationText}message at {_recordingService.RecordingFile?.Path}"
            });
            Messages.Add(new Message
            {
                Sender = "[Me]",
                Content = "",
                Audio = _recordingService.RecordingFile
            });
            IsRecording = false;
            IsRecordedFileAvailable = true;
            IsCancelAction = false;

            // Dispose of the subscription when recording stops
            _audioDataStreamSubscription?.Dispose();
        }

        private async Task CancelRecordingAsync()
        {
            OperationHistory.Add(new Operation(DateTime.Now, "Cancel recording."));
            await _recordingService.StopRecordingAsync();
            await _recordingService.RecordingFile?.DeleteAsync();
            IsRecording = false;
            IsRecordedFileAvailable = false;
            IsCancelAction = false;

            // Dispose of the subscription when recording stops
            _audioDataStreamSubscription?.Dispose();
        }

        /// <summary>
        /// Sends the audio message and handles the response.
        /// </summary>
        /// <returns></returns>
        public async Task SendMessageAsync()
        {
            OperationHistory.Add(new Operation(DateTime.Now, "Sending message..."));

            try
            {
                IsRecordedFileAvailable = false;
                var audioMessage = Messages.Last();
                var responseString = await SendAudioAsync(audioMessage);
                OperationHistory.Add(new Operation(DateTime.Now, $"Received Message: {responseString}"));

                var responseMessages = ParseResponse(responseString);

                ReplaceMessageInList(audioMessage, responseMessages);

                await SpeakResponseAsync(responseMessages.Last());
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                OperationHistory.Add(new Operation(DateTime.Now, ex.ToString()));
                IsRecordedFileAvailable = true;
            }
        }

        private async Task<string> SendAudioAsync(Message audioMessage)
        {
            var buffer = await FileIO.ReadBufferAsync(audioMessage.Audio);
            return await _apiService.SendRequestAsync(buffer, audioMessage.Audio.Name);
        }

        private static List<Message> ParseResponse(string responseString)
        {
            var parts = responseString.Split(';');
            return new List<Message>
            {
                new Message { Sender = "[Me]", Content = parts[0] },
                new Message { Sender = "[UChat]", Content = parts[1] }
            };
        }

        private void ReplaceMessageInList(Message originalMessage, List<Message> responseMessages)
        {
            var index = Messages.IndexOf(originalMessage);
            if (index != -1)
            {
                var requestMessage = responseMessages[0];
                requestMessage.Audio = originalMessage.Audio;
                Messages[index] = requestMessage;
                Messages.Insert(index + 1, responseMessages[1]);
            }
        }

        private async Task SpeakResponseAsync(Message responseMessage)
        {
            await _textToSpeech.GetService().Speak(responseMessage.Content);
        }

        /// <summary>
        /// Plays the audio of the message or speaks the message if there is no audio.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task PlayAudioAsync(Message message)
        {
            if (message.Audio == null)
            {
                await _textToSpeech.GetService().Speak(message.Content);
            }
            else
            {
                if (!message.IsPlaying)
                {
                    message.IsPlaying = true;
                    await _audioPlayer.PlayAudioAsync(message.Audio);
                    // This is clever but needs the knowledge of Reactive-Programming,
                    // which construct the PlayAudioAsync to only return after finished playing.
                    // Otherwise, you'll need to register an event handler in this ViewModel, and assign the message.IsPlaying = false in the handler,
                    // which involves another problem of how the message should be tracked in that context.
                    message.IsPlaying = false;
                }
                else
                {
                    _audioPlayer.StopAudio();
                    message.IsPlaying = false;
                }
            }
        }
    }
}
