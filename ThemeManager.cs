using System.Windows;
using System.Windows.Media;

namespace WechatBot;

/// <summary>
/// Theme manager: runtime switch between dark/light themes
/// </summary>
public static class ThemeManager
{
    private static ResourceDictionary? _currentTheme;

    public static void ApplyTheme(string themeName)
    {
        var app = Application.Current;
        if (app == null) return;

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

        // Background layers
        rd["BgBrush"] = new SolidColorBrush(ColorFromHex("#F0111113"));
        rd["BgDeepBrush"] = new SolidColorBrush(ColorFromHex("#F00D0D0F"));
        rd["SurfaceBrush"] = new SolidColorBrush(ColorFromHex("#CC222226"));
        rd["SurfaceHoverBrush"] = new SolidColorBrush(ColorFromHex("#DD2E2E33"));
        rd["SurfaceBorderBrush"] = new SolidColorBrush(ColorFromHex("#29FFFFFF"));
        rd["CardBrush"] = new SolidColorBrush(ColorFromHex("#B31E1E22"));
        rd["CardHoverBrush"] = new SolidColorBrush(ColorFromHex("#CC2A2A30"));

        // Accent colors (emerald green)
        rd["AccentBrush"] = new SolidColorBrush(ColorFromHex("#FF34D399"));
        rd["AccentHoverBrush"] = new SolidColorBrush(ColorFromHex("#FF4AE0A8"));
        rd["AccentDimBrush"] = new SolidColorBrush(ColorFromHex("#FF1A2E26"));
        rd["AccentGlowBrush"] = new SolidColorBrush(ColorFromHex("#3334D399"));

        // Functional colors
        rd["DangerBrush"] = new SolidColorBrush(ColorFromHex("#FFF87171"));
        rd["DangerDimBrush"] = new SolidColorBrush(ColorFromHex("#FF2D1F1F"));
        rd["WarningBrush"] = new SolidColorBrush(ColorFromHex("#FFFBBF24"));
        rd["WarningDimBrush"] = new SolidColorBrush(ColorFromHex("#FF2D2A1A"));
        rd["InfoBrush"] = new SolidColorBrush(ColorFromHex("#FF60A5FA"));
        rd["InfoDimBrush"] = new SolidColorBrush(ColorFromHex("#FF1A2535"));

        // Text hierarchy
        rd["TextPrimaryBrush"] = new SolidColorBrush(ColorFromHex("#FFF5F5F5"));
        rd["TextSecondaryBrush"] = new SolidColorBrush(ColorFromHex("#FFA1A1AA"));
        rd["TextTertiaryBrush"] = new SolidColorBrush(ColorFromHex("#FF71717A"));
        rd["TextDisabledBrush"] = new SolidColorBrush(ColorFromHex("#FF4A4A52"));

        // Navigation
        rd["NavBgBrush"] = new SolidColorBrush(ColorFromHex("#F018181B"));
        rd["NavItemHoverBrush"] = new SolidColorBrush(ColorFromHex("#1AFFFFFF"));
        rd["NavIndicatorBrush"] = new SolidColorBrush(ColorFromHex("#FF34D399"));

        // Status bar
        rd["StatusBarBrush"] = new SolidColorBrush(ColorFromHex("#F018181B"));

        // Tags
        rd["TagInfoBrush"] = new SolidColorBrush(ColorFromHex("#2634D399"));
        rd["TagWarnBrush"] = new SolidColorBrush(ColorFromHex("#26FBBF24"));
        rd["TagErrorBrush"] = new SolidColorBrush(ColorFromHex("#26F87171"));

        // Input
        rd["InputBgBrush"] = new SolidColorBrush(ColorFromHex("#FF27272A"));
        rd["InputBorderBrush"] = new SolidColorBrush(ColorFromHex("#FF3F3F46"));
        rd["InputFocusBrush"] = new SolidColorBrush(ColorFromHex("#FF34D399"));

        // Scrollbar
        rd["ScrollThumbBrush"] = new SolidColorBrush(ColorFromHex("#33FFFFFF"));
        rd["ScrollThumbHoverBrush"] = new SolidColorBrush(ColorFromHex("#55FFFFFF"));

        // Separator
        rd["SeparatorBrush"] = new SolidColorBrush(ColorFromHex("#FF27272A"));

        // Title bar
        rd["TitleBarBrush"] = new SolidColorBrush(ColorFromHex("#FF141416"));
        rd["TitleBarTextBrush"] = new SolidColorBrush(ColorFromHex("#FFD4D4D8"));
        rd["TitleBarButtonBrush"] = new SolidColorBrush(ColorFromHex("#FF71717A"));
        rd["TitleBarCloseHoverBrush"] = new SolidColorBrush(ColorFromHex("#FFE8525A"));

        // HandyControl dark skin
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

        // Background layers
        rd["BgBrush"] = new SolidColorBrush(ColorFromHex("#F0F8F8F9"));
        rd["BgDeepBrush"] = new SolidColorBrush(ColorFromHex("#F0F0F0F2"));
        rd["SurfaceBrush"] = new SolidColorBrush(ColorFromHex("#CCFFFFFF"));
        rd["SurfaceHoverBrush"] = new SolidColorBrush(ColorFromHex("#DDF5F5F7"));
        rd["SurfaceBorderBrush"] = new SolidColorBrush(ColorFromHex("#1A000000"));
        rd["CardBrush"] = new SolidColorBrush(ColorFromHex("#B3FFFFFF"));
        rd["CardHoverBrush"] = new SolidColorBrush(ColorFromHex("#CCFAFAFA"));

        // Accent colors
        rd["AccentBrush"] = new SolidColorBrush(ColorFromHex("#FF059669"));
        rd["AccentHoverBrush"] = new SolidColorBrush(ColorFromHex("#FF10B981"));
        rd["AccentDimBrush"] = new SolidColorBrush(ColorFromHex("#FFE6F7F0"));
        rd["AccentGlowBrush"] = new SolidColorBrush(ColorFromHex("#26059669"));

        // Functional colors
        rd["DangerBrush"] = new SolidColorBrush(ColorFromHex("#FFDC2626"));
        rd["DangerDimBrush"] = new SolidColorBrush(ColorFromHex("#FFFEE2E2"));
        rd["WarningBrush"] = new SolidColorBrush(ColorFromHex("#FFD97706"));
        rd["WarningDimBrush"] = new SolidColorBrush(ColorFromHex("#FFFFFBEB"));
        rd["InfoBrush"] = new SolidColorBrush(ColorFromHex("#FF2563EB"));
        rd["InfoDimBrush"] = new SolidColorBrush(ColorFromHex("#FFEFF6FF"));

        // Text hierarchy
        rd["TextPrimaryBrush"] = new SolidColorBrush(ColorFromHex("#FF18181B"));
        rd["TextSecondaryBrush"] = new SolidColorBrush(ColorFromHex("#FF52525B"));
        rd["TextTertiaryBrush"] = new SolidColorBrush(ColorFromHex("#FFA1A1AA"));
        rd["TextDisabledBrush"] = new SolidColorBrush(ColorFromHex("#FFD4D4D8"));

        // Navigation
        rd["NavBgBrush"] = new SolidColorBrush(ColorFromHex("#F0FFFFFF"));
        rd["NavItemHoverBrush"] = new SolidColorBrush(ColorFromHex("#0D000000"));
        rd["NavIndicatorBrush"] = new SolidColorBrush(ColorFromHex("#FF059669"));

        // Status bar
        rd["StatusBarBrush"] = new SolidColorBrush(ColorFromHex("#F0FFFFFF"));

        // Tags
        rd["TagInfoBrush"] = new SolidColorBrush(ColorFromHex("#1A059669"));
        rd["TagWarnBrush"] = new SolidColorBrush(ColorFromHex("#1AD97706"));
        rd["TagErrorBrush"] = new SolidColorBrush(ColorFromHex("#1ADC2626"));

        // Input
        rd["InputBgBrush"] = new SolidColorBrush(ColorFromHex("#FFF4F4F5"));
        rd["InputBorderBrush"] = new SolidColorBrush(ColorFromHex("#FFE4E4E7"));
        rd["InputFocusBrush"] = new SolidColorBrush(ColorFromHex("#FF059669"));

        // Scrollbar
        rd["ScrollThumbBrush"] = new SolidColorBrush(ColorFromHex("#1A000000"));
        rd["ScrollThumbHoverBrush"] = new SolidColorBrush(ColorFromHex("#33000000"));

        // Separator
        rd["SeparatorBrush"] = new SolidColorBrush(ColorFromHex("#FFE4E4E7"));

        // Title bar
        rd["TitleBarBrush"] = new SolidColorBrush(ColorFromHex("#FFFAFAFA"));
        rd["TitleBarTextBrush"] = new SolidColorBrush(ColorFromHex("#FF27272A"));
        rd["TitleBarButtonBrush"] = new SolidColorBrush(ColorFromHex("#FF71717A"));
        rd["TitleBarCloseHoverBrush"] = new SolidColorBrush(ColorFromHex("#FFDC2626"));

        // HandyControl light skin
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
