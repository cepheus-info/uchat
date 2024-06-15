using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UChat.Services.Interfaces;

namespace UChat.Services
{
    public class TextToSpeechContext
    {
        private readonly ISettings _settings;
        private readonly Dictionary<string, ITextToSpeech> _services;

        public TextToSpeechContext(ISettings settings, IEnumerable<ITextToSpeech> textToSpeechServices)
        {
            _settings = settings;
            _services = textToSpeechServices.ToDictionary(
                service => service.GetType().GetCustomAttribute<TextToSpeechAttribute>().Tag,
                service => service
                );
        }

        public ITextToSpeech GetService()
        {
            var tag = _settings.TextToSpeechImplementation;
            return _services[tag];
        }
    }
}
