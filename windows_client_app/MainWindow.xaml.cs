using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ControlzEx.Theming;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace ClientApp
{
    public partial class MainWindow : MetroWindow
    {
        private string _currentPath = string.Empty;

        private readonly MediaLibrary _mediaLib;
        private readonly FullScreenManager _fullScreenManager;
        private readonly Stack<string> _history = new Stack<string>();

        public MainWindow()
        {
            InitializeComponent();

            _mediaLib = new MediaLibrary();

            _fullScreenManager = new FullScreenManager(this, EnterFullScreen, ExitFullScreen);

            Closing += WindowClosing;

            Loaded += OnWindowLoaded;

            ThemeManager.Current.ThemeSyncMode = ThemeSyncMode.SyncWithAppMode;
            ThemeManager.Current.SyncTheme();

            Google.Apis.YouTube.v3.YouTubeService.Initializer a = new Google.Apis.Services.BaseClientService.Initializer();
            Google.Apis.YouTube.v3.YouTubeService b = new Google.Apis.YouTube.v3.YouTubeService();
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e) => LoadSavedLibrary();

        private void WindowClosing(object sender, CancelEventArgs e) => _mediaLib.Save();

        private void LoadSavedLibrary()
        {
            mediaList.ItemsSource = _mediaLib.GetSavedLibrary();
        }

        private void AddLibClickHandler(object sender, RoutedEventArgs e) => mediaList.ItemsSource = _mediaLib.CreateLibrary();

        private void DeleteCurrentLib(object sender, RoutedEventArgs e)
        {
            _mediaLib.DeleteCurrentLibrary();

            mediaList.ItemsSource = null;
        }

        private void OnMediaSelected(object sender, RoutedEventArgs e)
        {
            var selectedMedia = mediaList.SelectedItem as MediaElement;

            if (selectedMedia is null)
            {
                return;
            }

            if (selectedMedia.IsFile)
            {
                mediaElement.Source = new Uri(selectedMedia.FullPath);

                mediaElement.Play();
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(_currentPath))
                {
                    _history.Push(_currentPath);
                }

                _currentPath = selectedMedia.FullPath;

                mediaList.ItemsSource = _mediaLib.LoadLibrary(selectedMedia.FullPath);
            }
        }

        private void BackClickHandler(object sender, RoutedEventArgs e)
        {
            if (_history.Count == 0)
            {
                _currentPath = string.Empty;

                mediaList.ItemsSource = _mediaLib.GetSavedLibrary();
            }
            else
            {
                var path = _history.Pop();

                _currentPath = path;

                mediaList.ItemsSource = _mediaLib.LoadLibrary(path);
            }
        }

        private void ConnectChromeCast(object sender, RoutedEventArgs e) => chromeCastList.Visibility = Visibility.Visible;

        private void OnToggleFullScreenHandler(object sender, EventArgs e) => _fullScreenManager.ToggleFullScreen();

        private void ExitFullScreen()
        {
            mediaList.Visibility = Visibility.Visible;
            toolbar.Visibility = Visibility.Visible;

            Grid.SetColumnSpan(mediaElement, 1);

            mediaElement.ToggleFullScreenMode();
        }

        private void EnterFullScreen()
        {
            mediaList.Visibility = Visibility.Collapsed;
            toolbar.Visibility = Visibility.Collapsed;

            mediaElement.ToggleFullScreenMode();
        }

        private void OnVideoEnded(object sender, EventArgs e)
        {
            if (mediaList.Items.Count > mediaList.SelectedIndex + 1)
            {
                mediaList.SelectedItem = mediaList.Items[mediaList.SelectedIndex + 1];
            }
        }

        private void OnDelete(object sender, RoutedEventArgs e) => _mediaLib.Remove(mediaList.SelectedItem as MediaElement);

        private void OnHardDelete(object sender, RoutedEventArgs e) => _mediaLib.HardRemove(mediaList.SelectedItem as MediaElement);

        private void OnClose(object sender, RoutedEventArgs e) => Close();

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        private void OnDoubleClick(object sender, MouseButtonEventArgs e) => _fullScreenManager.ToggleWindowState();

        private void OnHamburgerButtonClick(object sender, RoutedEventArgs e)
        {
        }
    }
}
