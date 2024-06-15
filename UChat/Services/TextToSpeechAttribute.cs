using System;

namespace UChat.Services
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TextToSpeechAttribute : Attribute
    {
        public string Tag { get; }

        public TextToSpeechAttribute(string tag)
        {
            Tag = tag;
        }
    }
}
