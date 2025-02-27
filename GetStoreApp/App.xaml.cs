﻿using GetStoreApp.Extensions.DataType.Constant;
using GetStoreApp.Extensions.DataType.Enums;
using GetStoreApp.Helpers.Root;
using GetStoreApp.Services.Controls.Download;
using GetStoreApp.Services.Controls.Settings.Appearance;
using GetStoreApp.Services.Root;
using GetStoreApp.Views.Windows;
using GetStoreApp.WindowsAPI.PInvoke.Kernel32;
using GetStoreApp.WindowsAPI.PInvoke.User32;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.Graphics;
using Windows.UI.StartScreen;

namespace GetStoreApp
{
    /// <summary>
    /// 获取商店应用程序
    /// </summary>
    public partial class App : Application, IDisposable
    {
        private bool isDisposed;

        private IntPtr[] hIcons;

        public bool IsAppRunning { get; set; } = true;

        public bool IsAppLaunched { get; set; } = false;

        public MainWindow MainWindow { get; private set; }

        public JumpList TaskbarJumpList { get; private set; }

        public App()
        {
            InitializeComponent();
            UnhandledException += OnUnhandledException;
        }

        /// <summary>
        /// 应用启动后执行其他操作
        /// </summary>
        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            base.OnLaunched(args);
            ResourceDictionaryHelper.InitializeResourceDictionary();

            MainWindow = new MainWindow();
            IsAppLaunched = true;
            ActivateWindow();

            await InitializeJumpListAsync();
            Startup();
        }

        /// <summary>
        /// 初始化任务栏的跳转列表
        /// </summary>
        private async Task InitializeJumpListAsync()
        {
            if (JumpList.IsSupported())
            {
                TaskbarJumpList = await JumpList.LoadCurrentAsync();
                RemoveUnusedItems();
                UpdateJumpListGroupName();
                await TaskbarJumpList.SaveAsync();
            }
        }

        /// <summary>
        /// 处理应用程序未知异常处理
        /// </summary>
        public void OnUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs args)
        {
            args.Handled = true;

            // 系统背景色弹出的异常，不进行处理
            if (args.Exception.HResult is -2147024809 && args.Exception.StackTrace.Contains("SystemBackdropConfiguration"))
            {
                LogService.WriteLog(LogType.WARNING, "System backdrop config warning.", args.Exception);
                return;
            }
            // 处理其他异常
            else
            {
                LogService.WriteLog(LogType.ERROR, "Unknown unhandled exception.", args.Exception);

                // 退出应用
                Dispose();
            }
        }

        /// <summary>
        /// 窗口激活前进行配置
        /// </summary>
        public void ActivateWindow()
        {
            bool? IsWindowMaximized = ConfigService.ReadSetting<bool?>(ConfigKey.IsWindowMaximizedKey);
            int? WindowWidth = ConfigService.ReadSetting<int?>(ConfigKey.WindowWidthKey);
            int? WindowHeight = ConfigService.ReadSetting<int?>(ConfigKey.WindowHeightKey);
            int? WindowPositionXAxis = ConfigService.ReadSetting<int?>(ConfigKey.WindowPositionXAxisKey);
            int? WindowPositionYAxis = ConfigService.ReadSetting<int?>(ConfigKey.WindowPositionYAxisKey);

            if (IsWindowMaximized.HasValue && IsWindowMaximized.Value is true)
            {
                MainWindow.Presenter.Maximize();
            }
            else
            {
                if (WindowWidth.HasValue && WindowHeight.HasValue && WindowPositionXAxis.HasValue && WindowPositionYAxis.HasValue)
                {
                    MainWindow.AppWindow.MoveAndResize(new RectInt32(
                        WindowPositionXAxis.Value,
                        WindowPositionYAxis.Value,
                        WindowWidth.Value,
                        WindowHeight.Value
                        ));
                }
            }

            SetAppIcon();
            MainWindow.AppWindow.Show();
        }

        /// <summary>
        /// 窗口激活后配置其他设置
        /// </summary>
        public void Startup()
        {
            // 设置应用主题
            ThemeService.SetWindowTheme();

            // 设置应用背景色
            BackdropService.SetAppBackdrop();

            // 设置应用置顶状态
            TopMostService.SetAppTopMost();

            // 初始化 WinGet 程序包安装信息
            WinGetService.InitializeService();

            // 初始化下载监控服务
            DownloadSchedulerService.InitializeDownloadScheduler();

            // 初始化Aria2配置文件信息
            Aria2Service.InitializeAria2Conf();

            // 启动Aria2下载服务
            Aria2Service.StartAria2Process();
        }

        /// <summary>
        /// 移除用户主动删除的条目
        /// </summary>
        public void RemoveUnusedItems()
        {
            // 如果某一条目被用户主动删除，应用初始化时则自动删除该条目
            for (int index = 0; index < TaskbarJumpList.Items.Count; index++)
            {
                if (TaskbarJumpList.Items[index].RemovedByUser)
                {
                    TaskbarJumpList.Items.RemoveAt(index);
                    index--;
                }
            }
        }

        /// <summary>
        /// 更新跳转列表组名
        /// </summary>
        public void UpdateJumpListGroupName()
        {
            foreach (JumpListItem jumpListItem in TaskbarJumpList.Items)
            {
                jumpListItem.GroupName = ResourceService.GetLocalized("Window/JumpListGroupName");
            }
        }

        /// <summary>
        /// 设置应用窗口图标
        /// </summary>
        private void SetAppIcon()
        {
            // 选中文件中的图标总数
            int iconTotalCount = User32Library.PrivateExtractIcons(Path.Combine(InfoHelper.AppInstalledLocation, "GetStoreApp.exe"), 0, 0, 0, null, null, 0, 0);

            // 用于接收获取到的图标指针
            hIcons = new IntPtr[iconTotalCount];

            // 对应的图标id
            int[] ids = new int[iconTotalCount];

            // 成功获取到的图标个数
            int successCount = User32Library.PrivateExtractIcons(Path.Combine(InfoHelper.AppInstalledLocation, "GetStoreApp.exe"), 0, 256, 256, hIcons, ids, iconTotalCount, 0);

            // GetStoreApp.exe 应用程序只有一个图标
            if (successCount >= 1 && hIcons[0] != IntPtr.Zero)
            {
                MainWindow.AppWindow.SetIcon(Win32Interop.GetIconIdFromIcon(hIcons[0]));
            }
        }

        /// <summary>
        /// 关闭应用并释放所有资源
        /// </summary>
        private void CloseApp()
        {
            IsAppRunning = false;
            SaveWindowInformation();
            DownloadSchedulerService.CloseDownloadScheduler();
            Aria2Service.CloseAria2();
            Exit();
        }

        /// <summary>
        /// 关闭窗口时保存窗口的大小和位置信息
        /// </summary>
        private void SaveWindowInformation()
        {
            ConfigService.SaveSetting(ConfigKey.IsWindowMaximizedKey, MainWindow.AppTitlebar.IsWindowMaximized);
            ConfigService.SaveSetting(ConfigKey.WindowWidthKey, MainWindow.AppWindow.Size.Width);
            ConfigService.SaveSetting(ConfigKey.WindowHeightKey, MainWindow.AppWindow.Size.Height);
            ConfigService.SaveSetting(ConfigKey.WindowPositionXAxisKey, MainWindow.AppWindow.Position.X);
            ConfigService.SaveSetting(ConfigKey.WindowPositionYAxisKey, MainWindow.AppWindow.Position.Y);
        }

        /// <summary>
        /// 重启应用
        /// </summary>
        public void Restart()
        {
            MainWindow.AppWindow.Hide();
            unsafe
            {
                Kernel32Library.GetStartupInfo(out STARTUPINFO WinGetProcessStartupInfo);
                WinGetProcessStartupInfo.lpReserved = null;
                WinGetProcessStartupInfo.lpDesktop = null;
                WinGetProcessStartupInfo.lpTitle = null;
                WinGetProcessStartupInfo.dwX = 0;
                WinGetProcessStartupInfo.dwY = 0;
                WinGetProcessStartupInfo.dwXSize = 0;
                WinGetProcessStartupInfo.dwYSize = 0;
                WinGetProcessStartupInfo.dwXCountChars = 500;
                WinGetProcessStartupInfo.dwYCountChars = 500;
                WinGetProcessStartupInfo.dwFlags = STARTF.STARTF_USESHOWWINDOW;
                WinGetProcessStartupInfo.wShowWindow = WindowShowStyle.SW_SHOWNORMAL;
                WinGetProcessStartupInfo.cbReserved2 = 0;
                WinGetProcessStartupInfo.lpReserved2 = IntPtr.Zero;
                WinGetProcessStartupInfo.cb = Marshal.SizeOf(typeof(STARTUPINFO));

                bool createResult = Kernel32Library.CreateProcess(null, "GetStoreApp.exe Restart", IntPtr.Zero, IntPtr.Zero, false, CreateProcessFlags.None, IntPtr.Zero, null, ref WinGetProcessStartupInfo, out PROCESS_INFORMATION WinGetProcessInformation);

                if (createResult)
                {
                    if (WinGetProcessInformation.hProcess != IntPtr.Zero) Kernel32Library.CloseHandle(WinGetProcessInformation.hProcess);
                    if (WinGetProcessInformation.hThread != IntPtr.Zero) Kernel32Library.CloseHandle(WinGetProcessInformation.hThread);
                }
            }
            Dispose();
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~App()
        {
            Dispose(false);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing)
                {
                    CloseApp();
                }

                isDisposed = true;
            }
        }
    }
}
