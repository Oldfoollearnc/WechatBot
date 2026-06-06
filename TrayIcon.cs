using System.IO;
using System.Runtime.InteropServices;

namespace WechatBot;

/// <summary>
/// 纯 Win32 P/Invoke 实现的系统托盘图标（不依赖 WinForms）
/// </summary>
public class TrayIcon : IDisposable
{
    private const uint WM_TRAYICON = 0x0400 + 1; // WM_USER + 1
    private const uint WM_LBUTTONDBLCLK = 0x0203;
    private const uint WM_RBUTTONUP = 0x0205;
    private const uint NIM_ADD = 0x00;
    private const uint NIM_MODIFY = 0x01;
    private const uint NIM_DELETE = 0x02;
    private const uint NIF_MESSAGE = 0x01;
    private const uint NIF_ICON = 0x02;
    private const uint NIF_TIP = 0x04;
    private const uint NIF_INFO = 0x10;
    private const uint NIIF_INFO = 0x01;
    private const uint IDI_APPLICATION = 32512;
    private const uint IMAGE_ICON = 1;
    private const uint LR_LOADFROMFILE = 0x0010;
    private const uint MF_STRING = 0x0000;
    private const uint MF_SEPARATOR = 0x0800;
    private const uint TPM_RIGHTBUTTON = 0x0002;
    private const uint TPM_BOTTOMALIGN = 0x0020;

    // NOTIFYICONDATAW_V2 的大小（不含 hBalloonIcon，兼容所有 Windows 版本）
    private const int NOTIFYDATA_V2_SIZE = 952;

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern bool Shell_NotifyIcon(uint dwMessage, ref NOTIFYDATA lpData);

    [DllImport("user32.dll")]
    private static extern IntPtr LoadIcon(IntPtr hInstance, IntPtr lpIconName);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr LoadImage(IntPtr hInst, string name, uint type, int cx, int cy, uint fuLoad);

    [DllImport("user32.dll")]
    private static extern bool DestroyIcon(IntPtr hIcon);

    [DllImport("user32.dll")]
    private static extern bool TrackPopupMenu(IntPtr hMenu, uint uFlags, int x, int y, int nReserved, IntPtr hWnd, IntPtr prcRect);

    [DllImport("user32.dll")]
    private static extern IntPtr CreatePopupMenu();

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern bool AppendMenu(IntPtr hMenu, uint uFlags, uint uIDNewItem, string lpNewItem);

    [DllImport("user32.dll")]
    private static extern bool DestroyMenu(IntPtr hMenu);

    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out POINT lpPoint);

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct NOTIFYDATA
    {
        public int cbSize;
        public IntPtr hWnd;
        public uint uID;
        public uint uFlags;
        public uint uCallbackMessage;
        public IntPtr hIcon;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string szTip;
        public uint dwState;
        public uint dwStateMask;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string szInfo;
        public uint uTimeoutOrVersion;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string szInfoTitle;
        public uint dwInfoFlags;
        public Guid guidItem;
        public IntPtr hBalloonIcon;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT { public int X, Y; }

    private NOTIFYDATA _data;
    private IntPtr _icon;
    private IntPtr _defaultIcon;
    private IntPtr _hWnd;
    private bool _visible;
    private readonly System.Windows.Threading.Dispatcher _dispatcher;

    public event Action? OnDoubleClick;
    public event Action? OnShowClick;
    public event Action? OnExitClick;

    public TrayIcon(IntPtr hWnd, string tooltip)
    {
        _hWnd = hWnd;
        _dispatcher = System.Windows.Threading.Dispatcher.CurrentDispatcher;

        // 使用系统默认应用图标
        _defaultIcon = LoadIcon(IntPtr.Zero, (IntPtr)(long)IDI_APPLICATION);
        _icon = _defaultIcon;

        _data = new NOTIFYDATA
        {
            cbSize = NOTIFYDATA_V2_SIZE,
            hWnd = hWnd,
            uID = 1,
            uFlags = NIF_ICON | NIF_TIP | NIF_MESSAGE,
            uCallbackMessage = WM_TRAYICON,
            hIcon = _icon,
            szTip = tooltip,
        };
    }

    /// <summary>
    /// 加载自定义图标文件
    /// </summary>
    public bool LoadIconFromFile(string path)
    {
        if (!File.Exists(path)) return false;
        var hIcon = LoadImage(IntPtr.Zero, path, IMAGE_ICON, 16, 16, LR_LOADFROMFILE);
        if (hIcon == IntPtr.Zero) return false;

        if (_icon != _defaultIcon)
            DestroyIcon(_icon);
        _icon = hIcon;
        _data.hIcon = hIcon;
        if (_visible) Shell_NotifyIcon(NIM_MODIFY, ref _data);
        return true;
    }

    public void Show()
    {
        if (_visible) return;
        Shell_NotifyIcon(NIM_ADD, ref _data);
        _visible = true;
    }

    public void Hide()
    {
        if (!_visible) return;
        Shell_NotifyIcon(NIM_DELETE, ref _data);
        _visible = false;
    }

    /// <summary>
    /// 显示气泡通知
    /// </summary>
    public void ShowBalloon(string title, string text, int timeoutMs = 2000)
    {
        _data.uFlags |= NIF_INFO;
        _data.szInfoTitle = title;
        _data.szInfo = text;
        _data.dwInfoFlags = NIIF_INFO;
        _data.uTimeoutOrVersion = (uint)timeoutMs;
        if (_visible) Shell_NotifyIcon(NIM_MODIFY, ref _data);
        _data.uFlags &= ~NIF_INFO;
    }

    /// <summary>
    /// 处理托盘消息（在 WindowProc 中调用）
    /// 返回 true 表示已处理
    /// </summary>
    public bool HandleMessage(uint msg, IntPtr wParam, IntPtr lParam)
    {
        if (msg != WM_TRAYICON) return false;

        uint mouseMsg = (uint)(lParam.ToInt64() & 0xFFFF);
        if (mouseMsg == WM_LBUTTONDBLCLK)
        {
            _dispatcher.BeginInvoke(() => OnDoubleClick?.Invoke());
        }
        else if (mouseMsg == WM_RBUTTONUP)
        {
            _dispatcher.BeginInvoke(() => ShowContextMenu());
        }
        return true;
    }

    private void ShowContextMenu()
    {
        var hMenu = CreatePopupMenu();
        AppendMenu(hMenu, MF_STRING, 1, "显示主窗口");
        AppendMenu(hMenu, MF_SEPARATOR, 0, "");
        AppendMenu(hMenu, MF_STRING, 2, "退出");

        GetCursorPos(out var pt);
        SetForegroundWindow(_hWnd);
        TrackPopupMenu(hMenu, TPM_RIGHTBUTTON | TPM_BOTTOMALIGN, pt.X, pt.Y, 0, _hWnd, IntPtr.Zero);
        DestroyMenu(hMenu);
    }

    /// <summary>
    /// 处理菜单命令（WM_COMMAND）
    /// </summary>
    public bool HandleCommand(uint wParam)
    {
        switch (wParam)
        {
            case 1:
                _dispatcher.BeginInvoke(() => OnShowClick?.Invoke());
                return true;
            case 2:
                _dispatcher.BeginInvoke(() => OnExitClick?.Invoke());
                return true;
        }
        return false;
    }

    public void Dispose()
    {
        Hide();
        if (_icon != _defaultIcon && _icon != IntPtr.Zero)
        {
            DestroyIcon(_icon);
            _icon = IntPtr.Zero;
        }
    }
}
