using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace ClientApp
{
    public partial class MainWindow : Window
    {
        private string _currentPath = string.Empty;

        private readonly MediaLibrary _mediaLib;
        private readonly FullScreenManager _fullScreenManager;
        private readonly Stack<string> _history = new Stack<string>();

        public MainWindow()
        {
            InitializeComponent();

            _mediaLib = new MediaLibrary();

            _fullScreenManager = new FullScreenManager(this, ExitFullScreen);

            Closing += WindowClosing;

            Loaded += OnWindowLoaded;
        }

        private void ExitFullScreen()
        {
            mediaList.Visibility = Visibility.Visible;
            toolbar.Visibility = Visibility.Visible;

            Grid.SetColumn(mediaPanel, 2);
            Grid.SetColumnSpan(mediaElement, 1);
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            LoadSavedLibrary();
        }

        private void WindowClosing(object sender, CancelEventArgs e)
        {
            _mediaLib.Save();
        }

        private void LoadSavedLibrary()
        {
            mediaList.ItemsSource = _mediaLib.GetSavedLibrary();
        }

        private void AddLibClickHandler(object sender, RoutedEventArgs e)
        {
            mediaList.ItemsSource = _mediaLib.CreateLibrary();
        }

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

        private void ConnectChromeCast(object sender, RoutedEventArgs e)
        {
            chromeCastList.Visibility = Visibility.Visible;
        }

        private void OnToggleFullScreenHandler(object sender, EventArgs e)
        {
            mediaList.Visibility = Visibility.Collapsed;
            toolbar.Visibility = Visibility.Collapsed;


            Grid.SetColumnSpan(mediaPanel, 3);
            Grid.SetColumn(mediaPanel, 0);

            _fullScreenManager.EnterFullScreen();
        }

        private void OnVideoEnded(object sender, EventArgs e)
        {
            if (mediaList.Items.Count > mediaList.SelectedIndex + 1)
            {
                mediaList.SelectedItem = mediaList.Items[mediaList.SelectedIndex + 1];
            }
        }
    }
}
