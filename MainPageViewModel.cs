using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace UChat
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        private string _recordingStatus = "Idle";

        private bool _isRecordingAvailable = false;

        public bool IsRecordingAvailable
        {
            get { return _isRecordingAvailable; }
            set
            {
                _isRecordingAvailable = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanSend));
            }
        }

        public bool CanSend
        {
            get { return !IsRecording && IsRecordingAvailable; }
        }

        public string RecordingStatus
        {
            get { return _recordingStatus; }
            set
            {
                _recordingStatus = value;
                OnPropertyChanged();
            }
        }

        public string RecordButtonText
        {
            get { return IsRecording ? "⏺ Stop Record" : "🎙Record"; }
        }

        private bool _isRecording = false;
        public bool IsRecording
        {
            get { return _isRecording; }
            set
            {
                _isRecording = value;
                OnPropertyChanged(nameof(IsRecording));
                OnPropertyChanged(nameof(RecordButtonText));
                OnPropertyChanged();
            }
        }

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

        public ObservableCollection<Message> Messages { get; } = new ObservableCollection<Message>();

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
