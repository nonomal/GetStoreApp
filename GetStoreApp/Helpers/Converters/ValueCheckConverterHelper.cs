using GetStoreApp.Services.Root;
using Microsoft.UI.Xaml;
using System;
using System.Diagnostics;
using System.IO;
using Windows.ApplicationModel;

namespace GetStoreApp.Helpers.Converters
{
    /// <summary>
    /// 值检查辅助类
    /// </summary>
    public static class ValueCheckConverterHelper
    {
        public static Visibility DownloadFlagCheck(int value, int checkValue)
        {
            return value == checkValue ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// 检测文件是否存在
        /// </summary>
        public static bool FileExistCheck(string path)
        {
            return File.Exists(path);
        }

        /// <summary>
        /// 检测文件是否存在（判断结果相反）
        /// </summary>
        public static bool FileExistReverseCheck(string path)
        {
            return !File.Exists(path);
        }

        /// <summary>
        /// 检测文件是否存在，进行文字提示
        /// </summary>
        public static string FileExistTextCheck(string path)
        {
            if (File.Exists(path))
            {
                return ResourceService.GetLocalized("Download/CompleteDownload");
            }
            else
            {
                return ResourceService.GetLocalized("Download/FileNotExist");
            }
        }

        /// <summary>
        /// 历史记录过滤单选框检查
        /// </summary>
        public static bool FilterValueCheck(string filterRawType, string filterValue)
        {
            return filterRawType == filterValue;
        }

        /// <summary>
        /// 安装文件按钮可用值检查
        /// </summary>
        public static bool InstallFileExistsCheck(string path)
        {
            if (path.EndsWith(".appx", StringComparison.OrdinalIgnoreCase) ||
                path.EndsWith(".msix", StringComparison.OrdinalIgnoreCase) ||
                path.EndsWith(".appxbundle", StringComparison.OrdinalIgnoreCase) ||
                path.EndsWith(".msixbundle", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 检查应用是否为商店应用
        /// </summary>
        public static bool IsStorePackage(Package packagePath)
        {
            return packagePath.SignatureKind is PackageSignatureKind.Store;
        }

        /// <summary>
        /// 历史记录按时间排序单选框检查
        /// </summary>
        public static bool TimeSortValueCheck(bool value, bool checkValue)
        {
            return value == checkValue;
        }

        /// <summary>
        /// 检测当前页面是否为应用列表页面
        /// </summary>
        public static Visibility IsAppListPageCheck(int count)
        {
            return count is 1 ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// 检查应用图标是否存在，不存在返回空图标
        /// </summary>
        public static Uri GetAppLogo(Package packagePath)
        {
            try
            {
                return packagePath.Logo;
            }
            catch
            {
                return null;
            }
        }
    }
}
