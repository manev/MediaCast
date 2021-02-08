using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Windows.Forms;

namespace ClientApp
{
    internal class MediaLibrary
    {
        private readonly string[] _mediaFormatTypes = new string[]
        {
            ".WEBM", ".MPG", ".MP2", ".MPEG", ".MPE", ".MPV", ".OGG", ".MP4", ".M4P", ".M4V", ".AVI", ".WMV",".MOV", ".MKV", ".MP3", ".MP4"
        };

        private readonly string _libFileName = "libs.json";

        private IEnumerable<MediaElement> _currenMedia;

        private string LibPath
        {
            get
            {
                return Path.Combine(Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName, _libFileName);
            }
        }

        public IEnumerable<MediaElement> CreateLibrary()
        {
            var path = string.Empty;

            if (TryGetUserSelectedFolder(ref path))
            {
                if (string.IsNullOrWhiteSpace(path))
                {
                    throw new ArgumentNullException(nameof(path));
                }

                _currenMedia = GetMediaElements(path);

                return _currenMedia;
            }

            return null;
        }

        internal void Save()
        {
            var content = JsonSerializer.Serialize(_currenMedia);

            File.WriteAllText(LibPath, content);
        }

        internal void DeleteCurrentLibrary()
        {
            _currenMedia = null;
        }

        internal IEnumerable<MediaElement> GetSavedLibrary()
        {
            if (File.Exists(LibPath))
            {
                var content = File.ReadAllText(LibPath);

                _currenMedia = JsonSerializer.Deserialize<IEnumerable<MediaElement>>(content);

                var list = _currenMedia.ToList();

                list.Sort();

                return list.AsEnumerable();
            }

            return Enumerable.Empty<MediaElement>();
        }

        public IEnumerable<MediaElement> LoadLibrary(string path)
        {
            return GetMediaElements(path);
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
                        .Select(x => new MediaElement { Name = x.Name, FullPath = x.FullName, IsFile = true })
                        .Union(folder.GetDirectories()
                            .Select(x => new MediaElement { Name = x.Name, FullPath = x.FullName, IsFile = false }))
                        .ToList();

            list.Sort();

            return list.AsEnumerable();
        }
    }
}
