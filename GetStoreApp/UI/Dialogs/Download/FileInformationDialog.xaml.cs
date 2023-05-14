﻿using GetStoreApp.Models.Controls.Download;
using GetStoreApp.Services.Controls.Settings.Appearance;
using GetStoreApp.Views.CustomControls.DialogsAndFlyouts;
using Microsoft.UI.Xaml;
using System;

namespace GetStoreApp.UI.Dialogs.Download
{
    /// <summary>
    /// 文件信息对话框视图
    /// </summary>
    public sealed partial class FileInformationDialog : ExtendedContentDialog
    {
        public ElementTheme DialogTheme { get; } = (ElementTheme)Enum.Parse(typeof(ElementTheme), ThemeService.AppTheme.InternalName);

        public FileInformationDialog(CompletedModel completedItem)
        {
            InitializeComponent();
            ViewModel.InitializeFileInformation(completedItem);
        }

        /// <summary>
        /// 关闭对话框
        /// </summary>
        public void OnCloseDialogClicked(object sender, RoutedEventArgs args)
        {
            Hide();
        }

        /// <summary>
        /// 复制文件信息
        /// </summary>
        public void OnCopyFileInformationClicked(object sender, RoutedEventArgs args)
        {
            ViewModel.CopyFileInformation();
            Hide();
        }
    }
}
