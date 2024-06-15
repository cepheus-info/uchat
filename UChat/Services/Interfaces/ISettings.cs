using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UChat.Services.Interfaces
{
    public interface ISettings
    {
        string ApiUrl { get; set; }

        bool AcceptInsecureConnection { get; set; }

        int Timeout { get; set; }

        string TextToSpeechImplementation { get; set; }

        string HttpClientName { get; }
    }
}
