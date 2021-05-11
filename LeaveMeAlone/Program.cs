using System.Runtime.Versioning;

namespace LeaveMeAlone
{
    [SupportedOSPlatform("windows")]
    class Program
    {
        static void Main(string[] args)
        {
            MainMenu menu = new MainMenu(args);
            menu.Display();
        }
    }
}
