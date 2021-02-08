using System.Windows;

using LibVLCSharp.Shared;

namespace ClientApp
{
    public partial class App : Application
    {
        public App()
        {
            Core.Initialize();
        }
    }
}
