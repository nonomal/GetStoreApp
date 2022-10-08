﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using GetStoreApp.Contracts.Services.Settings;
using GetStoreApp.Helpers;
using GetStoreApp.Messages;
using GetStoreApp.Models.Settings;
using System.Collections.Generic;

namespace GetStoreApp.ViewModels.Controls.Settings
{
    public class HistoryLiteConfigViewModel : ObservableRecipient
    {
        private IHistoryLiteNumService HistoryLiteNumService { get; } = IOCHelper.GetService<IHistoryLiteNumService>();

        public List<HistoryLiteNumModel> HistoryLiteNumList => HistoryLiteNumService.HistoryLiteNumList;

        private HistoryLiteNumModel _historyLiteItem;

        public HistoryLiteNumModel HistoryLiteItem
        {
            get { return _historyLiteItem; }

            set { SetProperty(ref _historyLiteItem, value); }
        }

        // 主页面“历史记录”显示数目修改
        public IRelayCommand HistoryLiteItemSelectCommand => new RelayCommand(async () =>
        {
            await HistoryLiteNumService.SetHistoryLiteNumAsync(HistoryLiteItem);
            WeakReferenceMessenger.Default.Send(new HistoryLiteNumMessage(HistoryLiteItem));
        });

        public HistoryLiteConfigViewModel()
        {
            HistoryLiteItem = HistoryLiteNumService.HistoryLiteNum;
        }
    }
}
