using System.Text.Json.Serialization;

namespace MediaCast.Models;

internal class PlayList
{
    public string Name { get; set; }

    public bool IsSelected { get; set; }

    [JsonInclude]
    public BulkObservableCollection<MediaItem> MediaItems { get; private set; } = new BulkObservableCollection<MediaItem>();

    public void RemoveDeletedFiles() => MediaItems.RemoveRange(MediaItems.Where(x => !File.Exists(x.FullPath)).ToList());
}