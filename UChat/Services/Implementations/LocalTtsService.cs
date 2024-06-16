using System;
using System.Threading.Tasks;
using UChat.Services.Interfaces;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Media.SpeechSynthesis;

namespace UChat.Services.Implementations
{
    [TextToSpeech(tag: "LocalTTS")]
    public class LocalTtsService : ITextToSpeech
    {
        private MediaPlayer _mediaPlayer = new();

        public async Task Speak(string text)
        {
            // Use platform-specific TextToSpeech functionality here
            var synthesizer = new SpeechSynthesizer();
            var synthesisStream = await synthesizer.SynthesizeTextToStreamAsync(text);

            _mediaPlayer.Source = MediaSource.CreateFromStream(synthesisStream, synthesisStream.ContentType);
            _mediaPlayer.Play();
        }
    }
}
