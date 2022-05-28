﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using GetStoreApp.Messages;
using GetStoreApp.Services.Settings;

namespace GetStoreApp.ViewModels.Controls.Settings
{
    public class UseInstructionViewModel : ObservableRecipient
    {
        // 主页“使用说明”按钮显示状态
        private bool _useInsVisValue = UseInstructionService.UseInsVisValue;

        public bool UseInsVisValue
        {
            get
            {
                return _useInsVisValue;
            }
            set
            {
                SetProperty(ref _useInsVisValue, value);
                UseInstructionService.SetUseInsVisValue(value);
                Messenger.Send(new UseInstructionMessage(value));
            }
        }
    }
}