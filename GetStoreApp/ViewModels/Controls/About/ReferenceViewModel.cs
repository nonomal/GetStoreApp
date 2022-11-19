﻿using System.Collections.Generic;

namespace GetStoreApp.ViewModels.Controls.About
{
    public sealed class ReferenceViewModel
    {
        //项目引用信息
        public Dictionary<string, string> ReferenceDict => new Dictionary<string, string>
        {
            {"Aira2","https://aria2.github.io" },
            {"Aria2.NET","https://github.com/rogerfar/Aria2.NET" },
            {"H.NotifyIcon.WinUI","https://github.com/HavenDV/H.NotifyIcon" },
            {"HtmlAgilityPack","http://html-agility-pack.net" },
            {"Microsoft.Data.Sqlite.Core","https://docs.microsoft.com/dotnet/standard/data/sqlite" },
            {"Microsoft.Windows.CsWinRT","https://github.com/microsoft/cswinrt" },
            {"Microsoft.Windows.SDK.BuildTools","https://www.nuget.org/packages/Microsoft.Windows.SDK.BuildTools" },
            {"Microsoft.WindowsAppSDK","https://github.com/microsoft/windowsappsdk" },
            {"SQLitePCLRaw.bundle_winsqlite3","https://github.com/ericsink/SQLitePCL.raw" },
        };
    }
}
