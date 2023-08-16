using GetStoreApp.Helpers.Converters;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.ApplicationModel;

namespace GetStoreApp.UI.Pages
{
    /// <summary>
    /// 应用信息页面
    /// </summary>
    public sealed partial class AppInfoPage : Page, INotifyPropertyChanged
    {
        private string _displayName = string.Empty;

        public string DisplayName
        {
            get { return _displayName; }

            set
            {
                _displayName = value;
                OnPropertyChanged();
            }
        }

        private string _description = string.Empty;

        public string Description
        {
            get { return _description; }

            set
            {
                _description = value;
                OnPropertyChanged();
            }
        }

        private string _publisher = string.Empty;

        public string Publisher
        {
            get { return _publisher; }

            set
            {
                _publisher = value;
                OnPropertyChanged();
            }
        }

        private string _appVersion;

        public string AppVersion
        {
            get { return _appVersion; }

            set
            {
                _appVersion = value;
                OnPropertyChanged();
            }
        }

        private string _installedDate;

        public string InstalledDate
        {
            get { return _installedDate; }

            set
            {
                _installedDate = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public AppInfoPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs args)
        {
            base.OnNavigatedTo(args);
            if (args.Parameter is not null)
            {
                InitializeAppInfo(args.Parameter as Package);
            }
            else
            {
                SetDefaultAppInfo();
            }
        }

        /// <summary>
        /// 属性值发生变化时通知更改
        /// </summary>
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// 初始化应用信息
        /// </summary>
        private void InitializeAppInfo(Package currentPackage)
        {
            try { DisplayName = currentPackage.DisplayName; } catch { DisplayName = string.Empty; }
            try { Description = currentPackage.Description; } catch { Description = string.Empty; }
            try { Publisher = currentPackage.PublisherDisplayName; } catch { Publisher = string.Empty; }
            try
            {
                AppVersion = string.Format("{0}.{1}.{2}.{3}",
                    currentPackage.Id.Version.Major,
                    currentPackage.Id.Version.Minor,
                    currentPackage.Id.Version.Build,
                    currentPackage.Id.Version.Revision
                    );
            }
            catch
            {
                AppVersion = default;
            }
            try { InstalledDate = currentPackage.InstallDate.ToString("yyyy/MM/dd HH:mm", StringConverterHelper.AppCulture); } catch { InstalledDate = default; }
        }

        /// <summary>
        /// 输入的应用包有误，恢复到默认值
        /// </summary>
        private void SetDefaultAppInfo()
        {
            DisplayName = string.Empty;
            Description = string.Empty;
            Publisher = string.Empty;
            AppVersion = default;
            InstalledDate = default;
        }
    }
}
