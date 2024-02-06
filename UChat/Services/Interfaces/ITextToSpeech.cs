using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UChat.Services.Interfaces
{
    public interface ITextToSpeech
    {
        Task Speak(string text);
    }
}
