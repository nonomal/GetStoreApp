using GetStoreApp.Extensions.DataType.Enums;
using GetStoreApp.Models.Controls.UWPApp;
using GetStoreApp.Services.Root;
using GetStoreApp.Services.Window;
using GetStoreApp.Views.Pages;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Management.Deployment;
using Windows.Storage;
using Windows.System;
using WinRT;

namespace GetStoreApp.UI.Pages
{
    /// <summary>
    /// 应用信息列表页面
    /// </summary>
    public sealed partial class AppListPage : Page, INotifyPropertyChanged
    {
        private readonly object AppShortListObject = new object();

        private bool isInitialized = false;

        private PackageManager PackageManager { get; } = new PackageManager();

        public string SearchText { get; set; } = string.Empty;

        private bool _isLoadedCompleted = false;

        public bool IsLoadedCompleted
        {
            get { return _isLoadedCompleted; }

            set
            {
                _isLoadedCompleted = value;
                OnPropertyChanged();
            }
        }

        private bool _isPackageEmpty = true;

        public bool IsPackageEmpty
        {
            get { return _isPackageEmpty; }

            set
            {
                _isPackageEmpty = value;
                OnPropertyChanged();
            }
        }

        private bool _isIncrease = true;

        public bool IsIncrease
        {
            get { return _isIncrease; }

            set
            {
                _isIncrease = value;
                OnPropertyChanged();
            }
        }

        private bool _isFramework = false;

        public bool IsFramework
        {
            get { return _isFramework; }

            set
            {
                _isFramework = value;
                OnPropertyChanged();
            }
        }

        private AppListRuleSeletedType _selectedType = AppListRuleSeletedType.PackageName;

        public AppListRuleSeletedType SelectedType
        {
            get { return _selectedType; }

            set
            {
                _selectedType = value;
                OnPropertyChanged();
            }
        }

        private PackageSignType _signType = PackageSignType.None | PackageSignType.Developer | PackageSignType.Enterprise | PackageSignType.Store | PackageSignType.System;

        public PackageSignType SignType
        {
            get { return _signType; }

            set
            {
                _signType = value;
                OnPropertyChanged();
            }
        }

        // 查看应用信息
        public XamlUICommand ViewInformationCommand { get; } = new XamlUICommand();

        // 打开应用
        public XamlUICommand OpenAppCommand { get; } = new XamlUICommand();

        // 打开商店
        public XamlUICommand OpenStoreCommand { get; } = new XamlUICommand();

        // 获取应用安装包
        public XamlUICommand GetPackageCommand { get; } = new XamlUICommand();

        // 打开应用清单文件
        public XamlUICommand OpenManifestCommand { get; } = new XamlUICommand();

        // 打开应用安装目录
        public XamlUICommand OpenInstalledFolderCommand { get; } = new XamlUICommand();

        // 卸载应用
        public XamlUICommand UnInstallCommand { get; } = new XamlUICommand();

        private List<Package> MatchResultList;

        public ObservableCollection<PackageModel> AppShortList { get; } = new ObservableCollection<PackageModel>();

        public event PropertyChangedEventHandler PropertyChanged;

        public AppListPage()
        {
            InitializeComponent();

            ViewInformationCommand.ExecuteRequested += (sender, args) =>
            {
                Package package = args.Parameter.As<Package>();
                UWPAppPage uwpAppPage = NavigationService.NavigationFrame.Content.As<UWPAppPage>();
                if (package is not null && uwpAppPage is not null)
                {
                    uwpAppPage.ShowAppInformation(package);
                }
            };

            OpenAppCommand.ExecuteRequested += async (sender, args) =>
            {
                Package package = args.Parameter.As<Package>();

                if (package is not null)
                {
                    try
                    {
                        await package.GetAppListEntries()[0].LaunchAsync();
                    }
                    catch (Exception e)
                    {
                        LogService.WriteLog(LogType.ERROR, string.Format("Open app {0} failed", package.DisplayName), e);
                    }
                }
            };

            OpenStoreCommand.ExecuteRequested += async (sender, args) =>
            {
                string packageFamilyName = args.Parameter as string;
                if (packageFamilyName is not null)
                {
                    try
                    {
                        await Launcher.LaunchUriAsync(new Uri($"ms-windows-store://pdp/?PFN={packageFamilyName}"));
                    }
                    catch (Exception e)
                    {
                        LogService.WriteLog(LogType.ERROR, string.Format("Open microsoft store {0} failed", packageFamilyName), e);
                    }
                }
            };

            GetPackageCommand.ExecuteRequested += (sender, args) =>
            {
                string packageFamilyName = args.Parameter as string;
                if (packageFamilyName is not null)
                {
                    NavigationService.NavigateTo(typeof(StorePage), new object[] { AppNaviagtionArgs.Store, "PackageFamilyName", "Retail", packageFamilyName });
                }
            };

            OpenManifestCommand.ExecuteRequested += async (sender, args) =>
            {
                Package package = args.Parameter.As<Package>();
                if (package is not null)
                {
                    try
                    {
                        StorageFile file = await StorageFile.GetFileFromPathAsync(Path.Combine(package.InstalledPath, "AppxManifest.xml"));
                        if (file is not null)
                        {
                            await Launcher.LaunchFileAsync(file);
                        }
                    }
                    catch (Exception e)
                    {
                        LogService.WriteLog(LogType.ERROR, string.Format("{0}'s AppxManifest.xml file open failed", package.DisplayName), e);
                    }
                }
            };

            OpenInstalledFolderCommand.ExecuteRequested += async (sender, args) =>
            {
                StorageFolder folder = args.Parameter.As<StorageFolder>();
                if (folder is not null)
                {
                    await Launcher.LaunchFolderAsync(folder);
                }
            };

            UnInstallCommand.ExecuteRequested += (sender, args) =>
            {
                string packageFullName = args.Parameter as string;
                if (packageFullName is not null)
                {
                    for (int index = 0; index < AppShortList.Count; index++)
                    {
                        if (AppShortList[index].Package.Id.FullName == packageFullName)
                        {
                            lock (AppShortListObject)
                            {
                                AppShortList[index].IsUnInstalling = true;
                            }

                            try
                            {
                                IAsyncOperationWithProgress<DeploymentResult, DeploymentProgress> uninstallOperation = PackageManager.RemovePackageAsync(packageFullName);

                                ManualResetEvent uninstallCompletedEvent = new ManualResetEvent(false);

                                uninstallOperation.Completed = (result, progress) =>
                                {
                                    // 卸载成功
                                    if (result.Status is AsyncStatus.Completed)
                                    {
                                        ToastNotificationService.Show(NotificationArgs.UWPUnInstallSuccessfully, AppShortList[index].Package.DisplayName);

                                        DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Low, () =>
                                        {
                                            lock (AppShortListObject)
                                            {
                                                AppShortList.RemoveAt(index);
                                            }
                                        });
                                    }

                                    // 卸载失败
                                    else if (result.Status is AsyncStatus.Error)
                                    {
                                        DeploymentResult uninstallResult = uninstallOperation.GetResults();
                                        ToastNotificationService.Show(NotificationArgs.UWPUnInstallFailed,
                                            AppShortList[index].Package.DisplayName,
                                            uninstallResult.ExtendedErrorCode.HResult.ToString(),
                                            uninstallResult.ErrorText
                                            );

                                        LogService.WriteLog(LogType.INFO, string.Format("UnInstall app {0} failed", AppShortList[index].Package.DisplayName), uninstallResult.ExtendedErrorCode);

                                        DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Low, () =>
                                        {
                                            lock (AppShortListObject)
                                            {
                                                AppShortList[index].IsUnInstalling = false;
                                            }
                                        });
                                    }

                                    uninstallCompletedEvent.Set();
                                };

                                uninstallCompletedEvent.WaitOne();
                                uninstallCompletedEvent.Dispose();
                            }
                            catch (Exception e)
                            {
                                LogService.WriteLog(LogType.INFO, string.Format("UnInstall app {0} failed", AppShortList[index].Package.DisplayName), e);
                            }
                            break;
                        }
                    }
                }
            };
        }

        /// <summary>
        /// 本地化应用管理记录数量统计信息
        /// </summary>
        public string LocalizeUWPAppCountInfo(int count)
        {
            if (count is 0)
            {
                return ResourceService.GetLocalized("UWPApp/PackageEmpty");
            }
            else
            {
                return string.Format(ResourceService.GetLocalized("UWPApp/PackageCountInfo"), count);
            }
        }

        public bool IsItemChecked(AppListRuleSeletedType seletedType, AppListRuleSeletedType comparedType)
        {
            return seletedType == comparedType;
        }

        public bool IsSignatureItemChecked(PackageSignType selectedSignType, PackageSignType comparedSingType)
        {
            return selectedSignType.HasFlag(comparedSingType);
        }

        /// <summary>
        /// 初始化已安装应用信息
        /// </summary>
        public async void OnLoaded(object sender, RoutedEventArgs args)
        {
            if (!isInitialized)
            {
                await Task.Delay(500);
                await GetInstalledAppsAsync();
                await InitializeDataAsync();
                isInitialized = true;
            }
        }

        /// <summary>
        /// 显示排序规则
        /// </summary>
        public void OnSortClicked(object sender, RoutedEventArgs args)
        {
            FlyoutBase.ShowAttachedFlyout(sender.As<FrameworkElement>());
        }

        /// <summary>
        /// 根据排序方式对列表进行排序
        /// </summary>
        public async void OnSortWayClicked(object sender, RoutedEventArgs args)
        {
            ToggleMenuFlyoutItem toggleMenuFlyoutItem = sender.As<ToggleMenuFlyoutItem>();
            if (toggleMenuFlyoutItem is not null)
            {
                IsIncrease = Convert.ToBoolean(toggleMenuFlyoutItem.Tag);
                await InitializeDataAsync();
            }
        }

        /// <summary>
        /// 根据排序规则对列表进行排序
        /// </summary>
        public async void OnSortRuleClicked(object sender, RoutedEventArgs args)
        {
            ToggleMenuFlyoutItem toggleMenuFlyoutItem = sender.As<ToggleMenuFlyoutItem>();
            if (toggleMenuFlyoutItem is not null)
            {
                SelectedType = (AppListRuleSeletedType)toggleMenuFlyoutItem.Tag;
                await InitializeDataAsync();
            }
        }

        /// <summary>
        /// 显示过滤规则
        /// </summary>
        public void OnFilterClicked(object sender, RoutedEventArgs args)
        {
            FlyoutBase.ShowAttachedFlyout(sender.As<FrameworkElement>());
        }

        /// <summary>
        /// 根据过滤方式对列表进行过滤
        /// </summary>
        public async void OnFilterWayClicked(object sender, RoutedEventArgs args)
        {
            IsFramework = !IsFramework;
            await InitializeDataAsync();
        }

        /// <summary>
        /// 根据签名规则进行过滤
        /// </summary>
        public async void OnSignatureRuleClicked(object sender, RoutedEventArgs args)
        {
            ToggleMenuFlyoutItem toggleMenuFlyoutItem = sender.As<ToggleMenuFlyoutItem>();
            if (toggleMenuFlyoutItem is not null)
            {
                if (SignType.HasFlag((PackageSignType)toggleMenuFlyoutItem.Tag))
                {
                    SignType &= ~(PackageSignType)toggleMenuFlyoutItem.Tag;
                }
                else
                {
                    SignType |= (PackageSignType)toggleMenuFlyoutItem.Tag;
                }

                await InitializeDataAsync();
            }
        }

        /// <summary>
        /// 刷新数据
        /// </summary>
        public async void OnRefreshClicked(object sender, RoutedEventArgs args)
        {
            MatchResultList = null;
            IsLoadedCompleted = false;
            SearchText = string.Empty;
            await Task.Delay(500);
            await GetInstalledAppsAsync();
            await InitializeDataAsync();
        }

        /// <summary>
        /// 属性值发生变化时通知更改
        /// </summary>
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// 加载系统已安装的应用信息
        /// </summary>
        private async Task GetInstalledAppsAsync()
        {
            await Task.Run(() =>
            {
                MatchResultList = PackageManager.FindPackagesForUser(string.Empty).ToList();
                if (MatchResultList is not null)
                {
                    DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Low, () =>
                    {
                        IsPackageEmpty = MatchResultList.Count is 0;
                    });
                }
            });
        }

        /// <summary>
        /// 初始化列表数据
        /// </summary>
        public async Task InitializeDataAsync(bool hasSearchText = false)
        {
            lock (AppShortListObject)
            {
                IsLoadedCompleted = false;
                AppShortList.Clear();
            }

            if (MatchResultList is not null)
            {
                await Task.Run(() =>
                {
                    // 备份数据
                    List<Package> backupList = MatchResultList;
                    List<Package> appTypesList;

                    // 根据选项是否筛选包含框架包的数据
                    if (IsFramework)
                    {
                        appTypesList = backupList.Where(item => item.IsFramework == IsFramework).ToList();
                    }
                    else
                    {
                        appTypesList = backupList;
                    }

                    List<Package> filteredList = new List<Package>();
                    if (SignType.HasFlag(PackageSignType.Store))
                    {
                        filteredList.AddRange(appTypesList.Where(item => item.SignatureKind == PackageSignatureKind.Store));
                    }

                    if (SignType.HasFlag(PackageSignType.System))
                    {
                        filteredList.AddRange(appTypesList.Where(item => item.SignatureKind == PackageSignatureKind.System));
                    }

                    if (SignType.HasFlag(PackageSignType.Enterprise))
                    {
                        filteredList.AddRange(appTypesList.Where(item => item.SignatureKind == PackageSignatureKind.Enterprise));
                    }

                    if (SignType.HasFlag(PackageSignType.Developer))
                    {
                        filteredList.AddRange(appTypesList.Where(item => item.SignatureKind == PackageSignatureKind.Developer));
                    }

                    if (SignType.HasFlag(PackageSignType.None))
                    {
                        filteredList.AddRange(appTypesList.Where(item => item.SignatureKind == PackageSignatureKind.None));
                    }

                    // 对过滤后的列表数据进行排序
                    switch (SelectedType)
                    {
                        case AppListRuleSeletedType.PackageName:
                            {
                                if (IsIncrease)
                                {
                                    filteredList = filteredList.OrderBy(item => item.DisplayName).ToList();
                                }
                                else
                                {
                                    filteredList = filteredList.OrderByDescending(item => item.DisplayName).ToList();
                                }
                                break;
                            }
                        case AppListRuleSeletedType.PublisherName:
                            {
                                if (IsIncrease)
                                {
                                    filteredList = filteredList.OrderBy(item => item.PublisherDisplayName).ToList();
                                }
                                else
                                {
                                    filteredList = filteredList.OrderByDescending(item => item.PublisherDisplayName).ToList();
                                }
                                break;
                            }
                        case AppListRuleSeletedType.InstallDate:
                            {
                                if (IsIncrease)
                                {
                                    filteredList = filteredList.OrderBy(item => item.InstalledDate).ToList();
                                }
                                else
                                {
                                    filteredList = filteredList.OrderByDescending(item => item.InstalledDate).ToList();
                                }
                                break;
                            }
                    }

                    // 根据搜索条件对搜索符合要求的数据
                    if (hasSearchText)
                    {
                        filteredList = filteredList.Where(matchItem =>
                            matchItem.DisplayName.Contains(SearchText) ||
                            matchItem.Description.Contains(SearchText) ||
                            matchItem.PublisherDisplayName.Contains(SearchText)).ToList();

                        DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Low, () =>
                        {
                            lock (AppShortListObject)
                            {
                                foreach (Package filteredItem in filteredList)
                                {
                                    try
                                    {
                                        AppShortList.Add(new PackageModel()
                                        {
                                            IsUnInstalling = false,
                                            Package = filteredItem,
                                            AppListEntryCount = filteredItem.GetAppListEntries().Count,
                                        });
                                        Task.Delay(1);
                                    }
                                    catch (Exception)
                                    {
                                        continue;
                                    }
                                }

                                IsLoadedCompleted = true;
                            }
                        });
                    }
                    else
                    {
                        DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Low, () =>
                        {
                            lock (AppShortListObject)
                            {
                                foreach (Package filteredItem in filteredList)
                                {
                                    try
                                    {
                                        AppShortList.Add(new PackageModel()
                                        {
                                            IsUnInstalling = false,
                                            Package = filteredItem,
                                            AppListEntryCount = filteredItem.GetAppListEntries().Count
                                        });
                                        Task.Delay(1);
                                    }
                                    catch (Exception)
                                    {
                                        continue;
                                    }
                                }

                                IsLoadedCompleted = true;
                            }
                        });
                    }
                });
            }
        }
    }
}
