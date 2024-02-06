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

namespace UChat.ViewModels
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        #region StorageFile RecordingFile
        private StorageFile _recordingFile;
        public StorageFile RecordingFile { get => _recordingFile; }
        #endregion

        #region bool IsRecordingAvailable
        private bool _isRecordingAvailable = false;
        public bool IsRecordingAvailable
        {
            get => _isRecordingAvailable;
            set
            {
                _isRecordingAvailable = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanSend));
            }
        }
        #endregion

        #region string RecordingStatus
        private string _recordingStatus = "Idle";
        public string RecordingStatus
        {
            get { return _recordingStatus; }
            set
            {
                _recordingStatus = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region bool IsRecording
        private bool _isRecording = false;
        public bool IsRecording
        {
            get { return _isRecording; }
            set
            {
                _isRecording = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsRecording));
                OnPropertyChanged(nameof(RecordButtonText));
                OnPropertyChanged(nameof(CanSend));
            }
        }
        #endregion

        public bool CanSend
        {
            get { return !IsRecording && IsRecordingAvailable; }
        }

        public string RecordButtonText
        {
            get { return IsRecording ? "⏺ Stop Record" : "🎙Record"; }
        }

        #region ObservableCollection<string> OperationHistory
        private ObservableCollection<string> _operationHistory = new ObservableCollection<string>();
        public ObservableCollection<string> OperationHistory
        {
            get { return this._operationHistory; }
            set
            {
                this._operationHistory = value;
                OnPropertyChanged();
            }
        }
        #endregion

        public ObservableCollection<Message> Messages { get; } = new ObservableCollection<Message>();

        private readonly IRecordingService _recordingService;
        private readonly IApiService _apiService;
        private readonly ITextToSpeech _textToSpeech;
        private readonly IAudioPlayer _audioPlayer;

        public ICommand RecordCommand { get; }
        public ICommand SendCommand { get; }
        public ICommand PlayAudioCommand { get; }

        public MainPageViewModel(
            IRecordingService recordingService,
            IApiService apiService,
            ITextToSpeech textToSpeech,
            IAudioPlayer audioPlayer)
        {
            #region RecordCommand
            _recordingService = recordingService;
            RecordCommand = new RelayCommand(async () => await RecordAsync());
            #endregion

            #region SendCommand
            SendCommand = new RelayCommand(async () => await SendMessageAsync());
            _apiService = apiService;
            _textToSpeech = textToSpeech;
            #endregion

            #region PlayAudioCommand
            PlayAudioCommand = new RelayCommand<Message>(async (message) => await PlayAudioAsync(message));
            _audioPlayer = audioPlayer;
            #endregion
        }

        public async Task RecordAsync()
        {
            if (!IsRecording)
            {
                RecordingStatus = "Recording...";
                OperationHistory.Add("Starting recording...");
                _recordingFile = await _recordingService.CreateRecordingFileAsync("Recording.mp3");
                await _recordingService.StartRecordingAsync(_recordingFile);
                IsRecording = true;
            }
            else
            {
                RecordingStatus = "Idle";
                await _recordingService.StopRecordingAsync();
                var duration = await _recordingService.GetRecordingDurationAsync(_recordingFile);
                OperationHistory.Add($"You have recorded a {duration.TotalSeconds}s message at {_recordingFile.Path}");
                Messages.Add(new Message
                {
                    Sender = "[Me]",
                    Content = "",
                    Audio = _recordingFile
                });
                IsRecording = false;
                IsRecordingAvailable = true;
            }
        }

        public async Task SendMessageAsync()
        {
            OperationHistory.Add("Sending message...");

            try
            {
                IBuffer buffer = await FileIO.ReadBufferAsync(RecordingFile);
                string responseJson = await _apiService.SendRequestAsync(buffer, RecordingFile.Name);

                string[] messages = responseJson.Split(';');

                OperationHistory.Add($"Received Message: {responseJson}");

                Message tmpMessage = Messages.Last();
                Messages.Remove(tmpMessage);
                Messages.Add(new Message { Sender = tmpMessage.Sender, Audio = tmpMessage.Audio, Content = messages[0].Trim() });
                Messages.Add(new Message { Sender = "[UChat]", Content = messages[1].Trim() });

                IsRecordingAvailable = false;

                await _textToSpeech.Speak(messages[1].Trim());
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                OperationHistory.Add(ex.ToString());
            }
        }

        public async Task PlayAudioAsync(Message message)
        {
            if (message.Audio == null)
            {
                await _textToSpeech.Speak(message.Content);
            }
            else
            {
                if (!message.IsPlaying)
                {
                    await _audioPlayer.PlayAudioAsync(message.Audio);
                    message.IsPlaying = true;
                }
                else
                {
                    _audioPlayer.StopAudio();
                    message.IsPlaying = false;
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
