# 7ow.CurrentPlayingMedia
Access information about the media that's currently playing on the Windows system.

### Usage

1. Initialize
```
PlayingMedia pm = new PlayingMedia();
await pm.InitializeAsync();
```

2. Attach to relevant events
```
    pm.SongChanged += Pm_SongChanged;
    pm.PlaybackStateChanged += Pm_PlaybackStateChanged;
```

3. Access the media information
```
        private async void Pm_SongChanged(object sender, PlayingMedia e)
        {
            // basic info
            string Title = e.Title;
            string Artist = e.Artist;
            string Album = e.Album;

            // playback info
            PlayingMedia.PlaybackStates PlaybackState = e.PlaybackStatus;

            // thumbnail (can be null)
            Image Thumbnail = await e.GetThumbnailAsync();
            // .. dont forget to dispose the image afterwards
            Thumbnail.Dispose();
        }
```


### Demo app

![image](https://github.com/user-attachments/assets/810d8b55-bfa7-491d-a9fb-69343cf915ea)
