using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;

using LibVLCSharp.Shared;

namespace ClientApp
{
    public partial class MediaPlayerHost : UserControl
    {
        private LibVLC _libVLC;
        private RendererDiscoverer _rendererDiscoverer;
        private bool isSliderDragStarted = false;

        public Uri Source { get; set; }

        public event EventHandler ToggleFullScreen;

        public event EventHandler VideoEnded;

        public MediaPlayerHost()
        {
            InitializeComponent();

            videoView.Loaded += OnVideoViewLoaded;

            Unloaded += OnUnload;
        }

        private void OnVideoViewLoaded(object sender, RoutedEventArgs e)
        {
            _libVLC = new LibVLC(enableDebugLogs: true);

            videoView.MediaPlayer = new MediaPlayer(_libVLC) { EnableHardwareDecoding = true };
            videoView.MediaPlayer.Playing += OnMediaStartPlaying;
            videoView.MediaPlayer.TimeChanged += OnMediaPlayerTimeChanged;
            videoView.MediaPlayer.EncounteredError += MediaPlayerEncounteredError;
            videoView.MediaPlayer.EndReached += MediaPlayerEndReached;

            //VideoView.MediaPlayer.AddSlave(
            //    MediaSlaveType.Subtitle,
            //    @"E:\downloads\Mortal.Engines.2018.720p.WEBRip.x264.AC3-HUD\Mortal.Engines.2018.720p.WEBRip.x264.AC3-HUD.srt",
            //    true);

            //var media = new Media(_libVLC, "https://www.youtube.com/watch?v=vkPKA3ulaCg&ab_channel=ToToYotov", FromType.FromLocation);
            //await media.Parse(MediaParseOptions.ParseNetwork);
            //_parent.VideoView.MediaPlayer = _mediaPlayer;
            //_parent.VideoView.MediaPlayer.Mute = true;
            //_parent.VideoView.MediaPlayer.Play(media.SubItems.First());

            //var renderer = _libVLC.RendererList.FirstOrDefault(r => r.Name.Equals("microdns_renderer"));
            //_rendererDiscoverer = new RendererDiscoverer(_libVLC, renderer.Name);
            //_rendererDiscoverer.ItemAdded += RendererDiscoverer_ItemAdded;
            //var a = _rendererDiscoverer.Start();
        }

        private void MediaPlayerEndReached(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                if (VideoEnded != null)
                {
                    VideoEnded(this, e);
                }
            });
        }

        private void MediaPlayerEncounteredError(object sender, EventArgs e)
        {
            MessageBox.Show(_libVLC.LastLibVLCError);
        }

        private void OnMediaPlayerTimeChanged(object sender, MediaPlayerTimeChangedEventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                startLbl.Content = TimeSpan.FromMilliseconds(e.Time).ToString(@"hh\:mm\:ss");

                if (!isSliderDragStarted)
                {
                    slider.Value = e.Time;
                }
            });
        }

        private void OnMediaStartPlaying(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
             {
                 controlPanel.Visibility = Visibility.Visible;

                 endLbl.Content = TimeSpan.FromMilliseconds(videoView.MediaPlayer.Media.Duration).ToString(@"hh\:mm\:ss");

                 slider.Maximum = videoView.MediaPlayer.Media.Duration;
             });
        }

        public void Play()
        {
            if (Source != null)
            {
                // this._parent.VideoView.MediaPlayer.SetRenderer(items.First());

                if (videoView.MediaPlayer.IsPlaying)
                {
                    videoView.MediaPlayer.Stop();
                }

                if (!videoView.MediaPlayer.IsPlaying)
                {
                    using var media = new Media(_libVLC, Source);

                    media.AddOption(":subsdec-encoding=CP1251");

                    videoView.MediaPlayer.Play(media);

                    videoView.MediaPlayer.SetVideoTitleDisplay(Position.Top, 1000);

                    /// SPEED
                    /// videoView.MediaPlayer.SetRate(2);

                    videoView.MediaPlayer.Mute = true;
                }
            }
        }

        private void OnUnload(object sender, RoutedEventArgs e)
        {
            _libVLC.Dispose();

            videoView.Dispose();
        }

        private void StopVideo(object sender, RoutedEventArgs e)
        {
            if (videoView.MediaPlayer.IsPlaying)
            {
                videoView.MediaPlayer.Stop();
            }
        }

        private void PlayVideo(object sender, RoutedEventArgs e)
        {
            videoView.MediaPlayer.Play();
        }

        private void SliderValueChanged(object sender, DragCompletedEventArgs e)
        {
            isSliderDragStarted = false;

            videoView.MediaPlayer.Time = (long)slider.Value;
        }

        private void PauseVideo(object sender, RoutedEventArgs e)
        {
            videoView.MediaPlayer.Pause();
        }

        private void FullScreenVideo(object sender, RoutedEventArgs e)
        {
            if (ToggleFullScreen != null)
            {
                // controlPanel.Visibility = Visibility.Collapsed;

                ToggleFullScreen(this, EventArgs.Empty);
            }
        }

        private void OnSliderDragStarted(object sender, DragStartedEventArgs e)
        {
            isSliderDragStarted = true;
        }
    }
}
