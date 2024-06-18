using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UChat.Services.Interfaces;
using Windows.Storage;

namespace UChat.Services.Implementations
{
    public class LocalSettings : ISettings
    {
        public string ApiUrl
        {
            get => (string)ApplicationData.Current.LocalSettings.Values["ApiUrl"] ?? "https://174.34.106.19:10051/api/v2.0/dictation";
            set => ApplicationData.Current.LocalSettings.Values["ApiUrl"] = value;
        }

        public bool AcceptInsecureConnection
        {
            get => (bool?)ApplicationData.Current.LocalSettings.Values["AcceptInsecureConnection"] ?? true;
            set => ApplicationData.Current.LocalSettings.Values["AcceptInsecureConnection"] = value;
        }

        public int Timeout
        {
            get => (int?)ApplicationData.Current.LocalSettings.Values["Timeout"] ?? 10;
            set => ApplicationData.Current.LocalSettings.Values["Timeout"] = value;
        }

        public string TextToSpeechImplementation
        {
            get => (string)ApplicationData.Current.LocalSettings.Values["TextToSpeechImplementation"] ?? "LocalTTS";
            set => ApplicationData.Current.LocalSettings.Values["TextToSpeechImplementation"] = value;
        }

        public string HttpClientName { get => AcceptInsecureConnection ? "InsecureHttpClient" : "SecureHttpClient"; }

        public bool IsDebugMode
        {
            get => (bool?)ApplicationData.Current.LocalSettings.Values["IsDebugMode"] ?? false;
            set => ApplicationData.Current.LocalSettings.Values["IsDebugMode"] = value;
        }
    }
}
