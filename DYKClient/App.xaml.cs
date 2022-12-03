using System.Runtime.InteropServices;
using System.Windows;

namespace DYKClient
{
    public partial class App : Application
    {
        [DllImport("Kernel32")]
        public static extern void AllocConsole();

        [DllImport("Kernel32")]
        public static extern void FreeConsole();

        private App()
        {
        }
    }
}
