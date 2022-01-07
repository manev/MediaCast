using System.Text.Json.Serialization;

namespace MediaCast.Models;

internal class PlayList
{
    public string Name { get; set; }

    public bool IsSelected { get; set; }

    [JsonInclude]
    public BulkObservableCollection<MediaItem> MediaItems { get; private set; } = new BulkObservableCollection<MediaItem>();

    public void RemoveDeletedMediaItems() =>
        MediaItems.RemoveRange(
            MediaItems.Where(x => (x.IsFile && !File.Exists(x.FullPath)) || (!x.IsFile && !Directory.Exists(x.FullPath)))
                      .ToList());
}