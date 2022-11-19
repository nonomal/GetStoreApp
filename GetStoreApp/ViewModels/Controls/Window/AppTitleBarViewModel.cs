﻿using CommunityToolkit.Mvvm.Messaging;
using GetStoreApp.Helpers.Root;
using GetStoreApp.Messages;
using GetStoreApp.Services.Controls.Settings.Appearance;
using GetStoreAppWindowsAPI.PInvoke.User32;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using Windows.Graphics;
using Windows.UI;
using WinRT.Interop;

namespace GetStoreApp.ViewModels.Controls.Window
{
    public sealed class AppTitleBarViewModel
    {
        /// <summary>
        /// 初始化自定义标题栏
        /// </summary>
        public void OnLoaded(object sender, RoutedEventArgs args)
        {
            Grid appTitleBar = sender as Grid;
            if (appTitleBar is not null)
            {
                SetTitleBarColor();

                SetDragRectangles(Convert.ToInt32(appTitleBar.Margin.Left), appTitleBar.ActualWidth, appTitleBar.ActualHeight);

                ((FrameworkElement)App.MainWindow.Content).ActualThemeChanged += OnActualThemeChanged;

                // 应用主题设置跟随系统发生变化时，当系统主题设置发生变化时修改标题栏按钮主题
                WeakReferenceMessenger.Default.Register<AppTitleBarViewModel, SystemSettingsChnagedMessage>(this, (appTitleBarViewModel, systemSettingsChangedMessage) =>
                {
                    if (ThemeService.AppTheme.InternalName == ThemeService.ThemeList[0].InternalName)
                    {
                        SetTitleBarButtonColor(RegistryHelper.GetRegistryAppTheme());
                    }
                });
            }
        }

        /// <summary>
        /// 控件大小发生变化时，修改拖动区域
        /// </summary>
        public void OnSizeChanged(object sender, RoutedEventArgs args)
        {
            Grid appTitleBar = sender as Grid;
            if (appTitleBar is not null)
            {
                SetDragRectangles(Convert.ToInt32(appTitleBar.Margin.Left), appTitleBar.ActualWidth, appTitleBar.ActualHeight);
            }
        }

        /// <summary>
        /// 控件被卸载时，关闭所有事件，关闭消息服务
        /// </summary>
        public void OnUnloaded(object sender, RoutedEventArgs args)
        {
            ((FrameworkElement)App.MainWindow.Content).ActualThemeChanged -= OnActualThemeChanged;
            WeakReferenceMessenger.Default.UnregisterAll(this);
        }

        /// <summary>
        /// 设置主题发生变化时修改标题栏按钮的主题
        /// </summary>
        private void OnActualThemeChanged(FrameworkElement sender, object args)
        {
            SetTitleBarColor();
        }

        /// <summary>
        /// 根据应用设置存储的主题值设置标题栏按钮的颜色
        /// </summary>
        private void SetTitleBarColor()
        {
            if (ThemeService.AppTheme.InternalName == ThemeService.ThemeList[0].InternalName)
            {
                SetTitleBarButtonColor(RegistryHelper.GetRegistryAppTheme());
            }
            else if (ThemeService.AppTheme.InternalName == ThemeService.ThemeList[1].InternalName)
            {
                SetTitleBarButtonColor(ElementTheme.Light);
            }
            else if (ThemeService.AppTheme.InternalName == ThemeService.ThemeList[2].InternalName)
            {
                SetTitleBarButtonColor(ElementTheme.Dark);
            }
        }

        /// <summary>
        /// 设置标题栏按钮的颜色
        /// </summary>
        private void SetTitleBarButtonColor(ElementTheme theme)
        {
            AppWindowTitleBar bar = App.AppWindow.TitleBar;

            switch (theme)
            {
                case ElementTheme.Light:
                    {
                        bar.ButtonBackgroundColor = Colors.Transparent;
                        bar.ButtonForegroundColor = Colors.Black;
                        bar.ButtonHoverBackgroundColor = Color.FromArgb(255, 233, 233, 233);
                        bar.ButtonHoverForegroundColor = Colors.Black;
                        bar.ButtonPressedBackgroundColor = Color.FromArgb(255, 237, 237, 237);
                        bar.ButtonPressedForegroundColor = Colors.Black;
                        bar.ButtonInactiveBackgroundColor = Colors.Transparent;
                        bar.ButtonInactiveForegroundColor = Colors.Gray;
                        break;
                    }
                case ElementTheme.Dark:
                    {
                        bar.ButtonBackgroundColor = Colors.Transparent;
                        bar.ButtonForegroundColor = Colors.White;
                        bar.ButtonHoverBackgroundColor = Color.FromArgb(255, 45, 45, 45);
                        bar.ButtonHoverForegroundColor = Colors.White;
                        bar.ButtonPressedBackgroundColor = Color.FromArgb(255, 40, 40, 40);
                        bar.ButtonPressedForegroundColor = Colors.White;
                        bar.ButtonInactiveBackgroundColor = Colors.Transparent;
                        bar.ButtonInactiveForegroundColor = Colors.Gray;
                        break;
                    }
            }
        }

        /// <summary>
        /// 设置标题栏的可拖动区域
        /// </summary>
        private void SetDragRectangles(int leftMargin, double actualWidth, double actualHeight)
        {
            App.AppWindow.TitleBar.SetDragRectangles(new RectInt32[] { new RectInt32(leftMargin, 0, GetActualPixel(actualWidth), GetActualPixel(actualHeight)) });
        }

        /// <summary>
        /// 在设置拖动区域时，需要考虑到系统缩放比例对像素的影响.
        /// </summary>
        private static int GetActualPixel(double pixel)
        {
            int currentDpi = User32Library.GetDpiForWindow(WindowNative.GetWindowHandle(App.MainWindow));
            return Convert.ToInt32(pixel * (currentDpi / 96.0));
        }
    }
}
