﻿using GetStoreApp.Extensions.DataType.Enums;
using GetStoreApp.Extensions.SystemTray;
using GetStoreApp.Helpers.Root;
using GetStoreApp.Services.Controls.Download;
using GetStoreApp.Services.Controls.Settings.Advanced;
using GetStoreApp.Services.Controls.Settings.Appearance;
using GetStoreApp.Services.Controls.Settings.Common;
using GetStoreApp.Services.Controls.Settings.Experiment;
using GetStoreApp.Services.Root;
using GetStoreApp.WindowsAPI.PInvoke.Kernel32;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.StartScreen;
using WinRT;

namespace GetStoreApp
{
    public class Program
    {
        public static bool IsDesktopProgram { get; set; } = true;

        public static List<string> CommandLineArgs { get; set; }

        // 应用程序实例
        public static App ApplicationRoot { get; set; }

        public static bool IsAppLaunched { get; set; } = false;

        /// <summary>
        /// 应用程序的主入口点
        /// </summary>
        [STAThread]
        public static void Main(string[] args)
        {
            CommandLineArgs = args.ToList();
            IsDesktopProgram = GetAppExecuteMode();

            InitializeProgramResourcesAsync().Wait();

            // 以桌面应用程序方式正常启动
            if (IsDesktopProgram)
            {
                AppNotificationService.Initialize();
                DesktopLaunchService.InitializeLaunchAsync().Wait();

                ComWrappersSupport.InitializeComWrappers();
                Application.Start((param) =>
                {
                    DispatcherQueueSynchronizationContext context = new DispatcherQueueSynchronizationContext(DispatcherQueue.GetForCurrentThread());
                    SynchronizationContext.SetSynchronizationContext(context);
                    ApplicationRoot = new App();
                });
            }

            // 以控制台方式启动程序
            else
            {
                bool AttachResult = Kernel32Library.AttachConsole();
                if (!AttachResult)
                {
                    Kernel32Library.AllocConsole();
                }
                ConsoleLaunchService.InitializeConsoleStartupAsync().Wait();

                Kernel32Library.FreeConsole();

                // 退出应用程序
                Environment.Exit(Convert.ToInt32(AppExitCode.Successfully));
            }
        }

        /// <summary>
        /// 检查命令参数是否以桌面方式启动
        /// </summary>
        private static bool GetAppExecuteMode()
        {
            return CommandLineArgs.Count is 0 || !CommandLineArgs[0].Equals("Console", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 加载应用程序所需的资源
        /// </summary>
        private static async Task InitializeProgramResourcesAsync()
        {
            // 初始化应用资源，应用使用的语言信息和启动参数
            await LanguageService.InitializeLanguageAsync();
            await ResourceService.InitializeResourceAsync(LanguageService.DefaultAppLanguage, LanguageService.AppLanguage);

            // 初始化通用设置选项（桌面应用程序和控制台应用程序）
            await ResourceService.LocalizeReosurceAsync();
            await RegionService.InitializeRegionAsync();
            await LinkFilterService.InitializeLinkFilterValueAsnyc();
            await DownloadOptionsService.InitializeAsync();

            // 初始化应用版本和系统版本信息
            InfoHelper.InitializeAppVersion();
            InfoHelper.InitializeSystemVersion();

            // 初始化应用任务跳转列表信息
            AppJumpList.GroupName = ResourceService.GetLocalized("Window/JumpListGroupName");
            AppJumpList.GroupKind = JumpListSystemGroupKind.Recent;

            if (IsDesktopProgram)
            {
                // 初始化数据库信息
                await DataBaseService.InitializeDataBaseAsync();
                await DownloadDBService.InitializeDownloadDBAsync();

                // 初始化应用配置信息
                await AppExitService.InitializeAppExitAsync();
                await InstallModeService.InitializeInstallModeAsync();

                await AlwaysShowBackdropService.InitializeAlwaysShowBackdropAsync();
                await BackdropService.InitializeBackdropAsync();
                await ThemeService.InitializeThemeAsync();
                await TopMostService.InitializeTopMostValueAsync();

                await HistoryRecordService.InitializeAsync();
                await NotificationService.InitializeNotificationAsync();
                await UseInstructionService.InitializeUseInsVisValueAsync();

                // 实验功能设置配置
                await NetWorkMonitorService.InitializeNetWorkMonitorValueAsync();
            }
        }
    }
}
