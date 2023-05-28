﻿using GetStoreApp.Services.Controls.Settings.Appearance;
using Microsoft.UI.Xaml;
using System;
using Windows.System;

namespace GetStoreApp.ViewModels.Controls.WinGet
{
    /// <summary>
    /// WinGet 程序包不存在提示控件视图模型
    /// </summary>
    public sealed class InitializeFailedViewModel
    {
        /// <summary>
        /// 了解更多有关 WinGet 程序包的描述信息
        /// </summary>
        public async void OnLearnMoreClicked(object sender, RoutedEventArgs args)
        {
            await Launcher.LaunchUriAsync(new Uri(string.Format(@"https://learn.microsoft.com/{0}/windows/package-manager/", LanguageService.AppLanguage.InternalName)));
        }

        /// <summary>
        /// 从微软商店中下载 WinGet 程序包管理器
        /// </summary>
        public async void OnDownloadFromMicrosoftStoreClicked(object sender, RoutedEventArgs args)
        {
            await Launcher.LaunchUriAsync(new Uri("ms-windows-store://pdp/ProductId=9NBLGGH4NNS1"));
        }

        /// <summary>
        /// 从Github中下载 WinGet 程序包管理器
        /// </summary>
        public async void OnDownloadFromGithubClicked(object sender, RoutedEventArgs args)
        {
            await Launcher.LaunchUriAsync(new Uri("https://github.com/microsoft/winget-cli/releases"));
        }
    }
}
