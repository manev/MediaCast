using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Windows.Forms;

namespace ClientApp
{
    internal class MediaLibrary
    {
        private readonly string[] _mediaFormatTypes = new[]
        {
            ".WEBM", ".MPG", ".MP2", ".MPEG", ".MPE", ".MPV", ".OGG", ".MP4", ".M4P", ".M4V", ".AVI", ".WMV",".MOV", ".MKV", ".MP3", ".MP4"
        };

        private readonly string _libFileName = "libs.json";

        private ObservableCollection<MediaElement>? _currenMedia;

        private string LibPath => Path.Combine(Directory.GetParent(Assembly.GetExecutingAssembly().Location)?.FullName, _libFileName);

        public IEnumerable<MediaElement> CreateLibrary()
        {
            var path = string.Empty;

            if (TryGetUserSelectedFolder(ref path))
            {
                if (string.IsNullOrWhiteSpace(path))
                {
                    throw new ArgumentNullException(nameof(path));
                }

                _currenMedia = new ObservableCollection<MediaElement>(GetMediaElements(path));

                return _currenMedia;
            }

            return _currenMedia;
        }

        internal void Save()
        {
            var content = JsonSerializer.Serialize(_currenMedia);

            File.WriteAllText(LibPath, content);
        }

        internal void DeleteCurrentLibrary()
        {
            _currenMedia = new ObservableCollection<MediaElement>();
        }

        internal IEnumerable<MediaElement> GetSavedLibrary()
        {
            if (File.Exists(LibPath))
            {
                string? content = File.ReadAllText(LibPath);

                var list = string.IsNullOrWhiteSpace(content) ?
                    new List<MediaElement>() :
                    JsonSerializer.Deserialize<IEnumerable<MediaElement>>(content)?.ToList();

                list?.Sort();

                _currenMedia = new ObservableCollection<MediaElement>(list ?? new List<MediaElement>());

                return _currenMedia;
            }

            return Enumerable.Empty<MediaElement>();
        }

        public IEnumerable<MediaElement> LoadLibrary(string path) => GetMediaElements(path);

        public void Remove(MediaElement mediaElement)
        {
            if (mediaElement != null)
            {
                _currenMedia.Remove(mediaElement);
            }
        }

        public void HardRemove(MediaElement mediaElement)
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

        private bool TryGetUserSelectedFolder(ref string path)
        {
            using var dialog = new FolderBrowserDialog();

            DialogResult result = dialog.ShowDialog();

            path = dialog.SelectedPath;

            return result == DialogResult.OK;
        }

        private IEnumerable<MediaElement> GetMediaElements(string path)
        {
            var folder = new DirectoryInfo(path);

            if (!folder.Exists)
            {
                throw new Exception("Folder doesnt exist");
            }

            var list = folder.GetFiles()
                        .Where(x => _mediaFormatTypes.Contains(x.Extension.ToUpper()))
                        .Select(x => new MediaElement { Name = x.Name, FullPath = x.FullName.Trim(), IsFile = true })
                        .Union(folder.GetDirectories()
                                     .Select(x => new MediaElement { Name = x.Name, FullPath = x.FullName.Trim(), IsFile = false }))
                        .ToList();

            list.Sort();

            return list.AsEnumerable();
        }
    }
}
