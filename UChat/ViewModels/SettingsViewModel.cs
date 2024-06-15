using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UChat.Services.Interfaces;
using Windows.Storage;

namespace UChat.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private readonly ISettings _settings;

        public SettingsViewModel(ISettings settings)
        {
            _settings = settings;
        }

        public string ApiUrl
        {
            get => _settings.ApiUrl;
            set
            {
                _settings.ApiUrl = value;
                OnPropertyChanged(nameof(ApiUrl));
            }
        }

        public bool AcceptInsecureConnection
        {
            get => _settings.AcceptInsecureConnection;

            set
            {
                _settings.AcceptInsecureConnection = value;
                OnPropertyChanged(nameof(AcceptInsecureConnection));
            }
        }

        public string TextToSpeechImplementation
        {
            get => _settings.TextToSpeechImplementation;
            set
            {
                _settings.TextToSpeechImplementation = value;
                OnPropertyChanged(nameof(TextToSpeechImplementation));
            }
        }

        public int Timeout
        {
            get => _settings.Timeout;
            set
            {
                _settings.Timeout = value;
                OnPropertyChanged(nameof(Timeout));
                OnPropertyChanged(nameof(TimeoutDescription));
            }
        }

        public string TimeoutDescription
        {
            get => $"Timeout Value: {Timeout}s";
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
