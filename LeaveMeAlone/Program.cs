using System.Runtime.Versioning;

namespace LeaveMeAlone
{
    [SupportedOSPlatform("windows")]
    internal static class Program
    {
        private static void Main(string[] args)
        {
            MainMenu menu = new(args);
            menu.Display();
        }
    }
}
