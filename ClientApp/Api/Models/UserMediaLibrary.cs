using System.Text.Json.Serialization;

namespace MediaCast.Models;

internal class UserMediaLibrary
{
    public static UserMediaLibrary Default = new UserMediaLibrary
    {
        PlayLists = new ObservableCollection<PlayList>
        {
            new PlayList
            {
                IsSelected = true,
                Name = "Default"
            }
        }
    };

    private ObservableCollection<PlayList> _playLists;

    [JsonIgnore]
    public PlayList SelectedPlayList { get; private set; }

    public Version Version { get; set; }

    [JsonInclude]
    public ObservableCollection<PlayList> PlayLists
    {
        get
        {
            return _playLists;
        }
        private set
        {
            _playLists = value == null || value.Count == 0 ? Default.PlayLists : value;

            SelectedPlayList = _playLists.First(x => x.IsSelected);
        }
    }

    public void AddMediaFilesToCurrentPlayList(IEnumerable<MediaItem> files)
    {
        if (files == null)
        {
            return;
        }

        SelectedPlayList.MediaItems.AddRange(files);
    }

    public void RemoveDeletedFiles() => PlayLists.ForEach(x => x.RemoveDeletedFiles());

    public void CreatePlayList(string playListName)
    {
        if (string.IsNullOrEmpty(playListName))
        {
            return;
        }

        SelectedPlayList = new PlayList { Name = playListName, IsSelected = true };

        PlayLists.ForEach(x => x.IsSelected = false);

        PlayLists.Add(SelectedPlayList);
    }
}