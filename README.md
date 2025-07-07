### .NET Framework 4.7.2+
# 7ow.CurrentPlayingMedia
Access information about the media that's currently playing on the Windows system.

> ### Demo app
> 💾 Download @ [Releases/Demo](https://github.com/owtysm/CurrentPlayingMedia/releases/tag/demo)

# Showcase

- **Spotify**

![image](https://github.com/user-attachments/assets/810d8b55-bfa7-491d-a9fb-69343cf915ea)

- **SoundCloud**

![image](https://github.com/user-attachments/assets/0c206953-2866-419e-b6df-0add38090b0e)


# Usage

1. Initialize
```cs
PlayingMedia pm = new PlayingMedia();
await pm.InitializeAsync();
```

2. Attach to relevant events
```cs
    pm.SongChanged += Pm_SongChanged;
    pm.PlaybackStateChanged += Pm_PlaybackStateChanged;
```

3. Access the media information
```cs
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
