using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace UChat.Models
{
    public class Message : INotifyPropertyChanged
    {
        public string Sender {  get; set; }
        public string Content { get; set; }

        public StorageFile Audio { get; set; }

        private bool isPlaying;
        public bool IsPlaying
        {
            get => isPlaying;
            set
            {
                if(isPlaying != value)
                {
                    isPlaying = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsPlayingButtonText));
                }
            }
        }

        public string IsPlayingButtonText
        {
            get => IsPlaying ? "⏹" : "🔊";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
