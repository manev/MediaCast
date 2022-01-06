using ControlzEx.Theming;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace MediaCast;

public partial class MainWindow : MetroWindow
{
    private readonly MediaLibrary _mediaLib;
    private readonly WindowFullcreenManager _layoutScreenManager;
    private GridLength _mediaListColumntWidth;

    public MainWindow()
    {
        InitializeComponent();

        _mediaLib = new MediaLibrary();

        _layoutScreenManager = new WindowFullcreenManager(this);

        Closing += WindowClosing;

        Loaded += OnWindowLoaded;

        ThemeManager.Current.ThemeSyncMode = ThemeSyncMode.SyncWithAppMode;
        ThemeManager.Current.SyncTheme();

        DialogParticipation.SetRegister(this, _mediaLib);
    }

    private void OnWindowLoaded(object sender, RoutedEventArgs e) => DataContext = _mediaLib.LoadSavedPlayLists();

    private void WindowClosing(object sender, CancelEventArgs e) => _mediaLib.Save();

    private void ConnectChromeCast(object sender, RoutedEventArgs e)
    {
        mediaElement.LoadDevices();
    }

    private void OnVideoEnded(object sender, EventArgs e)
    {
        if (mediaList.Items.Count > mediaList.SelectedIndex + 1)
        {
            mediaList.SelectedItem = mediaList.Items[mediaList.SelectedIndex + 1];
        }
    }

    private void OnToggleFullScreenHandler(object sender, EventArgs e)
    {
        _layoutScreenManager.ToggleFullScreen();

        if (_layoutScreenManager.IsFullScreen)
        {
            CollapseMediaListView();
        }
        else
        {
            ExpandMediaListView();
        }
    }

    private void CollapseMediaListView()
    {
        if (mediaList.Visibility == Visibility.Collapsed)
        {
            return;
        }

        mediaList.Visibility = Visibility.Collapsed;
        gridSplitter.Visibility = Visibility.Collapsed;

        _mediaListColumntWidth = mediaListColumnt.Width;
        mediaListColumnt.Width = new GridLength(0);
    }

    private void ExpandMediaListView()
    {
        if (!IsLoaded)
        {
            return;
        }

        hamburgerExpander.IsChecked = true;

        gridSplitter.Visibility = Visibility.Visible;
        mediaList.Visibility = Visibility.Visible;
        mediaListColumnt.Width = _mediaListColumntWidth;
    }

    public void ToggleExpander()
    {
        if (hamburgerExpander.IsChecked == false)
        {
            CollapseMediaListView();
        }
        else
        {
            ExpandMediaListView();
        }
    }

    private void OnDelete(object sender, RoutedEventArgs e) => _mediaLib.Remove(mediaList.SelectedItem as MediaItem);

    private void OnHardDelete(object sender, RoutedEventArgs e) => _mediaLib.HardRemove(mediaList.SelectedItem as MediaItem);

    private void OnToggleExpanderChecker(object sender, RoutedEventArgs e) => ToggleExpander();
}