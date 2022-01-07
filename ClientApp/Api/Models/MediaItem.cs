using System.Text.Json.Serialization;

namespace MediaCast.Models;

public class MediaItem : ViewModelBase
{
    private bool _isExpanded = false;

    public string Name { get; set; }
    public string FullPath { get; set; }
    public bool IsFile { get; set; }

    [JsonIgnore]
    public bool IsExpanded
    {
        get => _isExpanded;
        set
        {
            SetField(ref _isExpanded, value);
        }
    }

    public MediaItem(string name, string fullPath, bool isFile)
    {
        Name = name;
        FullPath = fullPath;
        IsFile = isFile;
        IsExpanded = false;
    }

    public BulkObservableCollection<MediaItem> Items { get; } = new BulkObservableCollection<MediaItem>();

}
