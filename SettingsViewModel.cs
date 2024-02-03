using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace UChat
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string ApiUrl
        {
            get => (string)ApplicationData.Current.LocalSettings.Values["ApiUrl"] ?? "https://174.34.106.19:10051/api/v2.0/dictation";
            set
            {
                ApplicationData.Current.LocalSettings.Values["ApiUrl"] = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ApiUrl)));
            }
        }

        public bool AcceptInsecureConnection
        {
            get => (bool?)ApplicationData.Current.LocalSettings.Values["AcceptInsecureConnection"] ?? true;

            set
            {
                ApplicationData.Current.LocalSettings.Values["AcceptInsecureConnection"] = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AcceptInsecureConnection)));
            }
        }

        public string HttpClientName => AcceptInsecureConnection ? "InsecureHttpClient" : "SecureHttpClient";

        public int Timeout
        {
            get => (int?)ApplicationData.Current.LocalSettings.Values["Timeout"] ?? 10;
            set
            {
                ApplicationData.Current.LocalSettings.Values["Timeout"] = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Timeout)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TimeoutDescription)));
            }
        }

        public string TimeoutDescription
        {
            get => $"Timeout Value: {Timeout}s";
        }

    }
}
