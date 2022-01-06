namespace MediaCast;

internal class WindowFullcreenManager
{
    private readonly MainWindow _targetWindow;
    private WindowStateCache _windowStateCache;

    public bool IsFullScreen
    {
        get
        {
            return _windowStateCache?.IsFullScreenMode == true;
        }
    }

    public WindowFullcreenManager(MainWindow targetWindow)
    {
        _windowStateCache = new();

        _targetWindow = targetWindow ?? throw new ArgumentNullException(nameof(targetWindow));
    }

    public void EnterFullScreen()
    {
        if (!_windowStateCache.IsFullScreenMode)
        {
            _windowStateCache = new WindowStateCache
            {
                Height = _targetWindow.Height,
                Width = _targetWindow.Width,
                WindowState = _targetWindow.WindowState,
                WindowStyle = _targetWindow.WindowStyle,
                ResizeMode = _targetWindow.ResizeMode
            };

            _targetWindow.WindowStyle = WindowStyle.None;
            _targetWindow.WindowState = WindowState.Maximized;
            _targetWindow.UseNoneWindowStyle = true;
            _targetWindow.ShowTitleBar = false;

            _windowStateCache.IsFullScreenMode = true;
        }
    }

    public void ExitFullScreen()
    {
        _targetWindow.WindowStyle = _windowStateCache.WindowStyle;
        _targetWindow.WindowState = _windowStateCache.WindowState;
        _targetWindow.UseNoneWindowStyle = false;
        _targetWindow.ShowTitleBar = true;
        _targetWindow.Width = _windowStateCache.Width;
        _targetWindow.Height = _windowStateCache.Height;

        _windowStateCache.IsFullScreenMode = false;
    }

    public void ToggleFullScreen()
    {
        if (_windowStateCache?.IsFullScreenMode == true)
        {
            ExitFullScreen();
        }
        else
        {
            EnterFullScreen();
        }
    }

    private class WindowStateCache
    {
        public WindowStyle WindowStyle { get; set; }
        public WindowState WindowState { get; set; }
        public ResizeMode ResizeMode { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public bool IsFullScreenMode { get; set; }
    }
}
