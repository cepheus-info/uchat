using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using UChat.Services.Interfaces;
using UChat.Models;
using Windows.Storage;
using Windows.Storage.Streams;
using UChat.Services;

namespace UChat.ViewModels
{
    /// <summary>
    /// ViewModel for the MainPage. It handles the business logic for recording and sending audio messages.
    /// </summary>
    public class MainPageViewModel : INotifyPropertyChanged
    {
        private StorageFile recordingFile;

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

        /// <summary>
        /// Gets the image for the record button
        /// </summary>
        public string RecordButtonImage => IsRecording ? "ms-appx:///Assets/RecordButtonPressed.png" : "ms-appx:///Assets/RecordButton.png";

        #region ObservableCollection<string> OperationHistory
        private ObservableCollection<string> _operationHistory = new ObservableCollection<string>();

        /// <summary>
        /// Gets or sets the operation history.
        /// </summary>
        public ObservableCollection<string> OperationHistory
        {
            get => this._operationHistory;
            set
            {
                this._operationHistory = value;
                OnPropertyChanged();
            }
        }
        #endregion

        /// <summary>
        /// Gets the collection of messages.
        /// </summary>
        public ObservableCollection<Message> Messages { get; } = new ObservableCollection<Message>();

        private readonly ISettings _settings;
        private readonly IRecordingService _recordingService;
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
        public IAsyncRelayCommand PlayAudioCommand { get; }

        public MainPageViewModel(
            ISettings settings,
            IRecordingService recordingService,
            IApiService apiService,
            TextToSpeechContext textToSpeech,
            IAudioPlayer audioPlayer)
        {
            _settings = settings;
            _recordingService = recordingService;

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
            PlayAudioCommand = new AsyncRelayCommand<Message>(async (message) => await PlayAudioAsync(message));
            _audioPlayer = audioPlayer;
            #endregion
        }

        private async Task StartRecordingAsync()
        {
            OperationHistory.Add("Starting recording...");
            recordingFile = await _recordingService.CreateRecordingFileAsync("Recording.mp3");
            await _recordingService.StartRecordingAsync(recordingFile);
            IsRecording = true;
        }

        private async Task StopRecordingAsync()
        {
            await _recordingService.StopRecordingAsync();
            var duration = await _recordingService.GetRecordingDurationAsync(recordingFile);
            OperationHistory.Add($"You have recorded a {duration.TotalSeconds}s message at {recordingFile.Path}");
            Messages.Add(new Message
            {
                Sender = "[Me]",
                Content = "",
                Audio = recordingFile
            });
            IsRecording = false;
            IsRecordedFileAvailable = true;
            IsCancelAction = false;
        }

        private async Task CancelRecordingAsync()
        {
            OperationHistory.Add("Cancel recording.");
            await _recordingService.StopRecordingAsync();
            await recordingFile.DeleteAsync();
            IsRecording = false;
            IsRecordedFileAvailable = false;
            IsCancelAction = false;
        }

        /// <summary>
        /// Sends the audio message and handles the response.
        /// </summary>
        /// <returns></returns>
        public async Task SendMessageAsync()
        {
            OperationHistory.Add("Sending message...");

            try
            {
                IsRecordedFileAvailable = false;
                var audioMessage = Messages.Last();
                var responseString = await SendAudioAsync(audioMessage);
                OperationHistory.Add($"Received Message: {responseString}");

                var responseMessages = ParseResponse(responseString);

                ReplaceMessageInList(audioMessage, responseMessages);

                await SpeakResponseAsync(responseMessages.Last());
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                OperationHistory.Add(ex.ToString());
                IsRecordedFileAvailable = true;
            }
        }

        private async Task<string> SendAudioAsync(Message audioMessage)
        {
            var buffer = await FileIO.ReadBufferAsync(audioMessage.Audio);
            return await _apiService.SendRequestAsync(buffer, audioMessage.Audio.Name);
        }

        private List<Message> ParseResponse(string responseString)
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

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
