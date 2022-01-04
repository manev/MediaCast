using MahApps.Metro.Controls.Dialogs;
using System.Windows.Forms;

namespace MediaCast;

internal class MediaLibrary : ViewModelBase
{
    private readonly string[] _mediaFormatTypes = new[]
    {
        ".WEBM", ".MPG", ".MP2", ".MPEG", ".MPE", ".MPV", ".OGG", ".MP4", ".M4P", ".M4V", ".AVI", ".WMV",".MOV", ".MKV", ".MP3", ".MP4", ".M3U"
    };

    private readonly string _libFileName = "PlayLists.json";

    private UserMediaLibrary _userMediaLibrary;

    private PlayList _selectedPlayList;

    public event EventHandler CanExecuteChanged;

    private string LibPath => Path.Combine(Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName, _libFileName);

    public MediaLibrary LoadSavedPlayLists()
    {
        _userMediaLibrary = GetSavedPlayLists();

        SelectedPlayList = _userMediaLibrary.SelectedPlayList;

        OnPropertyChanged(nameof(MediaLibraries), nameof(SelectedPlayList));

        return this;
    }

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
                CreatePlaylist(playListName);
            });
        }
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

    public void LoadUserSelectedFiles()
    {
        var paths = GetUserSelectedFiles();

        if (paths.Any())
        {
            var files = GetMediaElements(paths);

            _userMediaLibrary.AddMediaFilesToCurrentPlayList(files);
        }
    }

    public void LoadUserSelectedFolder()
    {
        var path = TryGetUserFilesFromSelectedFolder();

        if (!string.IsNullOrWhiteSpace(path))
        {
            var paths = Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories);

            var files = GetMediaElements(paths);

            _userMediaLibrary.AddMediaFilesToCurrentPlayList(files);
        }
    }

    public ObservableCollection<PlayList> MediaLibraries { get { return _userMediaLibrary.PlayLists; } }

    public void Save()
    {
        var content = JsonSerializer.Serialize(_userMediaLibrary);

        File.WriteAllText(LibPath, content);
    }

    public void DeleteCurrentLibrary() => _userMediaLibrary.SelectedPlayList.MediaItems.Clear();

    public IEnumerable<MediaItem> LoadLibrary(string[] paths) => GetMediaElements(paths);

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

    public void CreatePlaylist(string playListName) => _userMediaLibrary.CreatePlayList(playListName);

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

    private IEnumerable<MediaItem> GetMediaElements(IEnumerable<string> paths) =>
        paths?.Select(x => new FileInfo(x))
              .Where(x => _mediaFormatTypes.Contains(x.Extension.ToUpper()))
              .Select(x => new MediaItem(x.Name, x.FullName.Trim(), true)) ?? Enumerable.Empty<MediaItem>();

    private UserMediaLibrary GetSavedPlayLists()
    {
        if (File.Exists(LibPath))
        {
            string content = File.ReadAllText(LibPath);

            var mediaLibrary = string.IsNullOrWhiteSpace(content) ?
                UserMediaLibrary.Default :
                JsonSerializer.Deserialize<UserMediaLibrary>(content) ?? UserMediaLibrary.Default;

            mediaLibrary.RemoveDeletedFiles();

            return mediaLibrary;
        }

        return UserMediaLibrary.Default;
    }
}

public class ActionCommand : ICommand
{
    public event EventHandler CanExecuteChanged;

    private readonly Action<object> _executeAction;

    public ActionCommand(Action<object> executeAction)
    {
        _executeAction = executeAction;
    }

    public bool CanExecute(object parameter)
    {
        return true;
    }

    public void Execute(object parameter)
    {
        _executeAction?.Invoke(parameter);
    }
}
