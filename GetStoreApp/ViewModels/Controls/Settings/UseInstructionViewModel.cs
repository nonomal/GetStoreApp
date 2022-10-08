﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GetStoreApp.Contracts.Services.Settings;
using GetStoreApp.Helpers;

namespace GetStoreApp.ViewModels.Controls.Settings
{
    public class UseInstructionViewModel : ObservableRecipient
    {
        private IUseInstructionService UseInstructionService { get; } = IOCHelper.GetService<IUseInstructionService>();

        private bool _useInsVisValue;

        public bool UseInsVisValue
        {
            get { return _useInsVisValue; }

            set { SetProperty(ref _useInsVisValue, value); }
        }

        // “使用说明”按钮显示设置
        public IRelayCommand UseInstructionCommand => new RelayCommand<bool>(async (useInsVisValue) =>
        {
            await UseInstructionService.SetUseInsVisValueAsync(useInsVisValue);
            UseInsVisValue = useInsVisValue;
        });

        public UseInstructionViewModel()
        {
            UseInsVisValue = UseInstructionService.UseInsVisValue;
        }
    }
}
