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

        private TaskCompletionSource<bool> _playTaskCompletionSource;

        /// <summary>
        /// Plays the audio file and finisheds the task when the audio is finished playing.
        /// </summary>
        /// <param name="storageFile">StorageFile being played</param>
        /// <returns>awaitable task when playing finished</returns>
        public async Task PlayAudioAsync(StorageFile storageFile)
        {
            _playTaskCompletionSource = new TaskCompletionSource<bool>();
            _mediaPlayer.MediaEnded += OnMediaPlayerMediaEnded;

            var file = await StorageFile.GetFileFromPathAsync(storageFile.Path);
            var stream = await file.OpenAsync(FileAccessMode.Read);
            _mediaPlayer.Source = MediaSource.CreateFromStream(stream, file.ContentType);
            _mediaPlayer.Play();

            await _playTaskCompletionSource.Task;
        }

        private void OnMediaPlayerMediaEnded(MediaPlayer sender, object args)
        {
            _mediaPlayer.MediaEnded -= OnMediaPlayerMediaEnded;
            _playTaskCompletionSource.SetResult(true);
        }

        /// <summary>
        /// Stop playing the audio file
        /// </summary>
        public void StopAudio()
        {
            _mediaPlayer.Source = null;
            _mediaPlayer.PlaybackSession.Position = TimeSpan.Zero;
            if (_playTaskCompletionSource != null && !_playTaskCompletionSource.Task.IsCompleted)
            {
                _mediaPlayer.MediaEnded -= OnMediaPlayerMediaEnded;
                _playTaskCompletionSource.SetResult(false);
            }
        }
    }
}
