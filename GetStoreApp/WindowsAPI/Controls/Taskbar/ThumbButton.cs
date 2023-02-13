﻿using System;
using System.Runtime.InteropServices;

namespace GetStoreApp.WindowsAPI.Controls.Taskbar
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    internal struct THUMBBUTTON
    {
        /// <summary>
        /// 单击按钮的 WPARAM 值
        /// </summary>
        internal const int Clicked = 0x1800;

        /// <summary>
        /// <see cref="THUMBBUTTONMASK"> 值的组合，指定此结构的成员包含有效数据;其他成员将被忽略，但 <see cref="iId"> 除外，这始终是必需的。
        /// </summary>
        [MarshalAs(UnmanagedType.U4)]
        internal THUMBBUTTONMASK dwMask;

        /// <summary>
        /// 按钮的应用程序定义标识符，在工具栏中是唯一的。
        /// </summary>
        internal uint iId;

        /// <summary>
        /// 通过 ITaskbarList3::ThumbBarSetImageList 设置的图像列表中的按钮图像的从零开始的索引。
        /// </summary>
        internal uint iBitmap;

        /// <summary>
        /// 用作按钮图像的图标的句柄。
        /// </summary>
        internal IntPtr hIcon;

        /// <summary>
        /// 包含按钮工具提示文本的宽字符数组，当鼠标指针悬停在按钮上时显示。
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        internal string szTip;

        /// <summary>
        /// <see cref="THUMBBUTTONFLAGS"> 值的组合，用于控制按钮的特定状态和行为
        /// </summary>
        [MarshalAs(UnmanagedType.U4)]
        internal THUMBBUTTONFLAGS dwFlags;
    }
}
