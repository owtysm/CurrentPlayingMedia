using System;
using System.Threading.Tasks;
using Windows.Media.Control;

namespace CurrentPlayingMedia
{
    public class PlayingMedia
    {
        public string Title { get; private set; }
        public string Artist { get; private set; }
        public string Album { get; private set; }
        public GlobalSystemMediaTransportControlsSessionPlaybackStatus PlaybackStatus { get; private set; }

        // so that the uninformed can just use `new PlayingMedia()`
        // please dont kill me (please????)
        public PlayingMedia()
        {
            var pmedia = Get().Result;
            Title = pmedia.Title;
            Artist = pmedia.Artist;
            Album = pmedia.Album;
            PlaybackStatus = pmedia.PlaybackStatus;
        }


        // bullshit constructor for .Get() method
        internal PlayingMedia(object _)
        {

        }

        public static async Task<PlayingMedia> Get()
        {
            var instance = new PlayingMedia(null);

            var mediaManager = await GetMediaManagerAsync();
            var currentSession = mediaManager.GetCurrentSession();

            if (currentSession == null)
            {
                return null;
            }

            var mediaProperties = await GetMediaPropertiesAsync(currentSession);

            instance.Title = mediaProperties.Title;
            instance.Artist = mediaProperties.Artist;
            instance.Album = mediaProperties.AlbumTitle;
            instance.PlaybackStatus = currentSession.GetPlaybackInfo().PlaybackStatus;

            return instance;
        }

        private static Task<GlobalSystemMediaTransportControlsSessionManager> GetMediaManagerAsync()
        {
            var tcs = new TaskCompletionSource<GlobalSystemMediaTransportControlsSessionManager>();
            var operation = GlobalSystemMediaTransportControlsSessionManager.RequestAsync();

            operation.Completed = (asyncOp, status) =>
            {
                try
                {
                    tcs.SetResult(asyncOp.GetResults());
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            };

            return tcs.Task;
        }

        private static Task<GlobalSystemMediaTransportControlsSessionMediaProperties> GetMediaPropertiesAsync(GlobalSystemMediaTransportControlsSession session)
        {
            var tcs = new TaskCompletionSource<GlobalSystemMediaTransportControlsSessionMediaProperties>();
            var operation = session.TryGetMediaPropertiesAsync();

            operation.Completed = (asyncOp, status) =>
            {
                try
                {
                    tcs.SetResult(asyncOp.GetResults());
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            };

            return tcs.Task;
        }
    }
}
