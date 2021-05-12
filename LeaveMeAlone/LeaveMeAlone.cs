using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Versioning;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using NetFwTypeLib;

namespace LeaveMeAlone
{
    [SupportedOSPlatform("windows")]
    public class LeaveMeAlone : WinFWChanger
    {
        private List<string> SteamGameDirs { get; } = new List<string>();
        private string GTA5ExePath { get; set; } = string.Empty;
        private string GuardianJSONPath { get; set; } = string.Empty;

        public LeaveMeAlone()
        {
            ErrorCode = EErrorCode.OK;

            if (!IsElevated)
            {
                ErrorCode = EErrorCode.REQUIRES_ADMIN_RIGHTS;
                ShowMessageAndExit(ErrorCode);
            }
        }

        public LeaveMeAlone(string gta5ExePath)
        {
            ErrorCode = EErrorCode.OK;

            if (!IsElevated)
            {
                ErrorCode = EErrorCode.REQUIRES_ADMIN_RIGHTS;
                ShowMessageAndExit(ErrorCode);
            }

            GTA5ExePath = gta5ExePath;
        }

        public override void AddFirewallRules()
        {
            if (string.IsNullOrEmpty(GTA5ExePath))
                SearchGTA5InSteamApps();
            else
            {
                base.CheckFileExists(GTA5ExePath);
            }

            FindGuardianInstallation();
            BuildIPWhitelist();
            UpdateFWRules();
        }

        protected override void CheckFileExists(string filePath)
        {
            if (File.Exists(filePath))
            {
                string name = "";
                if (filePath.Contains("GTA5.exe"))
                    name = "GTA5 Executable";
                else if (filePath.Contains("Guardian"))
                    name = "Guardian JSON File";

                Console.WriteLine($"{name} found at: \"{filePath}\"");
            }
            else
            {
                if (filePath.Contains("GTA5.exe"))
                    ErrorCode = EErrorCode.GTA5_PATH_INVALID;
                else if (filePath.Contains("Guardian"))
                    ErrorCode = EErrorCode.GUARDIAN_PATH_INVALID;

                Console.WriteLine($"{Path.GetFileName(filePath)} not found");

                ShowMessageAndExit(ErrorCode);
            }
        }

        private void SearchGTA5InSteamApps()
        {
            try
            {
                Console.WriteLine($"Scanning all Steam game install directories for the GTA5.exe");

                SteamGameDirs.Clear();
                string steam32 = "SOFTWARE\\VALVE\\";
                string steam64 = "SOFTWARE\\Wow6432Node\\Valve\\";
                string steam32path;
                string steam64path;
                string config32path;
                string config64path;
                RegistryKey key32 = Registry.LocalMachine.OpenSubKey(steam32);
                RegistryKey key64 = Registry.LocalMachine.OpenSubKey(steam64);

                if (key64.ToString() == null || key64.ToString() == "")
                {
                    foreach (string k32subKey in key32.GetSubKeyNames())
                    {
                        using RegistryKey subKey = key32.OpenSubKey(k32subKey);
                        steam32path = subKey.GetValue("InstallPath").ToString();
                        config32path = steam32path + "/steamapps/libraryfolders.vdf";
                        string driveRegex = @"[A-Z]:\\";
                        if (File.Exists(config32path))
                        {
                            string[] configLines = File.ReadAllLines(config32path);
                            foreach (string item in configLines)
                            {
                                Match match = Regex.Match(item, driveRegex);
                                if (item != string.Empty && match.Success)
                                {
                                    string matched = match.ToString();
                                    string item2 = item.Substring(item.IndexOf(matched));
                                    item2 = item2.Replace("\\\\", "\\");
                                    item2 = item2.Replace("\"", "\\steamapps\\common\\");
                                    SteamGameDirs.Add(item2);
                                }
                            }
                            SteamGameDirs.Add(steam32path + "\\steamapps\\common\\");
                        }
                    }
                }
                else
                {
                    foreach (string k64subKey in key64.GetSubKeyNames())
                    {
                        using (RegistryKey subKey = key64.OpenSubKey(k64subKey))
                        {
                            steam64path = (string)subKey.GetValue("InstallPath");
                            config64path = steam64path + "/steamapps/libraryfolders.vdf";
                            string driveRegex = @"[A-Z]:\\";
                            if (File.Exists(config64path))
                            {
                                string[] configLines = File.ReadAllLines(config64path);
                                foreach (string item in configLines)
                                {
                                    Match match = Regex.Match(item, driveRegex);
                                    if (item != string.Empty && match.Success)
                                    {
                                        string matched = match.ToString();
                                        string item2 = item.Substring(item.IndexOf(matched));
                                        item2 = item2.Replace("\\\\", "\\");
                                        item2 = item2.Replace("\"", "\\steamapps\\common\\");
                                        SteamGameDirs.Add(item2);
                                    }
                                }
                                SteamGameDirs.Add(steam64path + "\\steamapps\\common\\");
                            }
                        }
                    }
                }

                foreach (string s in SteamGameDirs)
                {
                    if (Directory.Exists($"{s}Grand Theft Auto V"))
                    {
                        GTA5ExePath = $"{s}Grand Theft Auto V\\GTA5.exe";
                        break;
                    }
                }

                CheckFileExists(GTA5ExePath);
            }
            catch (Exception e)
            {
                ErrorCode = EErrorCode.FAILED_TO_LOOKUP_REGISTRY_KEY;
                ShowMessageAndExit(e.ToString());
            }
        }

        private void FindGuardianInstallation()
        {
            try
            {
                RegistryKey k = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer\\FeatureUsage\\AppSwitched");
                foreach (string s in k.GetValueNames())
                {
                    if (s.Contains("Guardian.exe"))
                    {
                        GuardianJSONPath = $"{Path.GetDirectoryName(s)}\\data.json";
                        break;
                    }
                }

                CheckFileExists(GuardianJSONPath);
            }
            catch (Exception e)
            {
                ErrorCode = EErrorCode.FAILED_TO_LOOKUP_REGISTRY_KEY;
                ShowMessageAndExit(e.ToString());
            }
        }

        protected override void BuildIPWhitelist()
        {
            WhitelistedIps.Clear();
            string js = File.ReadAllText(GuardianJSONPath);
            Rootobject jsonDocument = JsonSerializer.Deserialize<Rootobject>(js);
            if (jsonDocument.friends != null)
                foreach (Friend s in jsonDocument.friends)
                {
                    if (s.enabled)
                        WhitelistedIps.Add(s.ip);
                }
        }

        public override List<string> GetExistingRules()
        {
            List<string> rulesList = new List<string>();

            try
            {
                INetFwPolicy2 firewallPolicy = (INetFwPolicy2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));

                foreach (INetFwRule p in firewallPolicy.Rules)
                {
                    if (p.ApplicationName != null)
                        if (p.ApplicationName.Contains("GTA5.exe"))
                        {
                            rulesList.Add(p.Name);
                        }
                }
            }
            catch (Exception e)
            {
                ErrorCode = EErrorCode.FAILED_TO_CHANGE_WINDOWS_FIREWALL;
                ShowMessageAndExit(e.ToString());
            }

            return rulesList;
        }

        public override void ClearFWRules()
        {
            try
            {
                Console.WriteLine("Removing all Firewall Rules pointing to the GTA5.exe");

                INetFwPolicy2 firewallPolicy = (INetFwPolicy2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));

                List<string> delList = GetExistingRules();

                foreach (string s in delList)
                {
                    firewallPolicy.Rules.Remove(s);
                }
            }
            catch (Exception e)
            {
                ErrorCode = EErrorCode.FAILED_TO_CHANGE_WINDOWS_FIREWALL;
                ShowMessageAndExit(e.ToString());
            }
        }

        protected override void UpdateInboundFWRules()
        {
            try
            {
                INetFwRule firewallRule = (INetFwRule)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FWRule"));
                INetFwPolicy2 firewallPolicy = (INetFwPolicy2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));
                firewallRule.ApplicationName = GTA5ExePath;
                firewallRule.RemoteAddresses = string.Join(",", WhitelistedIps);
                firewallRule.Action = NET_FW_ACTION_.NET_FW_ACTION_ALLOW;
                firewallRule.Enabled = true;
                firewallRule.InterfaceTypes = "All";
                firewallRule.Name = $"GTA5 Online - LeaveMeAlone Inbound Whitelist";
                //firewallRule.Protocol = 17;
                //firewallRule.LocalPorts = "*";
                firewallRule.Direction = NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN;
                firewallPolicy.Rules.Add(firewallRule);
                Console.WriteLine("Added Firewall Inbound Whitelist Rule");

                firewallRule = (INetFwRule)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FWRule"));
                firewallPolicy = (INetFwPolicy2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));
                firewallRule.Action = NET_FW_ACTION_.NET_FW_ACTION_BLOCK;
                firewallRule.RemoteAddresses = string.Join(",", WhitelistedIps);
                firewallRule.ApplicationName = GTA5ExePath;
                firewallRule.Enabled = true;
                firewallRule.InterfaceTypes = "All";
                firewallRule.Direction = NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN;
                firewallRule.Protocol = 17;
                firewallRule.LocalPorts = "6672,65431";
                firewallRule.Name = $"GTA5 Online - LeaveMeAlone Inbound Blacklist";
                firewallPolicy.Rules.Add(firewallRule);
                Console.WriteLine("Added Firewall Inbound Blacklist Rule");
            }
            catch (Exception e)
            {
                ErrorCode = EErrorCode.FAILED_TO_CHANGE_WINDOWS_FIREWALL;
                ShowMessageAndExit(e.ToString());
            }
        }

        protected override void UpdateOutboundFWRules()
        {
            try
            {
                INetFwRule firewallRule = (INetFwRule)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FWRule"));
                INetFwPolicy2 firewallPolicy = (INetFwPolicy2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));
                firewallRule.ApplicationName = GTA5ExePath;
                firewallRule.RemoteAddresses = string.Join(",", WhitelistedIps);
                firewallRule.Action = NET_FW_ACTION_.NET_FW_ACTION_ALLOW;
                firewallRule.Enabled = true;
                firewallRule.InterfaceTypes = "All";
                //firewallRule.Protocol = 17;
                //firewallRule.LocalPorts = "6672";
                firewallRule.Name = $"GTA5 Online - LeaveMeAlone Outbound Whitelist";
                firewallRule.Direction = NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_OUT;
                firewallPolicy.Rules.Add(firewallRule);
                Console.WriteLine("Added Firewall Outbound Whitelist Rule");

                firewallRule = (INetFwRule)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FWRule"));
                firewallPolicy = (INetFwPolicy2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));
                firewallRule.Action = NET_FW_ACTION_.NET_FW_ACTION_BLOCK;
                firewallRule.RemoteAddresses = "*";// string.Join(",", WhitelistedIps);
                firewallRule.ApplicationName = GTA5ExePath;
                firewallRule.Protocol = 17;
                firewallRule.Enabled = true;
                firewallRule.Direction = NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_OUT;
                firewallRule.InterfaceTypes = "All";
                firewallRule.LocalPorts = "6672,65431";
                firewallRule.Name = $"GTA5 Online - LeaveMeAlone Outbound Blacklist";
                firewallPolicy.Rules.Add(firewallRule);
                Console.WriteLine("Added Firewall Outbound Blacklist Rule");
            }
            catch (Exception e)
            {
                ErrorCode = EErrorCode.FAILED_TO_CHANGE_WINDOWS_FIREWALL;
                ShowMessageAndExit(e.ToString());
            }
        }
    }
}
