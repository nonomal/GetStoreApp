﻿using GetStoreAppHelper.Extensions.DataType.Enums;
using GetStoreAppHelper.Extensions.SystemTray;
using GetStoreAppHelper.Helpers;
using GetStoreAppHelper.Helpers.Root;
using GetStoreAppHelper.Services;
using GetStoreAppHelper.UI.Controls;
using GetStoreAppHelper.WindowsAPI.PInvoke.User32;
using Mile.Xaml;
using System;
using Windows.Graphics;

namespace GetStoreAppHelper
{
    public sealed partial class App : Application
    {
        public WindowsTrayIcon TrayIcon { get; private set; }

        public MileWindow MainWindow { get; set; }

        public App()
        {
            Initialize();
        }

        /// <summary>
        /// 启动应用
        /// </summary>
        public void Run()
        {
            InitializeTrayIcon();
            ResourceDictionaryHelper.InitializeResourceDictionaryAsync().Wait();

            MainWindow = new MileWindow();

            MainWindow.Content = new TrayMenuControl();
            MainWindow.Title = ResourceService.GetLocalized("HelperResources/Title");
            MainWindow.Position.X = 0;
            MainWindow.Position.Y = 0;
            MainWindow.Size.X = 0;
            MainWindow.Size.Y = 0;

            MainWindow.InitializeWindow();
            MainWindow.Activate();
        }

        /// <summary>
        /// 初始化应用的托盘图标
        /// </summary>
        private void InitializeTrayIcon()
        {
            TrayIcon = new WindowsTrayIcon(
                string.Format(@"{0}\{1}", InfoHelper.GetAppInstalledLocation(), "GetStoreApp.exe"),
                ResourceService.GetLocalized("HelperResources/AppName")
            );

            TrayIcon.DoubleClick = () =>
            {
                (MainWindow.Content as TrayMenuControl).ViewModel.ShowOrHideWindowCommand.Execute(null);
            };

            TrayIcon.RightClick = () =>
            {
                User32Library.GetCursorPos(out PointInt32 CurrentPoint);
                User32Library.SetForegroundWindow(MainWindow.Handle);
                (MainWindow.Content as TrayMenuControl).SetXamlRoot(MainWindow.Content.XamlRoot);
                (MainWindow.Content as TrayMenuControl).ShowMenuFlyout(CurrentPoint);
            };
        }

        /// <summary>
        /// 关闭应用并释放所有资源
        /// </summary>
        public void CloseApp()
        {
            TrayIcon.Dispose();
            Environment.Exit(Convert.ToInt32(AppExitCode.Successfully));
        }
    }
}
