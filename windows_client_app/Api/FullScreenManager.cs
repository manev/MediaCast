using System;
using System.Windows;
using System.Windows.Input;

namespace ClientApp
{
    internal class FullScreenManager
    {
        private readonly Window _targetWindow;
        private readonly Action onExit;
        private WindowStateCache _windowStateCache;

        public FullScreenManager(Window targetWindow, Action onExit)
        {
            _targetWindow = targetWindow ?? throw new ArgumentNullException(nameof(targetWindow));

            this.onExit = onExit;
            targetWindow.KeyDown += OnKeyDown;
        }

        public void EnterFullScreen()
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
            _targetWindow.WindowState = WindowState.Maximized;
            _targetWindow.ResizeMode = ResizeMode.NoResize;
            _targetWindow.Width = SystemParameters.VirtualScreenWidth;
            _targetWindow.Height = SystemParameters.VirtualScreenHeight;
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                ExitFullScreen();

                onExit?.Invoke();
            }
        }

        private void ExitFullScreen()
        {
            Application.Current.MainWindow.WindowStyle = _windowStateCache.WindowStyle;
            Application.Current.MainWindow.WindowState = _windowStateCache.WindowState;
            Application.Current.MainWindow.ResizeMode = _windowStateCache.ResizeMode;
            Application.Current.MainWindow.Width = _windowStateCache.Width;
            Application.Current.MainWindow.Height = _windowStateCache.Height;
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
