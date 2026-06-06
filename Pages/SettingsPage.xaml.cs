using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using HandyControl.Controls;
using WechatBot.ViewModels;

namespace WechatBot.Pages;

public partial class SettingsPage : UserControl
{
    private readonly MainViewModel _vm;

    public SettingsPage(MainViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        DataContext = _vm;
        UpdateThemeSelection();
    }

    private void ThemeDark_Click(object sender, RoutedEventArgs e)
    {
        _vm.Theme = "Dark";
        ThemeManager.ApplyTheme("Dark");
        UpdateThemeSelection();
    }

    private void ThemeLight_Click(object sender, RoutedEventArgs e)
    {
        _vm.Theme = "Light";
        ThemeManager.ApplyTheme("Light");
        UpdateThemeSelection();
    }

    private void UpdateThemeSelection()
    {
        var isDark = _vm.Theme == "Dark";
        ThemeDarkCard.BorderBrush = isDark
            ? (Brush)Application.Current.FindResource("AccentBrush")
            : Brushes.Transparent;
        ThemeLightCard.BorderBrush = !isDark
            ? (Brush)Application.Current.FindResource("AccentBrush")
            : Brushes.Transparent;
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        _vm.SaveSettingsCommand.Execute(null);
        Growl.Success("设置已保存 ✓");

        if (sender is Button btn)
        {
            var scaleAnim = new DoubleAnimation(1, 0.95, TimeSpan.FromMilliseconds(80))
            {
                AutoReverse = true,
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };
            var transform = btn.RenderTransform as ScaleTransform;
            if (transform == null)
            {
                transform = new ScaleTransform(1, 1);
                btn.RenderTransformOrigin = new Point(0.5, 0.5);
                btn.RenderTransform = transform;
            }
            transform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnim);
            transform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnim);
        }
    }
}
