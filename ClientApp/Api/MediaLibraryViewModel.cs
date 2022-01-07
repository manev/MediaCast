using MahApps.Metro.Controls.Dialogs;
using System.Windows.Forms;

namespace MediaCast;

internal class MediaLibraryViewModel : ViewModelBase
{
    private readonly string[] _mediaFormatTypes = new[]
    {
        ".WEBM", ".MPG", ".MP2", ".MPEG", ".MPE", ".MPV", ".OGG", ".MP4", ".M4P", ".M4V", ".AVI", ".WMV",".MOV", ".MKV", ".MP3", ".MP4", ".M3U", ".PLS"
    };

    private readonly string _libFileName = "PlayLists.json";
    private UserMediaLibrary _userMediaLibrary;
    private PlayList _selectedPlayList;
    private MediaItem _selectedMedia;

    private string LibPath => Path.Combine(Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName, _libFileName);

    public ICommand CreatePlayListCommand
    {
        get
        {
            return new ActionCommand(_ =>
            {
                var playListName = DialogCoordinator.Instance.ShowModalInputExternal(
                        this,
                       "Enter playlist name",
                       string.Empty,
                       new MetroDialogSettings
                       {
                           AnimateShow = true,
                           AnimateHide = true,
                           ColorScheme = MetroDialogColorScheme.Accented
                       });
                SelectedPlayList = _userMediaLibrary.CreatePlayList(playListName);
            });
        }
    }

    public ICommand AddFilesCommand
    {
        get
        {
            return new ActionCommand(_ => LoadUserSelectedFiles());
        }
    }

    public ICommand AddFoldersCommand
    {
        get
        {
            return new ActionCommand(_ => LoadUserSelectedFolder());
        }
    }

    public ICommand DeletePlayListMediaItems
    {
        get
        {
            return new ActionCommand(_ => DeleteCurrentLibrary());
        }
    }

    public ICommand SelectMediaItem
    {
        get
        {
            return new ActionCommand(item =>
            {
                var mediaItem = item as MediaItem;

                SelectedMediaItem = mediaItem;

                if (!mediaItem.IsFile)
                {
                    _selectedMedia.IsExpanded = !_selectedMedia.IsExpanded;

                    if (_selectedMedia.IsExpanded && !_selectedMedia.Items.Any())
                    {
                        var items = GetMediaItemsFromFolder(_selectedMedia.FullPath);

                        _selectedMedia.Items.AddRange(new ObservableCollection<MediaItem>(items));
                    }
                }
            });
        }
    }

    public MediaItem SelectedMediaItem
    {
        get => _selectedMedia;
        set => SetField(ref _selectedMedia, value);
    }

    public PlayList SelectedPlayList
    {
        get
        {
            return _selectedPlayList;
        }
        set
        {
            if (_selectedPlayList != value)
            {
                SetField(ref _selectedPlayList, value);

                MediaLibraries?.ForEach(library => library.IsSelected = false);

                if (_selectedPlayList != null)
                {
                    _selectedPlayList.IsSelected = true;
                }
            }
        }
    }

    public ObservableCollection<PlayList> MediaLibraries { get { return _userMediaLibrary.PlayLists; } }

    public MediaLibraryViewModel LoadSavedPlayLists()
    {
        _userMediaLibrary = GetSavedPlayLists();

        SelectedPlayList = _userMediaLibrary.SelectedPlayList;

        OnPropertyChanged(nameof(MediaLibraries), nameof(SelectedPlayList));

        return this;
    }

    public void LoadUserSelectedFiles()
    {
        var paths = GetUserSelectedFiles();

        if (paths.Any())
        {
            var files = GetMediaItemsFromFilePaths(paths);

            _userMediaLibrary.AddMediaFilesToCurrentPlayList(files);
        }
    }

    public void LoadUserSelectedFolder()
    {
        var path = TryGetUserFilesFromSelectedFolder();

        if (!string.IsNullOrWhiteSpace(path))
        {
            _userMediaLibrary.AddMediaFilesToCurrentPlayList(GetMediaItemsFromFolder(path));
        }
    }

    public void Save()
    {
        var content = JsonSerializer.Serialize(_userMediaLibrary);

        File.WriteAllText(LibPath, content);
    }

    public void DeleteCurrentLibrary() => _userMediaLibrary.SelectedPlayList.MediaItems.Clear();

    public void Remove(MediaItem mediaElement)
    {
        //if (mediaElement != null)
        //{
        //    _currentPlayLists.Remove(mediaElement);
        //}
    }

    public void HardRemove(MediaItem mediaElement)
    {
        if (mediaElement.IsFile && File.Exists(mediaElement.FullPath))
        {
            File.Delete(mediaElement.FullPath);
        }
        else if (Directory.Exists(mediaElement.FullPath))
        {
            Directory.Delete(mediaElement.FullPath, true);
        }

        Remove(mediaElement);
    }

    private IEnumerable<string> GetUserSelectedFiles()
    {
        using var dialog = new OpenFileDialog
        {
            CheckFileExists = true,
            CheckPathExists = true
        };

        DialogResult result = dialog.ShowDialog();

        return result == DialogResult.OK ? dialog.FileNames : Enumerable.Empty<string>();
    }

    private string TryGetUserFilesFromSelectedFolder()
    {
        using var dialog = new FolderBrowserDialog();

        DialogResult result = dialog.ShowDialog();

        return result == DialogResult.OK ? dialog.SelectedPath : string.Empty;
    }

    private IEnumerable<MediaItem> GetMediaItemsFromFilePaths(IEnumerable<string> paths) =>
        paths?.Select(x => new FileInfo(x))
              .Where(x => x.Exists && _mediaFormatTypes.Contains(x.Extension.ToUpper()))
              .Select(x => new MediaItem(x.Name, x.FullName.Trim(), true)) ?? Enumerable.Empty<MediaItem>();

    private IEnumerable<MediaItem> GetMediaItemsFromFolderPaths(IEnumerable<string> paths) =>
        paths?.Select(x => new DirectoryInfo(x))
              .Where(x => x.Exists)
              .Select(x => new MediaItem(x.Name, x.FullName.Trim(), false)) ?? Enumerable.Empty<MediaItem>();

    private UserMediaLibrary GetSavedPlayLists()
    {
        if (File.Exists(LibPath))
        {
            string content = File.ReadAllText(LibPath);

            var mediaLibrary = string.IsNullOrWhiteSpace(content) ?
                UserMediaLibrary.Default :
                JsonSerializer.Deserialize<UserMediaLibrary>(content) ?? UserMediaLibrary.Default;

            mediaLibrary.RemoveDeletedMediaItems();

            return mediaLibrary;
        }

        return UserMediaLibrary.Default;
    }

    private IEnumerable<MediaItem> GetMediaItemsFromFolder(string path)
    {
        var paths = Directory.EnumerateFiles(path);

        var mediaItems = GetMediaItemsFromFilePaths(paths);

        var folders = Directory.EnumerateDirectories(path);

        return mediaItems.Union(GetMediaItemsFromFolderPaths(folders));
    }
}
