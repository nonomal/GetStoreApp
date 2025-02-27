﻿using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GetStoreApp.Models.Controls.WinGet
{
    /// <summary>
    /// 可升级应用数据模型
    /// </summary>
    public class UpgradableAppsModel : INotifyPropertyChanged
    {
        /// <summary>
        /// 应用ID
        /// </summary>
        public string AppID { get; set; }

        /// <summary>
        /// 应用名称
        /// </summary>
        public string AppName { get; set; }

        /// <summary>
        /// 应用的发布者
        /// </summary>
        public string AppPublisher { get; set; }

        /// <summary>
        /// 应用版本
        /// </summary>
        public string AppCurrentVersion { get; set; }

        /// <summary>
        /// 应用可升级的最新版本
        /// </summary>
        public string AppNewestVersion { get; set; }

        /// <summary>
        /// 应用是否处于正在升级中状态
        /// </summary>
        private bool _isUpgrading;

        public bool IsUpgrading
        {
            get { return _isUpgrading; }

            set
            {
                _isUpgrading = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 属性值发生变化时通知更改
        /// </summary>
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
