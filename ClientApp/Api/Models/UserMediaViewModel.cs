namespace MediaCast.Models;

internal class UserMediaViewModel
{
    public ObservableCollection<string> MediaListNames { get; set; }

    public MediaList SelectedMediaList { get; set; }
}

internal class MediaList
{
    public string Name { get; set; }

    public BulkObservableCollection<MediaItem> MediaItems { get; set; }
}
