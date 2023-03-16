<div align=center>
<img src="https://user-images.githubusercontent.com/49179966/219057775-f8d6abb5-c9c3-46c6-829e-05d164937b76.png" width="140" height="140"/>
</div>

# <p align="center">欢迎访问 获取商店应用</p>

### 语言选择（Language selection）

> * [简体中文](https://github.com/Gaoyifei1011/GetStoreApp/blob/master/Description/README_ZH-CN.md)&emsp;
> * [English](https://github.com/Gaoyifei1011/GetStoreApp/blob/master/Description/README_EN-US.md)&emsp;

------

### 应用简介

微软商店提供了对已上架商店应用的分发，下载和更新通道。但是在最新的微软商店中，微软要求用户下载商店的应用需要使用在线账户。这对一些从不使用微软账户且应用必须依赖商店下载的用户带来了困扰。因此我开发了这一款获取商店应用的APP，该应用使用了 store.rg-adguard.net 提供的获取接口，绕开了微软商店官方提供的应用下载渠道。用户可以离线下载所需的应用安装包，进行独立部署。

------

### 该应用的基础功能

> * 绕开微软商店下载并离线部署 Microsoft Store 应用
> * 访问已经成功获取的历史链接和添加的下载任务
> * 访问网页版（接口出现问题时），并使用应用内置的下载工具下载

注意：该应用不能绕过微软商店的付费渠道，如果您要获取的应用是付费应用，请从微软商店购买后下载。

------

### 应用截图

#### <p align="center">应用成功获取界面</p>
![image](https://user-images.githubusercontent.com/49179966/213072610-b0acba12-bb60-43cb-b5b1-a0270cd3b7f1.png)
#### <p align="center">历史记录</p>
![image](https://user-images.githubusercontent.com/49179966/213072877-d1be2dc7-f351-4607-809d-f23134b18ca9.png)
#### <p align="center">下载管理</p>
![image](https://user-images.githubusercontent.com/49179966/213073804-83c985f2-4917-4b5f-8c3b-f49c94b8c5a1.png)
![image](https://user-images.githubusercontent.com/49179966/213074001-02fca36d-65a1-493f-abf1-0b087e51e2fc.png)
#### <p align="center">访问网页版</p>
![image](https://user-images.githubusercontent.com/49179966/213074325-54989cde-4a2e-4876-8c31-67532d0614f9.png)
#### <p align="center">应用说明</p>
![image](https://user-images.githubusercontent.com/49179966/213076697-4a3e45f1-3474-4fde-8f72-06a0fc79b65c.png)

------

### 项目开发进展

| 项目内容                        | 开发进展                                          |
| --------------------------------| --------------------------------------------------|
| 主页面功能                      | 已完成                                            |
| 历史记录（记录使用过的链接）    | 已完成                                            |
| 通过生成的链接下载文件          | 已完成                                            |
| 下载完成后离线部署应用          | 已完成                                            |
| 访问网页版对接下载接口          | 已完成                                            |
| 控制台应用程序（快速下载）      | 已完成                                            |
| 程序性能优化                    | 已完成                                            |
| 界面现代化改造                  | 已完成                                            |
| 使用CppWinRT重新改造该应用，进一步大幅度降低应用体积，整体提升应用性能 | 预计2.0正式版本实现 |

程序所有功能都已开发完成

------

### 项目引用（按英文首字母排序）

> * [Aira2](https://aria2.github.io)&emsp;
> * [Microsoft.Windows.SDK.Contracts](https://aka.ms/WinSDKProjectURL)&emsp;
> * [Microsoft.WindowsAppSDK](https://github.com/microsoft/windowsappsdk)&emsp;
> * [Mile.Xaml](https://github.com/ProjectMile/Mile.Xaml)&emsp;

[学习过程中参考或使用的代码](https://github.com/Gaoyifei1011/GetStoreApp/blob/master/Description/StudyReferenceCode.md)&emsp;

------

### 下载与安装注意事项

> * 该程序使用的是Windows 应用 SDK构建的，建议您的系统版本为Windows 11（代号 21H2 / 内部版本号 22000）或更高版本，最低版本为Windows 10（代号2004 / 内部版本号19041）或更高版本。
> * 如果您的系统是Windows 10，应用功能存在一些限制：
    不支持设置云母/云母Alt背景色
> * [Release](https://github.com/Gaoyifei1011/GetStoreApp/releases)页面的二进制安装文件已经打包成压缩包。请解压压缩包并使用Powershell管理员模式（必要情况下）运行install.ps1文件方可实现快速安装。
> * 自行下载项目源代码并编译。（请仔细阅读下面的项目编译步骤）

------

### 项目编译步骤和应用本地化

#### <p align="center">必须安装的工具</p>

> * [Microsoft Visual Studio 2022](https://visualstudio.microsoft.com/) 
> * .NET桌面开发（Visual Studio Installer中安装，.NET SDK 版本 7.0 和 .NET Framework SDK 4.8.1）
> * [Microsoft Edge WebView2 运行时](https://developer.microsoft.com/zh-cn/microsoft-edge/webview2/) （推荐安装）

#### <p align="center">编译步骤</p>

> * 克隆项目并下载代码到本地
> * 使用Visual Studio 2022打开GetStoreApp.sln文件，如果解决方案提示部分工具没有安装，请完成安装工具步骤后再次打开该解决方案。
> * 还原项目的Nuget包。
> * 还原完成后，右键项目解决方案，生成该解决方案后点击部署解决方案。
> * 部署完成后打开“开始”菜单即可运行应用。

#### <p align="center">应用本地化</p>
##### 项目在最初仅提供简体中文和英文两种语言格式，如果您想将应用翻译到您熟悉的语言或纠正已完成翻译的内容中存在的错误，请参考下面的步骤

> * 在Description文件夹中寻找Readme模板文件，例如英文版的是README_EN-US.md文件，将其重命名为README_(对应的语言).md文件。
> * 打开重命名后的文件，翻译所有的语句后并保存。翻译完成后请您认真检查一下。
> * 打开项目主页面的README.md，在最上方的“语言选择”中添加您对应的语言。例如“英文”，注意该文字附带超链接。
> * README_(对应的语言).md文件中添加的语言截图替换为您熟悉的语言的应用截图。
> * 完成上面所述的翻译步骤，确保所有步骤能够顺利运行。
> * 打开GetStoreAppPackage打包项目，找到Package.appxmanifest文件，右键该文件，点击查看代码，找到Resources标签，根据模板添加相对应的语言，例如“<Resource Language="EN-US"/>”。
> * 打开GetStoreApp项目的Strings文件夹，并创建您使用的语言，比如（English(United States)）文件夹名称为en-us，具体可以参考表示语言(文化)代码与国家地区对照表）。
> * 打开子文件夹下的resw文件，对每一个名称进行翻译。
> * 编译运行代码并测试您的语言，应用在初次打开的时候如果没有您使用的语言默认显示English(United States)，需要在设置中动态调整。
> * 完成上述步骤后创建PR，然后将修改的内容提交到本项目，等待合并即可。

------

### 感谢（按英文首字母排序）

> * [AndromedaMelody](https://github.com/AndromedaMelody)&emsp;
> * [cnbluefire](https://github.com/cnbluefire)&emsp;
> * [飞翔](https://fionlen.azurewebsites.net)&emsp;
> * [MouriNaruto](https://github.com/MouriNaruto)&emsp;
> * [TaylorShi](https://github.com/TaylorShi)&emsp;

------

### 其他内容

> * 这是我个人在学习c#时自己动手实践的第一个小项目，由于在关于c#的高级内容中涉及的并不是很深，所以在代码内容和质量上存在着很多的欠缺，希望能多多包涵。
> * 该项目是基于MIT协议许可的开源项目，您可以修改、分发该项目或将副本与新副本合并。如果您使用了该项目，请勿用于非法用途，本开发者不会承担任何责任。

------

### 项目 Star 数量统计趋势图
[![Stargazers over time](https://starchart.cc/Gaoyifei1011/GetStoreApp.svg)](https://starchart.cc/Gaoyifei1011/GetStoreApp)


