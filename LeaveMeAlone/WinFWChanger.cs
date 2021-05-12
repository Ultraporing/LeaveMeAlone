using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Versioning;
using System.Security.Principal;

namespace LeaveMeAlone
{
    [SupportedOSPlatform("windows")]
    public abstract class WinFWChanger
    {
        private enum DefaultEnum
        {
            FILE_NOT_FOUND = -1
        }

        protected List<string> WhitelistedIps { get; } = new List<string>();
        protected Enum ErrorCode { get; set; }

        public static bool IsElevated => new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
        public abstract void AddFirewallRules();
        public abstract void ClearFWRules();
        public abstract List<string> GetExistingRules();
        
        protected virtual void UpdateFWRules()
        {
            ClearFWRules();
            UpdateInboundFWRules();
            UpdateOutboundFWRules();
        }

        protected abstract void UpdateInboundFWRules();
        protected abstract void UpdateOutboundFWRules();
        protected abstract void BuildIPWhitelist();

        protected virtual void CheckFileExists(string filePath)
        {
            if (File.Exists(filePath))
            {
                string name = "";
                name = Path.GetFileNameWithoutExtension(filePath);
                Console.WriteLine($"{name} found at: \"{filePath}\"");
            }
            else
            {
                ErrorCode = DefaultEnum.FILE_NOT_FOUND;
                Console.WriteLine($"{Path.GetFileName(filePath)} not found");
                ShowMessageAndExit(ErrorCode);
            }
        }

        protected void ShowMessageAndExit(string optAdditionalMsg = "")
        {
            Console.WriteLine($"The Application Exited with the ErrorCode:{Enum.GetName(ErrorCode.GetType(), ErrorCode)}.");
            Console.WriteLine($"{optAdditionalMsg}\nPress Enter to close...");

            int idx = Array.BinarySearch(ErrorCode.GetType().GetEnumValues(), ErrorCode);
            Console.ReadLine();
            Environment.Exit((int)ErrorCode.GetType().GetEnumValues().GetValue(idx));
        }

        protected void ShowMessageAndExit<ErrorEnumType>(ErrorEnumType errorEnumType, string optAdditionalMsg = "")
        {
            Console.WriteLine($"The Application Exited with the ErrorCode:{Enum.GetName(errorEnumType.GetType(), errorEnumType)}.");
            Console.WriteLine($"{optAdditionalMsg}\nPress Enter to close...");

            int idx = Array.BinarySearch(errorEnumType.GetType().GetEnumValues(), errorEnumType);
            Console.ReadLine();
            Environment.Exit((int)errorEnumType.GetType().GetEnumValues().GetValue(idx));
        }
    }
}