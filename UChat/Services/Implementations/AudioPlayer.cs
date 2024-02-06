using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UChat.Services.Interfaces;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;

namespace UChat.Services.Implementations
{
    public class AudioPlayer : IAudioPlayer
    {
        private MediaPlayer _mediaPlayer = new MediaPlayer();

        public async Task PlayAudioAsync(StorageFile storageFile)
        {
            var file = await StorageFile.GetFileFromPathAsync(storageFile.Path);
            var stream = await file.OpenAsync(FileAccessMode.Read);
            _mediaPlayer.Source = MediaSource.CreateFromStream(stream, file.ContentType);
            _mediaPlayer.Play();
        }

        public void StopAudio()
        {
            _mediaPlayer.Source = null;
            _mediaPlayer.PlaybackSession.Position = TimeSpan.Zero;
        }
    }
}
