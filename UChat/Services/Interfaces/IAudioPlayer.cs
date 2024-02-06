using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace UChat.Services.Interfaces
{
    public interface IAudioPlayer
    {
        Task PlayAudioAsync(StorageFile file);

        void StopAudio();
    }
}
