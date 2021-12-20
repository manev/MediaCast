using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;

using LibVLCSharp.Shared;

using MaterialDesignThemes.Wpf;

namespace ClientApp
{
    public partial class MediaPlayerHost : UserControl
    {
        private bool _isLoaded = false;
        private bool _isSliderDragStarted = false;
        private bool _isFullScreenMode = false;
        private bool _isShowAnimationStarted = false;
        private bool _isMouseOverControlPanel = false;

        private LibVLC _libVLC;
        private RendererDiscoverer _rendererDiscoverer;
        private DispatcherTimer _showControlPanelTimer = new DispatcherTimer();

        public Uri Source { get; set; }
        public event EventHandler ToggledFullScreen;
        public event EventHandler VideoEnded;

        public MediaPlayerHost()
        {
            InitializeComponent();

            Unloaded += OnUnload;

            Loaded += OnLoaded;
        }

        public void ToggleFullScreenMode()
        {
            _isFullScreenMode = !_isFullScreenMode;

            fullScreenIcon.Kind = _isFullScreenMode ? PackIconKind.FullscreenExit : PackIconKind.Fullscreen;

            videoView.Margin = _isFullScreenMode ? new Thickness(0, 0, 0, -controlPanel.ActualHeight - 10) : new Thickness(10);

            if (!_isFullScreenMode)
            {
                AnimateControlPanelVisibility("showControlPanelAnimation");
            }
        }

        public void Play()
        {
            if (Source != null)
            {
                if (videoView?.MediaPlayer?.IsPlaying == true)
                {
                    StopMedia();
                }

                using var media = new Media(_libVLC, Source);

                TryLoadSubtitlesFile();

                SetMarquee();

                media.AddOption(":subsdec-encoding=CP1251");

                videoView?.MediaPlayer?.Play(media);

                videoView?.MediaPlayer?.SetVideoTitleDisplay(Position.Top, 1000);

                videoView.MediaPlayer.Volume = 20;

                volumeSlider.Value = videoView.MediaPlayer.Volume;
            }
        }

        private void OnVolumeChanged(object sender, MouseWheelEventArgs e)
        {
            videoView.MediaPlayer.Volume += e.Delta > 0 ? 5 : -5;

            volumeSlider.Value = videoView.MediaPlayer.Volume;

            videoView.MediaPlayer.SetMarqueeString(VideoMarqueeOption.Text, videoView.MediaPlayer.Volume.ToString());
        }

        private void TryLoadSubtitlesFile()
        {
            var subFile = Source.LocalPath;

            subFile = subFile.Replace(Path.GetExtension(Source.LocalPath), ".srt");

            if (File.Exists(subFile))
            {
                videoView.MediaPlayer.AddSlave(MediaSlaveType.Subtitle, subFile, true);
            }
        }

        private void SetMarquee()
        {
            videoView.MediaPlayer.SetMarqueeInt(VideoMarqueeOption.Enable, 1);
            videoView.MediaPlayer.SetMarqueeInt(VideoMarqueeOption.Timeout, 5000);
            videoView.MediaPlayer.SetMarqueeInt(VideoMarqueeOption.X, 50);
            videoView.MediaPlayer.SetMarqueeInt(VideoMarqueeOption.Y, 50);
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (_isLoaded) return;

            _isLoaded = true;

            Core.Initialize();

            _libVLC = new LibVLC(enableDebugLogs: true, ":rmtosd-mouse-events", ":mouse-events", " :video-on-top", ":hotkeys-x-wheel-mode=0");
            _libVLC.Log += LibVLC_Log;

            videoView.MediaPlayer = new MediaPlayer(_libVLC) { EnableHardwareDecoding = true };
            videoView.MediaPlayer.Playing += OnMediaStartPlaying;
            videoView.MediaPlayer.TimeChanged += OnMediaPlayerTimeChanged;
            videoView.MediaPlayer.EncounteredError += MediaPlayerEncounteredError;
            videoView.MediaPlayer.EndReached += MediaPlayerEndReached;
            videoView.MediaPlayer.Paused += MediaPlayerPaused;
            videoView.MediaPlayer.Stopped += MediaPlayerStopped;

            //var media = new Media(_libVLC, new Uri("https://www.youtube.com/watch?v=aqz-KE-bpKQ"));
            //await media.Parse(MediaParseOptions.ParseNetwork);
            //videoView.MediaPlayer.Play(media.SubItems.First());

            //var renderer = _libVLC.RendererList.FirstOrDefault(r => r.Name.Equals("microdns_renderer"));
            //_rendererDiscoverer = new RendererDiscoverer(_libVLC, renderer.Name);
            //_rendererDiscoverer.ItemAdded += RendererDiscoverer_ItemAdded;
            //var a = _rendererDiscoverer.Start();
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

        private void ToggleFullScreen(object sender, RoutedEventArgs e) => ToggledFullScreen?.Invoke(this, e);

        private void OnSliderDragStarted(object sender, DragStartedEventArgs e) => _isSliderDragStarted = true;

        private void OnSpeedChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = speedCombobBox.SelectedItem as ComboBoxItem;

            if (selectedItem is not null && videoView.MediaPlayer is not null)
            {
                var value = float.Parse(selectedItem.DataContext.ToString());
                videoView.MediaPlayer.SetRate(value);
            }
        }

        private void StopMedia() => videoView.MediaPlayer.Stop();

        private void PauseMedia() => videoView.MediaPlayer.Pause();

        private void PlayMedia() => videoView.MediaPlayer.Play();

        private void SyncInUIThread(Action action) => Dispatcher.BeginInvoke(() => action?.Invoke());

        private void VideoClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                ToggledFullScreen?.Invoke(this, e);
            }
        }

        private void OnVolumeSliderChanged(object sender, RoutedPropertyChangedEventArgs<double> e) =>
            videoView.MediaPlayer.Volume = (int)volumeSlider.Value;

        private void AnimateControlPanelVisibility(string resourceAnimation)
        {
            var animation = this.FindResource(resourceAnimation) as Storyboard;
            Storyboard.SetTarget(animation, controlPanel);
            animation.Begin();
        }

        private void VideoOverlayMouseMove(object sender, MouseEventArgs e)
        {
            if (_isFullScreenMode && controlPanel.Margin != new Thickness() && !_isShowAnimationStarted)
            {
                _showControlPanelTimer.Tick -= DispathcerTimerTick;
                _showControlPanelTimer.IsEnabled = false;
                _showControlPanelTimer.Stop();

                _isShowAnimationStarted = true;

                videoView.Margin = new Thickness();

                AnimateControlPanelVisibility("showControlPanelAnimation");

                _showControlPanelTimer.Interval = TimeSpan.FromSeconds(3);
                _showControlPanelTimer.IsEnabled = true;

                _showControlPanelTimer.Tick += DispathcerTimerTick;
                _showControlPanelTimer.Start();
            }
        }

        private void DispathcerTimerTick(object sender, EventArgs e)
        {
            if (_isMouseOverControlPanel)
            {
                return;
            }

            var dispathcerTimer = sender as DispatcherTimer;
            dispathcerTimer.Tick -= DispathcerTimerTick;
            dispathcerTimer.Stop();

            if (_isFullScreenMode)
            {
                _isShowAnimationStarted = false;

                videoView.Margin = new Thickness(0, 0, 0, -controlPanel.ActualHeight);

                AnimateControlPanelVisibility("hideControlPanelAnimation");
            }
        }

        private void ControlPanelMouseEnter(object sender, MouseEventArgs e) => _isMouseOverControlPanel = true;

        private void ControlPanelMouseLeave(object sender, MouseEventArgs e) => _isMouseOverControlPanel = false;
    }
}
