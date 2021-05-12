using System;
using System.IO;
using System.Runtime.Versioning;
using ConsoleTools;

namespace LeaveMeAlone
{
    [SupportedOSPlatform("windows")]
    internal class MainMenu
    {
        LeaveMeAlone LeaveMeAlone;
        ConsoleMenu Menu;

        public MainMenu(string[] args)
        {
            LeaveMeAlone = args.Length > 0 ? new LeaveMeAlone(Path.GetFullPath(args[0]).ToString()) : new LeaveMeAlone();
            Menu = new ConsoleMenu(args, level: 0)
                .Add("Enable Friend Only", () => LeaveMeAlone.AddFirewallRules())
                .Add("Exit", ConsoleMenu.Close)
                .Configure((config) =>
                {
                    config.SelectedItemBackgroundColor = Console.ForegroundColor;
                    config.SelectedItemForegroundColor = Console.BackgroundColor;
                    config.ItemBackgroundColor = Console.BackgroundColor;
                    config.ItemForegroundColor = Console.ForegroundColor;
                    config.WriteHeaderAction = () => Console.WriteLine("Pick an option:");
                    config.WriteItemAction = item => Console.Write("[{0}] {1}", item.Index, item.Name);
                    config.Selector = ">> ";
                    config.FilterPrompt = "Filter: ";
                    config.ClearConsole = true;
                    config.EnableFilter = false;
                    config.ArgsPreselectedItemsKey = "--menu-select=";
                    config.ArgsPreselectedItemsValueSeparator = '.';
                    config.EnableWriteTitle = true;
                    config.Title = "Leave Me Alone Firewall menu";
                    config.WriteTitleAction = title => Console.WriteLine(title);
                    config.EnableBreadcrumb = false;
                    config.WriteBreadcrumbAction = titles => Console.WriteLine(string.Join(" > ", titles));
                });

            UpdateMenuEntries();
        }

        public void UpdateMenuEntries()
        {
            if (LeaveMeAlone.GetExistingRules().Count > 0)
            {
                Menu.Items[0].Name = "Disable Friend Only";
                Menu.Items[0].Action = () =>
                {
                    DisableFriendOnly();
                    WaitForEnter();
                };
            }
            else
            {
                Menu.Items[0].Name = "Enable Friend Only";
                Menu.Items[0].Action = () =>
                {
                    EnableFriendOnly();
                    WaitForEnter();
                };
            }
        }

        private void DisableFriendOnly()
        {
            LeaveMeAlone.ClearFWRules();
            UpdateMenuEntries();
        }

        private void EnableFriendOnly()
        {
            LeaveMeAlone.AddFirewallRules();
            UpdateMenuEntries();
        }

        private void WaitForEnter()
        {
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
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
}
