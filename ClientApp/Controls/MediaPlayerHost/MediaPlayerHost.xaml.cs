using LibVLCSharp.Shared;

using MaterialDesignThemes.Wpf;
using System.Text.RegularExpressions;

namespace MediaCast;

public partial class MediaPlayerHost : UserControl
{
    private static string url_regex = @"https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)";
    private static string[] radio_format_extensions = new[] { ".m3u", ".pls" };

    private bool _isLoaded = false;
    private bool _isSliderDragStarted = false;
    private bool _isFullScreenMode = false;
    private Uri _source;
    private LibVLC _libVLC;
    private RendererDiscoverer _rendererDiscoverer;

    public event EventHandler ToggledFullScreen;
    public event EventHandler VideoEnded;

    public MediaItem MediaItem
    {
        get { return (MediaItem)GetValue(MediaItemProperty); }
        set { SetValue(MediaItemProperty, value); }
    }

    public static readonly DependencyProperty MediaItemProperty =
        DependencyProperty.Register(
            "MediaItem",
            typeof(MediaItem),
            typeof(MediaPlayerHost),
            new PropertyMetadata(new PropertyChangedCallback(OnMediaItemChanged)));

    public MediaPlayerHost()
    {
        InitializeComponent();

        Unloaded += OnUnload;

        Loaded += OnLoaded;

        Core.Initialize();

        _libVLC = new LibVLC(enableDebugLogs: true, ":rmtosd-mouse-events", ":mouse-events", ":video-on-top", ":hotkeys-x-wheel-mode=0");
        _libVLC.Log += LibVLC_Log;
    }

    public void Play()
    {
        if (_source != null)
        {
            if (videoView?.MediaPlayer?.IsPlaying == true)
            {
                StopMedia();
            }

            using var media = new Media(_libVLC, _source);

            TryLoadSubtitlesFile();

            SetMarquee();

            media.AddOption(":subsdec-encoding=CP1251");

            videoView?.MediaPlayer?.Play(media);

            videoView?.MediaPlayer?.SetVideoTitleDisplay(Position.Top, 1000);

            videoView.MediaPlayer.Volume = 20;

            volumeSlider.Value = videoView.MediaPlayer.Volume;
        }
    }

    private static void OnMediaItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var playerHost = d as MediaPlayerHost;

        var mediaItem = e.NewValue as MediaItem;

        if (mediaItem != null)
        {
            if (mediaItem.IsFile)
            {
                playerHost._source = GetUrlFromMediaItem((MediaItem)e.NewValue);

                playerHost.Play();
            }
        }
        else
        {
            playerHost._source = null;

            playerHost.StopMedia();
        }
    }

    private static Uri GetUrlFromMediaItem(MediaItem mediaItem)
    {
        var path = mediaItem.FullPath;

        if (IsRadioFormat(Path.GetExtension(path).ToLower()))
        {
            var url = File.ReadAllLines(path)
                          .Select(line => Regex.Match(line, url_regex)?.Value)
                          .FirstNotNullOrDefault();

            return url != null ? new Uri(url) : new Uri(path);
        }

        return new Uri(path);
    }

    private void OnVolumeChanged(object sender, MouseWheelEventArgs e)
    {
        videoView.MediaPlayer.Volume += e.Delta > 0 ? -1 : 1;

        volumeSlider.Value = videoView.MediaPlayer.Volume;

        videoView.MediaPlayer.SetMarqueeString(VideoMarqueeOption.Text, videoView.MediaPlayer.Volume.ToString());
    }

    private void TryLoadSubtitlesFile()
    {
        var subFile = _source.LocalPath;

        var pathExtension = Path.GetExtension(_source.LocalPath);

        if (!string.IsNullOrWhiteSpace(pathExtension))
        {
            subFile = subFile.Replace(pathExtension, ".srt");

            if (File.Exists(subFile))
            {
                videoView.MediaPlayer.AddSlave(MediaSlaveType.Subtitle, subFile, true);
            }
        }
    }

    private void SetMarquee()
    {
        videoView.MediaPlayer.SetMarqueeInt(VideoMarqueeOption.Enable, 1);
        videoView.MediaPlayer.SetMarqueeInt(VideoMarqueeOption.Timeout, 5000);
        videoView.MediaPlayer.SetMarqueeInt(VideoMarqueeOption.X, 50);
        videoView.MediaPlayer.SetMarqueeInt(VideoMarqueeOption.Y, 50);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (_isLoaded) return;

        _isLoaded = true;

        videoView.MediaPlayer = new MediaPlayer(_libVLC) { EnableHardwareDecoding = true };
        videoView.MediaPlayer.Playing += OnMediaStartPlaying;
        videoView.MediaPlayer.TimeChanged += OnMediaPlayerTimeChanged;
        videoView.MediaPlayer.EncounteredError += MediaPlayerEncounteredError;
        videoView.MediaPlayer.EndReached += MediaPlayerEndReached;
        videoView.MediaPlayer.Paused += MediaPlayerPaused;
        videoView.MediaPlayer.Stopped += MediaPlayerStopped;
        videoView.MediaPlayer.EnableKeyInput = false;
        videoView.MediaPlayer.EnableMouseInput = false;

        //var media = new Media(_libVLC, new Uri("https://www.youtube.com/watch?v=aqz-KE-bpKQ"));
        //await media.Parse(MediaParseOptions.ParseNetwork);
        //videoView.MediaPlayer.Play(media.SubItems.First());

        //var renderer = _libVLC.RendererList.FirstOrDefault(r => r.Name.Equals("microdns_renderer"));
        //_rendererDiscoverer = new RendererDiscoverer(_libVLC, renderer.Name);
        //_rendererDiscoverer.ItemAdded += RendererDiscoverer_ItemAdded;
        //var a = _rendererDiscoverer.Start();

        foreach (var item in Application.Current.Windows)
        {
            (item as Window).KeyDown += OnKeyDown;
        }

    }

    internal void LoadDevices()
    {
        foreach (var item in _libVLC.RendererList)
        {

        }
    }

    private void LibVLC_Log(object sender, LogEventArgs e)
    {
        var a = e.Message;
    }

    private void MediaPlayerStopped(object sender, EventArgs e) => SyncInUIThread(() => playPauseIcon.Kind = PackIconKind.Play);

    private void MediaPlayerPaused(object sender, EventArgs e) => SyncInUIThread(() => playPauseIcon.Kind = PackIconKind.Play);

    private void MediaPlayerEndReached(object sender, EventArgs e) => SyncInUIThread(() => VideoEnded?.Invoke(this, e));

    private void MediaPlayerEncounteredError(object sender, EventArgs e) => SyncInUIThread(() => MessageBox.Show(_libVLC.LastLibVLCError));

    private void OnMediaPlayerTimeChanged(object sender, MediaPlayerTimeChangedEventArgs e) => SyncInUIThread(() =>
    {
        startLbl.Content = TimeSpan.FromMilliseconds(e.Time).ToString(@"hh\:mm\:ss");

        slider.Value = _isSliderDragStarted ? slider.Value : e.Time;
    });

    private void OnMediaStartPlaying(object sender, EventArgs e) => SyncInUIThread(() =>
    {
        controlPanel.Visibility = Visibility.Visible;

        endLbl.Content = TimeSpan.FromMilliseconds(videoView.MediaPlayer.Media.Duration).ToString(@"hh\:mm\:ss");

        slider.Maximum = videoView.MediaPlayer.Media.Duration;

        playPauseIcon.Kind = PackIconKind.Pause;
    });

    private void OnUnload(object sender, RoutedEventArgs e)
    {
        _libVLC.Dispose();

        videoView.Dispose();
    }

    private void TogglePlayVideo(object sender, RoutedEventArgs e) => TogglePlayVideo();

    private void OnSliderDragStarted(object sender, DragStartedEventArgs e) => _isSliderDragStarted = true;

    private void StopMedia() => videoView.MediaPlayer.Stop();

    private void PauseMedia() => videoView.MediaPlayer.Pause();

    private void PlayMedia() => videoView.MediaPlayer.Play();

    private void SyncInUIThread(Action action) => Dispatcher.BeginInvoke(() => action?.Invoke());

    private void OnVolumeSliderChanged(object sender, RoutedPropertyChangedEventArgs<double> e) => videoView.MediaPlayer.Volume = (int)volumeSlider.Value;

    private void VideoClick(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 1 && _isFullScreenMode)
        {
            HideControlPanel();

            overlay.Cursor = Cursors.None;
        }
        else if (e.ClickCount == 2)
        {
            ToggleFullScreen(sender, e);
        }
    }

    private void TogglePlayVideo()
    {
        if (videoView.MediaPlayer.IsPlaying)
        {
            PauseMedia();
        }
        else
        {
            PlayMedia();
        }
    }

    private void SliderValueChanged(object sender, DragCompletedEventArgs e)
    {
        _isSliderDragStarted = false;

        videoView.MediaPlayer.Time = (long)slider.Value;
    }

    private void ToggleFullScreen(object sender, RoutedEventArgs e)
    {
        _isFullScreenMode = !_isFullScreenMode;

        if (_isFullScreenMode)
        {
            HideControlPanel();
        }
        else
        {
            overlay.Cursor = null;

            ShowControlPanel();
        }

        fullScreenIcon.Kind = _isFullScreenMode ? PackIconKind.FullscreenExit : PackIconKind.Fullscreen;

        ToggledFullScreen?.Invoke(this, e);
    }

    private void OnSpeedChanged(object sender, SelectionChangedEventArgs e)
    {
        var selectedItem = speedCombobBox.SelectedItem as ComboBoxItem;

        if (selectedItem is not null && videoView.MediaPlayer is not null)
        {
            var value = float.Parse(selectedItem.DataContext.ToString());

            videoView.MediaPlayer.SetRate(value);
        }
    }

    private void VideoOverlayMouseMove(object sender, MouseEventArgs e)
    {
        if (_isFullScreenMode)
        {
            overlay.Cursor = null;

            ShowControlPanel();
        }
    }

    private void HideControlPanel() => controlPanel.Visibility = Visibility.Collapsed;

    private void ShowControlPanel() => controlPanel.Visibility = Visibility.Visible;

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Space)
        {
            videoView.MediaPlayer.Pause();
        }
        else if (e.Key == Key.Escape && _isFullScreenMode)
        {
            ToggleFullScreen(sender, e);
        }
    }

    private static bool IsRadioFormat(string path)
    {
        return radio_format_extensions.Contains(Path.GetExtension(path).ToLower());
    }
}