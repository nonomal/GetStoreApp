﻿using GetStoreApp.Helpers.Root;
using GetStoreApp.Models.Controls.Store;
using GetStoreApp.Properties;
using GetStoreApp.Services.Root;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Data.Xml.Dom;
using Windows.Foundation.Diagnostics;
using Windows.Web.Http;
using Windows.Web.Http.Headers;

namespace GetStoreApp.Helpers.Controls.Store
{
    public static class QueryLinksHelper
    {
        private static string market = CultureInfo.InstalledUICulture.Name.Remove(0, 3).ToUpper();
        private static string locale = CultureInfo.InstalledUICulture.Name.ToLower();

        private static Uri CookieUri = new Uri("https://fe3.delivery.mp.microsoft.com/ClientWebService/client.asmx");
        private static Uri FileListXmlUri = new Uri("https://fe3.delivery.mp.microsoft.com/ClientWebService/client.asmx");
        private static Uri UrlUri = new Uri("https://fe3.delivery.mp.microsoft.com/ClientWebService/client.asmx/secured");

        /// <summary>
        /// 解析输入框输入的字符串
        /// </summary>
        public static string ParseRequestContent(string requestContent)
        {
            if (requestContent.Contains('/'))
            {
                requestContent = requestContent.Remove(0, requestContent.LastIndexOf('/') + 1);
            }
            if (requestContent.Contains('?'))
            {
                requestContent = requestContent.Remove(requestContent.IndexOf('?'));
            }
            return requestContent;
        }

        /// <summary>
        /// 获取微软商店服务器储存在用户本地终端上的数据
        /// </summary>
        public static async Task<string> GetCookieAsync()
        {
            // 添加超时设置（半分钟后停止获取）
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(30));

            string cookieResult = string.Empty;

            try
            {
                byte[] contentBytes = Encoding.UTF8.GetBytes(Resources.cookie);

                HttpStringContent httpStringContent = new HttpStringContent(Resources.cookie);
                httpStringContent.Headers.Expires = DateTime.Now;
                httpStringContent.Headers.ContentType = new HttpMediaTypeHeaderValue("application/soap+xml");
                httpStringContent.Headers.ContentLength = Convert.ToUInt64(contentBytes.Length);
                httpStringContent.Headers.ContentType.CharSet = "utf-8";

                HttpClient httpClient = new HttpClient();
                HttpResponseMessage responseMessage = await httpClient.PostAsync(CookieUri, httpStringContent).AsTask(cancellationTokenSource.Token);

                // 请求成功
                if (responseMessage.IsSuccessStatusCode)
                {
                    StringBuilder cookieResponseBuilder = new StringBuilder();

                    cookieResponseBuilder.Append("Status Code:");
                    cookieResponseBuilder.AppendLine(responseMessage.StatusCode.ToString());
                    cookieResponseBuilder.Append("Headers:");
                    cookieResponseBuilder.AppendLine(responseMessage.Headers is null ? "" : responseMessage.Headers.ToString().Replace('\r', ' ').Replace('\n', ' '));
                    cookieResponseBuilder.Append("ResponseMessage:");
                    cookieResponseBuilder.AppendLine(responseMessage.RequestMessage is null ? "" : responseMessage.RequestMessage.ToString().Replace('\r', ' ').Replace('\n', ' '));

                    LogService.WriteLog(LoggingLevel.Information, "Cookie request successfully.", cookieResponseBuilder);

                    string responseString = await responseMessage.Content.ReadAsStringAsync();
                    httpClient.Dispose();
                    responseMessage.Dispose();

                    XmlDocument responseStringDocument = new XmlDocument();
                    responseStringDocument.LoadXml(responseString);

                    XmlNodeList encryptedDataList = responseStringDocument.GetElementsByTagName("EncryptedData");
                    if (encryptedDataList.Count > 0)
                    {
                        cookieResult = encryptedDataList[0].InnerText;
                    }
                }
            }
            // 捕捉因为网络失去链接获取信息时引发的异常
            catch (COMException e)
            {
                LogService.WriteLog(LoggingLevel.Information, "Cookie request failed", e);
            }
            // 捕捉因访问超时引发的异常
            catch (TaskCanceledException e)
            {
                LogService.WriteLog(LoggingLevel.Information, "Cookie request timeout", e);
            }
            // 其他异常
            catch (Exception e)
            {
                LogService.WriteLog(LoggingLevel.Warning, "Cookie request unknown exception", e);
            }
            finally
            {
                cancellationTokenSource.Dispose();
            }

            return cookieResult;
        }

        /// <summary>
        /// 获取应用信息
        /// </summary>
        /// <param name="productId">应用的产品 ID</param>
        /// <returns>打包应用：有，非打包应用：无</returns>
        public static async Task<Tuple<bool, AppInfoModel>> GetAppInformationAsync(string productId)
        {
            // 添加超时设置（半分钟后停止获取）
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(30));

            Tuple<bool, AppInfoModel> appInformationResult = null;
            AppInfoModel appInfoModel = new AppInfoModel();

            try
            {
                string categoryIDAPI = string.Format("https://storeedgefd.dsx.mp.microsoft.com/v9.0/products/{0}?market={1}&locale={2}&deviceFamily=Windows.Desktop", productId, market, locale);

                HttpClient httpClient = new HttpClient();
                HttpResponseMessage responseMessage = await httpClient.GetAsync(new Uri(categoryIDAPI)).AsTask(cancellationTokenSource.Token);

                // 请求成功
                if (responseMessage.IsSuccessStatusCode)
                {
                    StringBuilder cookieResponseBuilder = new StringBuilder();

                    cookieResponseBuilder.Append("Status Code:");
                    cookieResponseBuilder.AppendLine(responseMessage.StatusCode.ToString());
                    cookieResponseBuilder.Append("Headers:");
                    cookieResponseBuilder.AppendLine(responseMessage.Headers is null ? "" : responseMessage.Headers.ToString().Replace('\r', ' ').Replace('\n', ' '));
                    cookieResponseBuilder.Append("ResponseMessage:");
                    cookieResponseBuilder.AppendLine(responseMessage.RequestMessage is null ? "" : responseMessage.RequestMessage.ToString().Replace('\r', ' ').Replace('\n', ' '));

                    LogService.WriteLog(LoggingLevel.Information, "App Information request successfully.", cookieResponseBuilder);

                    string responseString = await responseMessage.Content.ReadAsStringAsync();
                    httpClient.Dispose();
                    responseMessage.Dispose();

                    if (JsonObject.TryParse(responseString, out JsonObject responseStringObject))
                    {
                        JsonObject payLoadObject = responseStringObject.GetNamedValue("Payload").GetObject();

                        appInfoModel.Name = payLoadObject.GetNamedString("Title");
                        appInfoModel.Publisher = payLoadObject.GetNamedString("PublisherName");
                        appInfoModel.Description = payLoadObject.GetNamedString("Description");
                        appInfoModel.CategoryID = string.Empty;

                        JsonArray skusArray = payLoadObject.GetNamedArray("Skus");

                        if (skusArray.Count > 0)
                        {
                            try
                            {
                                string fulfillmentData = skusArray[0].GetObject().GetNamedValue("FulfillmentData").GetString();
                                if (JsonObject.TryParse(fulfillmentData, out JsonObject fulfillmentDataObject))
                                {
                                    appInfoModel.CategoryID = fulfillmentDataObject.GetNamedString("WuCategoryId");
                                }
                            }
                            catch
                            {
                            }
                            appInformationResult = new Tuple<bool, AppInfoModel>(true, appInfoModel);
                        }
                    }
                }
            }

            // 捕捉因为网络失去链接获取信息时引发的异常
            catch (COMException e)
            {
                LogService.WriteLog(LoggingLevel.Information, "App Information request failed", e);
            }

            // 捕捉因访问超时引发的异常
            catch (TaskCanceledException e)
            {
                LogService.WriteLog(LoggingLevel.Information, "App Information request timeout", e);
            }

            // 其他异常
            catch (Exception e)
            {
                LogService.WriteLog(LoggingLevel.Warning, "App Information request unknown exception", e);
            }
            finally
            {
                cancellationTokenSource.Dispose();
            }

            return appInformationResult is null ? new Tuple<bool, AppInfoModel>(false, appInfoModel) : appInformationResult;
        }

        /// <summary>
        /// 获取文件信息字符串
        /// </summary>
        /// <param name="cookie">cookie 数据</param>
        /// <param name="categoryId">category ID</param>
        /// <param name="ring">通道</param>
        /// <returns>文件信息的字符串</returns>
        public static async Task<string> GetFileListXmlAsync(string cookie, string categoryId, string ring)
        {
            // 添加超时设置（半分钟后停止获取）
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(30));

            string fileListXmlResult = string.Empty;

            try
            {
                string fileListXml = Resources.wu.Replace("{1}", cookie).Replace("{2}", categoryId).Replace("{3}", ring);
                byte[] contentBytes = Encoding.UTF8.GetBytes(fileListXml);

                HttpStringContent httpStringContent = new HttpStringContent(fileListXml);
                httpStringContent.Headers.Expires = DateTime.Now;
                httpStringContent.Headers.ContentType = new HttpMediaTypeHeaderValue("application/soap+xml");
                httpStringContent.Headers.ContentLength = Convert.ToUInt64(contentBytes.Length);
                httpStringContent.Headers.ContentType.CharSet = "utf-8";

                HttpClient httpClient = new HttpClient();
                HttpResponseMessage responseMessage = await httpClient.PostAsync(FileListXmlUri, httpStringContent).AsTask(cancellationTokenSource.Token);

                // 请求成功
                if (responseMessage.IsSuccessStatusCode)
                {
                    StringBuilder cookieResponseBuilder = new StringBuilder();

                    cookieResponseBuilder.Append("Status Code:");
                    cookieResponseBuilder.AppendLine(responseMessage.StatusCode.ToString());
                    cookieResponseBuilder.Append("Headers:");
                    cookieResponseBuilder.AppendLine(responseMessage.Headers is null ? "" : responseMessage.Headers.ToString().Replace('\r', ' ').Replace('\n', ' '));
                    cookieResponseBuilder.Append("ResponseMessage:");
                    cookieResponseBuilder.AppendLine(responseMessage.RequestMessage is null ? "" : responseMessage.RequestMessage.ToString().Replace('\r', ' ').Replace('\n', ' '));

                    LogService.WriteLog(LoggingLevel.Information, "FileListXml request successfully.", cookieResponseBuilder);

                    string responseString = await responseMessage.Content.ReadAsStringAsync();
                    httpClient.Dispose();
                    responseMessage.Dispose();

                    fileListXmlResult = responseString.Replace("&lt;", "<").Replace("&gt;", ">");
                }
            }
            // 捕捉因为网络失去链接获取信息时引发的异常
            catch (COMException e)
            {
                LogService.WriteLog(LoggingLevel.Information, "FileListXml request failed", e);
            }
            // 捕捉因访问超时引发的异常
            catch (TaskCanceledException e)
            {
                LogService.WriteLog(LoggingLevel.Information, "FileListXml request timeout", e);
            }
            // 其他异常
            catch (Exception e)
            {
                LogService.WriteLog(LoggingLevel.Warning, "Network state unknown exception", e);
            }
            finally
            {
                cancellationTokenSource.Dispose();
            }

            return fileListXmlResult;
        }

        /// <summary>
        /// 获取商店应用安装包
        /// </summary>
        /// <param name="fileListXml">文件信息的字符串</param>
        /// <param name="ring">通道</param>
        /// <returns>带解析后文件信息的列表</returns>
        public static List<ResultModel> GetAppxPackages(string fileListXml, string ring)
        {
            List<ResultModel> appxPackagesList = new List<ResultModel>();

            try
            {
                XmlDocument fileListDocument = new XmlDocument();
                fileListDocument.LoadXml(fileListXml);

                Dictionary<string, Tuple<string, string, string>> appxPackagesInfoDict = new Dictionary<string, Tuple<string, string, string>>();
                XmlNodeList fileList = fileListDocument.GetElementsByTagName("File");

                foreach (IXmlNode fileNode in fileList)
                {
                    if (fileNode.Attributes.GetNamedItem("InstallerSpecificIdentifier") is not null)
                    {
                        string name = fileNode.Attributes.GetNamedItem("InstallerSpecificIdentifier").InnerText;
                        string extension = fileNode.Attributes.GetNamedItem("FileName").InnerText.Remove(0, fileNode.Attributes.GetNamedItem("FileName").InnerText.LastIndexOf('.'));
                        string size = fileNode.Attributes.GetNamedItem("Size").InnerText;
                        string digest = fileNode.Attributes.GetNamedItem("Digest").InnerText;

                        if (!appxPackagesInfoDict.ContainsKey(name))
                        {
                            appxPackagesInfoDict.Add(name, new Tuple<string, string, string>(extension, size, digest));
                        }
                    }
                }

                object appxPackagesLock = new object();
                XmlNodeList securedFragmentList = fileListDocument.DocumentElement.GetElementsByTagName("SecuredFragment");
                CountdownEvent countdownEvent = new CountdownEvent(securedFragmentList.Count);

                foreach (IXmlNode securedFragmentNode in securedFragmentList)
                {
                    IXmlNode securedFragmentCloneNode = securedFragmentNode;
                    Task.Run(async () =>
                    {
                        ;
                        IXmlNode xmlNode = securedFragmentCloneNode.ParentNode.ParentNode;

                        if (xmlNode.GetElementsByName("ApplicabilityRules").GetElementsByName("Metadata").GetElementsByName("AppxPackageMetadata").GetElementsByName("AppxMetadata").Attributes.GetNamedItem("PackageMoniker") is not null)
                        {
                            string name = xmlNode.GetElementsByName("ApplicabilityRules").GetElementsByName("Metadata").GetElementsByName("AppxPackageMetadata").GetElementsByName("AppxMetadata").Attributes.GetNamedItem("PackageMoniker").InnerText;

                            if (appxPackagesInfoDict.TryGetValue(name, out Tuple<string, string, string> value))
                            {
                                string fileName = name + value.Item1;
                                string fileSize = value.Item2;
                                string digest = value.Item3;
                                string revisionNumber = xmlNode.GetElementsByName("UpdateIdentity").Attributes.GetNamedItem("RevisionNumber").InnerText;
                                string updateID = xmlNode.GetElementsByName("UpdateIdentity").Attributes.GetNamedItem("UpdateID").InnerText;
                                string uri = await GetAppxUrlAsync(updateID, revisionNumber, ring, digest);
                                string fileSizeString = FileSizeHelper.ConvertFileSizeToString(double.TryParse(fileSize, out double size) == true ? size : 0);

                                lock (appxPackagesLock)
                                {
                                    appxPackagesList.Add(new ResultModel()
                                    {
                                        FileName = fileName,
                                        FileLink = uri,
                                        FileSize = fileSizeString,
                                        IsSelected = false,
                                        IsSelectMode = false
                                    });
                                    countdownEvent.Signal();
                                }
                            }
                            else
                            {
                                countdownEvent.Signal();
                            }
                        }
                    });
                }

                countdownEvent.Wait();
                countdownEvent.Dispose();
            }
            catch (Exception e)
            {
                LogService.WriteLog(LoggingLevel.Information, "FileListXml parse failed", e);
            }

            return appxPackagesList;
        }

        /// <summary>
        /// 获取商店应用安装包对应的下载链接
        /// </summary>
        /// <returns>商店应用安装包对应的下载链接</returns>
        private static async Task<string> GetAppxUrlAsync(string updateID, string revisionNumber, string ring, string digest)
        {
            // 添加超时设置（半分钟后停止获取）
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(30));

            string urlResult = string.Empty;

            try
            {
                string url = Resources.url.Replace("{1}", updateID).Replace("{2}", revisionNumber).Replace("{3}", ring);
                byte[] contentBytes = Encoding.UTF8.GetBytes(url);

                HttpStringContent httpContent = new HttpStringContent(url);
                httpContent.Headers.Expires = DateTime.Now;
                httpContent.Headers.ContentType = new HttpMediaTypeHeaderValue("application/soap+xml");
                httpContent.Headers.ContentLength = Convert.ToUInt64(contentBytes.Length);
                httpContent.Headers.ContentType.CharSet = "utf-8";

                HttpClient httpClient = new HttpClient();
                HttpResponseMessage responseMessage = await httpClient.PostAsync(UrlUri, httpContent).AsTask(cancellationTokenSource.Token);

                // 请求成功
                if (responseMessage.IsSuccessStatusCode)
                {
                    StringBuilder cookieResponseBuilder = new StringBuilder();

                    cookieResponseBuilder.Append("Status Code:");
                    cookieResponseBuilder.AppendLine(responseMessage.StatusCode.ToString());
                    cookieResponseBuilder.Append("Headers:");
                    cookieResponseBuilder.AppendLine(responseMessage.Headers is null ? "" : responseMessage.Headers.ToString().Replace('\r', ' ').Replace('\n', ' '));
                    cookieResponseBuilder.Append("ResponseMessage:");
                    cookieResponseBuilder.AppendLine(responseMessage.RequestMessage is null ? "" : responseMessage.RequestMessage.ToString().Replace('\r', ' ').Replace('\n', ' '));

                    LogService.WriteLog(LoggingLevel.Information, "Appx Url request successfully.", cookieResponseBuilder);

                    string responseString = await responseMessage.Content.ReadAsStringAsync();
                    httpClient.Dispose();
                    responseMessage.Dispose();

                    XmlDocument responseStringDocument = new XmlDocument();
                    responseStringDocument.LoadXml(responseString);

                    XmlNodeList fileLocationList = responseStringDocument.DocumentElement.GetElementsByTagName("FileLocation");

                    foreach (IXmlNode fileLocationNode in fileLocationList)
                    {
                        if (fileLocationNode.GetElementsByName("FileDigest").InnerText.Equals(digest))
                        {
                            urlResult = fileLocationNode.GetElementsByName("Url").InnerText;
                            break;
                        }
                    }
                }
            }
            // 捕捉因为网络失去链接获取信息时引发的异常
            catch (COMException e)
            {
                LogService.WriteLog(LoggingLevel.Information, "Appx Url request failed", e);
            }
            // 捕捉因访问超时引发的异常
            catch (TaskCanceledException e)
            {
                LogService.WriteLog(LoggingLevel.Information, "Appx Url request timeout", e);
            }
            // 其他异常
            catch (Exception e)
            {
                LogService.WriteLog(LoggingLevel.Warning, "Appx Url request unknown exception", e);
            }
            finally
            {
                cancellationTokenSource.Dispose();
            }

            return urlResult;
        }

        /// <summary>
        /// 获取非商店应用的安装包
        /// </summary>
        /// <param name="productId">产品 ID</param>
        /// <returns>带解析后文件信息的列表</returns>
        public static async Task<List<ResultModel>> GetNonAppxPackagesAsync(string productId)
        {
            // 添加超时设置（半分钟后停止获取）
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(30));

            List<ResultModel> nonAppxPackagesList = new List<ResultModel>();

            try
            {
                string url = "https://storeedgefd.dsx.mp.microsoft.com/v9.0/packageManifests/" + productId;

                HttpClient httpClient = new HttpClient();
                HttpResponseMessage responseMessage = await httpClient.GetAsync(new Uri(url)).AsTask(cancellationTokenSource.Token);

                // 请求成功
                if (responseMessage.IsSuccessStatusCode)
                {
                    StringBuilder cookieResponseBuilder = new StringBuilder();

                    cookieResponseBuilder.Append("Status Code:");
                    cookieResponseBuilder.AppendLine(responseMessage.StatusCode.ToString());
                    cookieResponseBuilder.Append("Headers:");
                    cookieResponseBuilder.AppendLine(responseMessage.Headers is null ? "" : responseMessage.Headers.ToString().Replace('\r', ' ').Replace('\n', ' '));
                    cookieResponseBuilder.Append("ResponseMessage:");
                    cookieResponseBuilder.AppendLine(responseMessage.RequestMessage is null ? "" : responseMessage.RequestMessage.ToString().Replace('\r', ' ').Replace('\n', ' '));

                    LogService.WriteLog(LoggingLevel.Information, "Non Appx Url request successfully.", cookieResponseBuilder);

                    string responseString = await responseMessage.Content.ReadAsStringAsync();
                    httpClient.Dispose();
                    responseMessage.Dispose();

                    if (JsonObject.TryParse(responseString, out JsonObject responseStringObject))
                    {
                        JsonObject dataObject = responseStringObject.GetNamedValue("Data").GetObject();
                        JsonObject versionsObject = dataObject.GetNamedValue("Versions").GetArray()[0].GetObject();
                        JsonArray installersArray = versionsObject.GetNamedValue("Installers").GetArray();

                        object nonAppxPackagesLock = new object();
                        CountdownEvent countdownEvent = new CountdownEvent(installersArray.Count);

                        foreach (IJsonValue installer in installersArray)
                        {
                            JsonObject installerObject = installer.GetObject();

                            string installerType = installerObject.GetNamedString("InstallerType");
                            string installerUrl = installerObject.GetNamedString("InstallerUrl");
                            string fileSize = await GetNonAppxPackageFileSizeAsync(installerUrl);
                            string fileSizeString = FileSizeHelper.ConvertFileSizeToString(double.TryParse(fileSize, out double size) == true ? size : 0);

                            if (installerType.Equals(string.Empty) || installerUrl.ToLower().EndsWith(".exe") || installerUrl.ToLower().EndsWith(".msi"))
                            {
                                lock (nonAppxPackagesLock)
                                {
                                    nonAppxPackagesList.Add(new ResultModel()
                                    {
                                        FileName = installerUrl.Remove(installerUrl.LastIndexOf('.')).Remove(0, installerUrl.LastIndexOf('/') + 1),
                                        FileLink = installerUrl,
                                        FileSize = fileSizeString,
                                        IsSelected = false,
                                        IsSelectMode = false,
                                    });
                                    countdownEvent.Signal();
                                }
                            }
                            else
                            {
                                string name = string.Empty;
                                JsonArray appsAndFeaturesEntriesArray = installerObject.GetNamedValue("AppsAndFeaturesEntries").GetArray();
                                if (appsAndFeaturesEntriesArray.Count > 0)
                                {
                                    if (appsAndFeaturesEntriesArray[0].GetObject().GetNamedString("DisplayName") is not null)
                                    {
                                        name = appsAndFeaturesEntriesArray[0].GetObject().GetNamedString("DisplayName");
                                    }
                                }
                                else if (versionsObject.GetNamedValue("DefaultLocale").GetObject() is not null && versionsObject.GetNamedValue("DefaultLocale").GetObject().GetNamedString("PackageName") is not null)
                                {
                                    name = versionsObject.GetNamedValue("DefaultLocale").GetObject().GetNamedString("PackageName");
                                }

                                lock (nonAppxPackagesLock)
                                {
                                    nonAppxPackagesList.Add(new ResultModel()
                                    {
                                        FileName = string.Format("{0} ({1}).{2}", name, installerObject.GetNamedString("InstallerLocale"), installerType),
                                        FileLink = installerUrl,
                                        FileSize = fileSizeString,
                                        IsSelected = false,
                                        IsSelectMode = false,
                                    });
                                    countdownEvent.Signal();
                                }
                            }
                        }

                        countdownEvent.Wait();
                        countdownEvent.Dispose();
                    }
                }
            }
            // 捕捉因为网络失去链接获取信息时引发的异常
            catch (COMException e)
            {
                LogService.WriteLog(LoggingLevel.Information, "Non Appx Url request failed", e);
            }
            // 捕捉因访问超时引发的异常
            catch (TaskCanceledException e)
            {
                LogService.WriteLog(LoggingLevel.Information, "Non Appx Url request timeout", e);
            }
            // 其他异常
            catch (Exception e)
            {
                LogService.WriteLog(LoggingLevel.Warning, "Non Appx Url request unknown exception", e);
            }
            finally
            {
                cancellationTokenSource.Dispose();
            }

            return nonAppxPackagesList;
        }

        /// <summary>
        /// 获取非商店应用下载文件的大小
        /// </summary>
        private static async Task<string> GetNonAppxPackageFileSizeAsync(string url)
        {
            // 添加超时设置（半分钟后停止获取）
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(30));

            string fileSizeResult = string.Empty;

            try
            {
                HttpClient httpClient = new HttpClient();
                HttpResponseMessage responseMessage = await httpClient.GetAsync(new Uri(url)).AsTask(cancellationTokenSource.Token);

                // 请求成功
                if (responseMessage.IsSuccessStatusCode)
                {
                    StringBuilder cookieResponseBuilder = new StringBuilder();

                    cookieResponseBuilder.Append("Status Code:");
                    cookieResponseBuilder.AppendLine(responseMessage.StatusCode.ToString());
                    cookieResponseBuilder.Append("Headers:");
                    cookieResponseBuilder.AppendLine(responseMessage.Headers is null ? "" : responseMessage.Headers.ToString().Replace('\r', ' ').Replace('\n', ' '));
                    cookieResponseBuilder.Append("ResponseMessage:");
                    cookieResponseBuilder.AppendLine(responseMessage.RequestMessage is null ? "" : responseMessage.RequestMessage.ToString().Replace('\r', ' ').Replace('\n', ' '));

                    LogService.WriteLog(LoggingLevel.Information, "Non appx package file size request successfully.", cookieResponseBuilder);

                    fileSizeResult = Convert.ToString(responseMessage.Content.Headers.ContentLength);
                    httpClient.Dispose();
                    responseMessage.Dispose();
                }
            }
            // 捕捉因为网络失去链接获取信息时引发的异常
            catch (COMException e)
            {
                LogService.WriteLog(LoggingLevel.Information, "FileListXml request failed", e);
            }
            // 捕捉因访问超时引发的异常
            catch (TaskCanceledException e)
            {
                LogService.WriteLog(LoggingLevel.Information, "FileListXml request timeout", e);
            }
            // 其他异常
            catch (Exception e)
            {
                LogService.WriteLog(LoggingLevel.Warning, "Network state unknown exception", e);
            }
            finally
            {
                cancellationTokenSource.Dispose();
            }

            return fileSizeResult;
        }

        private static IXmlNode GetElementsByName(this IXmlNode xmlNode, string name)
        {
            foreach (IXmlNode node in xmlNode.ChildNodes)
            {
                if (node.NodeName == name)
                {
                    return node;
                }
            }

            return null;
        }
    }
}
