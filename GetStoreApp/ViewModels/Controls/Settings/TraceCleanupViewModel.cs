﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GetStoreApp.UI.Dialogs;
using System;

namespace GetStoreApp.ViewModels.Controls.Settings
{
    public class TraceCleanupViewModel : ObservableRecipient
    {
        // 清理应用内使用的所有痕迹
        public IRelayCommand TraceCleanupCommand = new RelayCommand(async () =>
        {
            await new TraceCleanupPromptDialog().ShowAsync();
        });
    }
}
