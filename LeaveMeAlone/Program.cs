using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using NetFwTypeLib;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LeaveMeAlone
{
    class Program
    {
        static void Main(string[] args)
        {
            LeaveMeAlone leaveMeAlone = args.Length > 0 ? new LeaveMeAlone(Path.GetFullPath(args[0]).ToString()) : new LeaveMeAlone();
        }
    }
}
