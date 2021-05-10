using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Versioning;
using ConsoleTools;

namespace LeaveMeAlone
{
    public class MenuConfig
    {
        public ConsoleColor SelectedItemBackgroundColor = Console.ForegroundColor;
        public ConsoleColor SelectedItemForegroundColor = Console.BackgroundColor;
        public ConsoleColor ItemBackgroundColor = Console.BackgroundColor;
        public ConsoleColor ItemForegroundColor = Console.ForegroundColor;
        public Action WriteHeaderAction = () => Console.WriteLine("Pick an option:");
        public Action<MenuItem> WriteItemAction = item => Console.Write("[{0}] {1}", item.Index, item.Name);
        public string Selector = ">> ";
        public string FilterPrompt = "Filter: ";
        public bool ClearConsole = true;
        public bool EnableFilter = false;
        public string ArgsPreselectedItemsKey = "--menu-select=";
        public char ArgsPreselectedItemsValueSeparator = '.';
        public bool EnableWriteTitle = false;
        public string Title = "My menu";
        public Action<string> WriteTitleAction = title => Console.WriteLine(title);
        public bool EnableBreadcrumb = false;
        public Action<IReadOnlyList<string>> WriteBreadcrumbAction = titles => Console.WriteLine(string.Join(" > ", titles));
    }

    internal class MainMenu
    {
        LeaveMeAlone LeaveMeAlone;
        ConsoleMenu Menu;
        ConsoleMenu subMenu;

        public MainMenu(string[] args)
        {
            LeaveMeAlone = args.Length > 0 ? new LeaveMeAlone(Path.GetFullPath(args[0]).ToString()) : new LeaveMeAlone();
            subMenu = new ConsoleMenu(args, level: 1)
            .Add("Sub_One", () => { })
            .Add("Sub_Two", () => { })
            .Add("Sub_Three", () => { })
            .Add("Sub_Four", () => { })
            .Add("Sub_Close", ConsoleMenu.Close)
            .Configure(config =>
            {
                config.Selector = "--> ";
                config.EnableFilter = true;
                config.Title = "Submenu";
                config.EnableBreadcrumb = true;
                config.WriteBreadcrumbAction = titles => Console.WriteLine(string.Join(" / ", titles));
            });
            Menu = new ConsoleMenu(args, level: 0)
                .Add("Enable Friend Only", () => LeaveMeAlone.AddFirewallRules())
                .Add("Sub", subMenu.Show)
                .Add("Exit", ConsoleMenu.Close)
                .Configure(config => { 
                    config.Selector = ">>> ";
                    config.ClearConsole = true;
                });
        }

        public void Display()
        {
            Menu.Show();
        }

        public void Close()
        {
            Menu.CloseMenu();
        }
    }

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
