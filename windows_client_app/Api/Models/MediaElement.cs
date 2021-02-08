using System;
using System.Runtime.InteropServices;
using System.Security;

namespace ClientApp
{
    internal class MediaElement : IEquatable<MediaElement>, IComparable<MediaElement>
    {
        public string Name { get; set; }

        public string FullPath { get; set; }

        public bool IsFile { get; set; }

        public int CompareTo(MediaElement other)
        {
            return SafeNativeMethods.StrCmpLogicalW(this.Name, other.Name);
        }

        public bool Equals(MediaElement other)
        {
            return this.FullPath == other.FullPath;
        }

        [SuppressUnmanagedCodeSecurity]
        private static class SafeNativeMethods
        {
            [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
            public static extern int StrCmpLogicalW(string psz1, string psz2);
        }
    }
}