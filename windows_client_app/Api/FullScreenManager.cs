using System;
using System.Windows;
using System.Windows.Input;

namespace ClientApp
{
    internal class FullScreenManager
    {
        private readonly Window _targetWindow;
        private WindowStateCache _windowStateCache;
        private bool _isFullScreenMode = false;

        private readonly Action onExit;
        private readonly Action onEnter;
        public FullScreenManager(Window targetWindow, Action onEnter, Action onExit)
        {
            _targetWindow = targetWindow ?? throw new ArgumentNullException(nameof(targetWindow));

            this.onExit = onExit;

            this.onEnter = onEnter;

            targetWindow.KeyDown += OnKeyDown;
        }

        private void EnterFullScreen()
        {
            if (!_isFullScreenMode)
            {
                _windowStateCache = new WindowStateCache
                {
                    Height = Application.Current.MainWindow.Height,
                    Width = Application.Current.MainWindow.Width,
                    WindowState = Application.Current.MainWindow.WindowState,
                    WindowStyle = Application.Current.MainWindow.WindowStyle,
                    ResizeMode = Application.Current.MainWindow.ResizeMode
                };

                _targetWindow.WindowStyle = WindowStyle.None;
                _targetWindow.ResizeMode = ResizeMode.NoResize;
                _targetWindow.Width = SystemParameters.VirtualScreenWidth;
                _targetWindow.Height = SystemParameters.VirtualScreenHeight;
                _targetWindow.WindowState = WindowState.Maximized;

                _isFullScreenMode = true;

                onEnter?.Invoke();
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                ExitFullScreen();

                e.Handled = true;
            }
        }

        public void ExitFullScreen()
        {
            if (_isFullScreenMode)
            {
                Application.Current.MainWindow.WindowStyle = _windowStateCache.WindowStyle;
                Application.Current.MainWindow.WindowState = _windowStateCache.WindowState;
                Application.Current.MainWindow.ResizeMode = _windowStateCache.ResizeMode;
                Application.Current.MainWindow.Width = _windowStateCache.Width;
                Application.Current.MainWindow.Height = _windowStateCache.Height;

                _isFullScreenMode = false;

                onExit?.Invoke();
            }
        }

        internal void ToggleFullScreen()
        {
            if (_isFullScreenMode)
            {
                ExitFullScreen();
            }
            else
            {
                EnterFullScreen();
            }
        }

        internal void ToggleWindowState()
        {
            Application.Current.MainWindow.WindowState =
                Application.Current.MainWindow.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        private class WindowStateCache
        {
            public WindowStyle WindowStyle { get; set; }
            public WindowState WindowState { get; set; }
            public ResizeMode ResizeMode { get; set; }
            public double Width { get; set; }
            public double Height { get; set; }
        }
    }
}
