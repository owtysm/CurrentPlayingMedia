using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using Windows.Media.Control;
using Windows.Storage.Streams;

namespace CurrentPlayingMedia
{
    public class PlayingMedia
    {
        public string Title { get; private set; }
        public string Artist { get; private set; }
        public string Album { get; private set; }

        public enum PlaybackStates
        {
            Changing,
            Closed,
            Opened,
            Paused,
            Playing,
            Stopped
        }

        private IRandomAccessStreamReference thumbnailRef;

        public event EventHandler<PlayingMedia> SongChanged;
        public event EventHandler<PlaybackStates> PlaybackStateChanged;

        public PlaybackStates PlaybackStatus { get; private set; }
        private PlaybackStates? lastPlaybackStatus = null;

        private GlobalSystemMediaTransportControlsSessionManager mediaManager;
        private GlobalSystemMediaTransportControlsSession currentSession;

        public PlayingMedia() { }

        public async Task InitializeAsync()
        {
            mediaManager = await GetMediaManagerAsync();
            mediaManager.CurrentSessionChanged += OnCurrentSessionChanged;

            await UpdateSessionAsync(mediaManager.GetCurrentSession());
        }

        private async void OnCurrentSessionChanged(GlobalSystemMediaTransportControlsSessionManager sender, CurrentSessionChangedEventArgs args)
        {
            await UpdateSessionAsync(sender.GetCurrentSession());
        }

        private async Task UpdateSessionAsync(GlobalSystemMediaTransportControlsSession session)
        {
            if (currentSession != null)
            {
                currentSession.MediaPropertiesChanged -= OnMediaPropertiesChanged;
                currentSession.PlaybackInfoChanged -= OnMediaPropertiesChanged;
            }

            currentSession = session;

            if (currentSession == null)
                return;

            currentSession.MediaPropertiesChanged += OnMediaPropertiesChanged;
            currentSession.PlaybackInfoChanged += OnMediaPropertiesChanged;

            await UpdateMediaPropertiesAsync();
        }

        private async void OnMediaPropertiesChanged(GlobalSystemMediaTransportControlsSession sender, object args)
        {
            await UpdateMediaPropertiesAsync();
        }

        private async Task UpdateMediaPropertiesAsync()
        {
            try
            {
                var mediaProperties = await GetMediaPropertiesAsync(currentSession);
                Title = mediaProperties.Title;
                Artist = mediaProperties.Artist;
                Album = mediaProperties.AlbumTitle;

                PlaybackStates newStatus = ParsePlaybackState(currentSession.GetPlaybackInfo().PlaybackStatus);
                if (lastPlaybackStatus != newStatus)
                {
                    lastPlaybackStatus = newStatus;
                    PlaybackStateChanged?.Invoke(this, newStatus);
                }
                PlaybackStatus = newStatus;

                thumbnailRef = mediaProperties.Thumbnail;

                SongChanged?.Invoke(this, this);
            }
            catch
            {
                // how did we get here
            }
        }

        private PlaybackStates ParsePlaybackState(GlobalSystemMediaTransportControlsSessionPlaybackStatus playbackStatus)
        {
            switch (playbackStatus)
            {
                case GlobalSystemMediaTransportControlsSessionPlaybackStatus.Closed:
                    return PlaybackStates.Closed;
                case GlobalSystemMediaTransportControlsSessionPlaybackStatus.Opened:
                    return PlaybackStates.Opened;
                case GlobalSystemMediaTransportControlsSessionPlaybackStatus.Stopped:
                    return PlaybackStates.Stopped;

                case GlobalSystemMediaTransportControlsSessionPlaybackStatus.Paused:
                    return PlaybackStates.Paused;
                case GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing:
                    return PlaybackStates.Playing;
                case GlobalSystemMediaTransportControlsSessionPlaybackStatus.Changing:
                    return PlaybackStates.Changing;
                default:
                    return PlaybackStates.Closed;
            }
        }

        public async Task<Bitmap> GetThumbnailAsync()
        {
            if (thumbnailRef == null)
                return null;

            var stream = await OpenReadAsync(thumbnailRef);
            var buffer = await ReadStreamToBufferAsync(stream);
            return new Bitmap(new MemoryStream(buffer));
        }

        #region Wrapper Helpers

        private static async Task<GlobalSystemMediaTransportControlsSessionManager> GetMediaManagerAsync()
        {
            var tcs = new TaskCompletionSource<GlobalSystemMediaTransportControlsSessionManager>();
            var op = GlobalSystemMediaTransportControlsSessionManager.RequestAsync();

            op.Completed = (asyncOp, status) =>
            {
                try { tcs.SetResult(asyncOp.GetResults()); }
                catch (Exception ex) { tcs.SetException(ex); }
            };

            return await tcs.Task;
        }

        private static async Task<GlobalSystemMediaTransportControlsSessionMediaProperties> GetMediaPropertiesAsync(GlobalSystemMediaTransportControlsSession session)
        {
            var tcs = new TaskCompletionSource<GlobalSystemMediaTransportControlsSessionMediaProperties>();
            var op = session.TryGetMediaPropertiesAsync();

            op.Completed = (asyncOp, status) =>
            {
                try { tcs.SetResult(asyncOp.GetResults()); }
                catch (Exception ex) { tcs.SetException(ex); }
            };

            return await tcs.Task;
        }

        private static async Task<IRandomAccessStreamWithContentType> OpenReadAsync(IRandomAccessStreamReference reference)
        {
            var tcs = new TaskCompletionSource<IRandomAccessStreamWithContentType>();
            var op = reference.OpenReadAsync();

            op.Completed = (asyncOp, status) =>
            {
                try { tcs.SetResult(asyncOp.GetResults()); }
                catch (Exception ex) { tcs.SetException(ex); }
            };

            return await tcs.Task;
        }

        private static Task<byte[]> ReadStreamToBufferAsync(IRandomAccessStream stream)
        {
            var tcs = new TaskCompletionSource<byte[]>();

            try
            {
                var reader = new DataReader(stream);
                var size = (uint)stream.Size;
                var loadOp = reader.LoadAsync(size);

                loadOp.Completed = (op, status) =>
                {
                    try
                    {
                        op.GetResults();
                        byte[] buffer = new byte[size];
                        reader.ReadBytes(buffer);
                        tcs.SetResult(buffer);
                    }
                    catch (Exception ex)
                    {
                        tcs.SetException(ex);
                    }
                };
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }

            return tcs.Task;
        }

        #endregion
    }
}
