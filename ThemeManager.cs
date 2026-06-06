using System.Windows;
using System.Windows.Media;

namespace WechatBot;

/// <summary>
/// 主题管理器：运行时切换深色/浅色毛玻璃主题
/// </summary>
public static class ThemeManager
{
    private static ResourceDictionary? _currentTheme;

    public static void ApplyTheme(string themeName)
    {
        var app = Application.Current;
        if (app == null) return;

        // 移除旧主题
        if (_currentTheme != null)
            app.Resources.MergedDictionaries.Remove(_currentTheme);

        _currentTheme = themeName switch
        {
            "Light" => CreateLightTheme(),
            _ => CreateDarkTheme()
        };

        app.Resources.MergedDictionaries.Add(_currentTheme);
    }

    private static ResourceDictionary CreateDarkTheme()
    {
        var rd = new ResourceDictionary();

        // 背景层
        rd["BgBrush"] = new SolidColorBrush(ColorFromHex("#F018181B"));
        rd["BgDeepBrush"] = new SolidColorBrush(ColorFromHex("#F0141416"));
        rd["SurfaceBrush"] = new SolidColorBrush(ColorFromHex("#CC2A2A2E"));
        rd["SurfaceHoverBrush"] = new SolidColorBrush(ColorFromHex("#DD3A3A3E"));
        rd["SurfaceBorderBrush"] = new SolidColorBrush(ColorFromHex("#33FFFFFF"));
        rd["CardBrush"] = new SolidColorBrush(ColorFromHex("#AA2A2A2E"));
        rd["CardHoverBrush"] = new SolidColorBrush(ColorFromHex("#CC353540"));

        // 强调色
        rd["AccentBrush"] = new SolidColorBrush(ColorFromHex("#FF00CC6A"));
        rd["AccentHoverBrush"] = new SolidColorBrush(ColorFromHex("#FF00E678"));
        rd["AccentDimBrush"] = new SolidColorBrush(ColorFromHex("#FF1A3A2A"));
        rd["AccentGlowBrush"] = new SolidColorBrush(ColorFromHex("#4400CC6A"));

        // 功能色
        rd["DangerBrush"] = new SolidColorBrush(ColorFromHex("#FFCC4444"));
        rd["DangerDimBrush"] = new SolidColorBrush(ColorFromHex("#FF3A2222"));
        rd["WarningBrush"] = new SolidColorBrush(ColorFromHex("#FFCCAA00"));
        rd["WarningDimBrush"] = new SolidColorBrush(ColorFromHex("#FF3A3522"));
        rd["InfoBrush"] = new SolidColorBrush(ColorFromHex("#FF4488CC"));
        rd["InfoDimBrush"] = new SolidColorBrush(ColorFromHex("#FF1A2A3A"));

        // 文字
        rd["TextPrimaryBrush"] = new SolidColorBrush(ColorFromHex("#FFEEEEEE"));
        rd["TextSecondaryBrush"] = new SolidColorBrush(ColorFromHex("#FFBBBBBB"));
        rd["TextTertiaryBrush"] = new SolidColorBrush(ColorFromHex("#FF888888"));
        rd["TextDisabledBrush"] = new SolidColorBrush(ColorFromHex("#FF555558"));

        // 导航栏
        rd["NavBgBrush"] = new SolidColorBrush(ColorFromHex("#EE252528"));
        rd["NavItemHoverBrush"] = new SolidColorBrush(ColorFromHex("#33FFFFFF"));
        rd["NavIndicatorBrush"] = new SolidColorBrush(ColorFromHex("#FF00CC6A"));

        // 状态栏
        rd["StatusBarBrush"] = new SolidColorBrush(ColorFromHex("#EE252528"));

        // 日志 Tag
        rd["TagInfoBrush"] = new SolidColorBrush(ColorFromHex("#3300CC6A"));
        rd["TagWarnBrush"] = new SolidColorBrush(ColorFromHex("#33CCAA00"));
        rd["TagErrorBrush"] = new SolidColorBrush(ColorFromHex("#33CC4444"));

        // 输入框
        rd["InputBgBrush"] = new SolidColorBrush(ColorFromHex("#FF3D3D40"));
        rd["InputBorderBrush"] = new SolidColorBrush(ColorFromHex("#FF555558"));
        rd["InputFocusBrush"] = new SolidColorBrush(ColorFromHex("#FF00CC6A"));

        // 滚动条
        rd["ScrollThumbBrush"] = new SolidColorBrush(ColorFromHex("#44FFFFFF"));
        rd["ScrollThumbHoverBrush"] = new SolidColorBrush(ColorFromHex("#66FFFFFF"));

        // 分隔线
        rd["SeparatorBrush"] = new SolidColorBrush(ColorFromHex("#FF3A3A3E"));

        // HandyControl 深色皮肤
        rd.MergedDictionaries.Add(new ResourceDictionary
        {
            Source = new Uri("pack://application:,,,/HandyControl;component/Themes/SkinDark.xaml")
        });
        rd.MergedDictionaries.Add(new ResourceDictionary
        {
            Source = new Uri("pack://application:,,,/HandyControl;component/Themes/Theme.xaml")
        });

        return rd;
    }

    private static ResourceDictionary CreateLightTheme()
    {
        var rd = new ResourceDictionary();

        // 背景层
        rd["BgBrush"] = new SolidColorBrush(ColorFromHex("#F0F5F5F5"));
        rd["BgDeepBrush"] = new SolidColorBrush(ColorFromHex("#F0EEEEEE"));
        rd["SurfaceBrush"] = new SolidColorBrush(ColorFromHex("#CCFFFFFF"));
        rd["SurfaceHoverBrush"] = new SolidColorBrush(ColorFromHex("#DDF0F0F0"));
        rd["SurfaceBorderBrush"] = new SolidColorBrush(ColorFromHex("#22000000"));
        rd["CardBrush"] = new SolidColorBrush(ColorFromHex("#AAFFFFFF"));
        rd["CardHoverBrush"] = new SolidColorBrush(ColorFromHex("#CCF8F8F8"));

        // 强调色
        rd["AccentBrush"] = new SolidColorBrush(ColorFromHex("#FF00A854"));
        rd["AccentHoverBrush"] = new SolidColorBrush(ColorFromHex("#FF00CC6A"));
        rd["AccentDimBrush"] = new SolidColorBrush(ColorFromHex("#FFE8F5E9"));
        rd["AccentGlowBrush"] = new SolidColorBrush(ColorFromHex("#3300A854"));

        // 功能色
        rd["DangerBrush"] = new SolidColorBrush(ColorFromHex("#FFD32F2F"));
        rd["DangerDimBrush"] = new SolidColorBrush(ColorFromHex("#FFFCE4EC"));
        rd["WarningBrush"] = new SolidColorBrush(ColorFromHex("#FFF9A825"));
        rd["WarningDimBrush"] = new SolidColorBrush(ColorFromHex("#FFFFF8E1"));
        rd["InfoBrush"] = new SolidColorBrush(ColorFromHex("#FF1976D2"));
        rd["InfoDimBrush"] = new SolidColorBrush(ColorFromHex("#FFE3F2FD"));

        // 文字
        rd["TextPrimaryBrush"] = new SolidColorBrush(ColorFromHex("#FF1A1A1A"));
        rd["TextSecondaryBrush"] = new SolidColorBrush(ColorFromHex("#FF555555"));
        rd["TextTertiaryBrush"] = new SolidColorBrush(ColorFromHex("#FF888888"));
        rd["TextDisabledBrush"] = new SolidColorBrush(ColorFromHex("#FFBBBBBB"));

        // 导航栏
        rd["NavBgBrush"] = new SolidColorBrush(ColorFromHex("#EEFFFFFF"));
        rd["NavItemHoverBrush"] = new SolidColorBrush(ColorFromHex("#11000000"));
        rd["NavIndicatorBrush"] = new SolidColorBrush(ColorFromHex("#FF00A854"));

        // 状态栏
        rd["StatusBarBrush"] = new SolidColorBrush(ColorFromHex("#EEFFFFFF"));

        // 日志 Tag
        rd["TagInfoBrush"] = new SolidColorBrush(ColorFromHex("#2200A854"));
        rd["TagWarnBrush"] = new SolidColorBrush(ColorFromHex("#22F9A825"));
        rd["TagErrorBrush"] = new SolidColorBrush(ColorFromHex("#22D32F2F"));

        // 输入框
        rd["InputBgBrush"] = new SolidColorBrush(ColorFromHex("#FFF0F0F0"));
        rd["InputBorderBrush"] = new SolidColorBrush(ColorFromHex("#FFD0D0D0"));
        rd["InputFocusBrush"] = new SolidColorBrush(ColorFromHex("#FF00A854"));

        // 滚动条
        rd["ScrollThumbBrush"] = new SolidColorBrush(ColorFromHex("#22000000"));
        rd["ScrollThumbHoverBrush"] = new SolidColorBrush(ColorFromHex("#44000000"));

        // 分隔线
        rd["SeparatorBrush"] = new SolidColorBrush(ColorFromHex("#FFE0E0E0"));

        // HandyControl 浅色皮肤
        rd.MergedDictionaries.Add(new ResourceDictionary
        {
            Source = new Uri("pack://application:,,,/HandyControl;component/Themes/SkinDefault.xaml")
        });
        rd.MergedDictionaries.Add(new ResourceDictionary
        {
            Source = new Uri("pack://application:,,,/HandyControl;component/Themes/Theme.xaml")
        });

        return rd;
    }

    private static Color ColorFromHex(string hex)
    {
        return (Color)ColorConverter.ConvertFromString(hex);
    }
}
