using System.Windows;
using System.Windows.Media.Animation;

namespace WechatBot;

public partial class FloatingStatusWindow : Window
{
    public FloatingStatusWindow()
    {
        InitializeComponent();
        PositionToBottomRight();
    }

    /// <summary>
    /// 定位到屏幕右下角
    /// </summary>
    private void PositionToBottomRight()
    {
        var screenWidth = SystemParameters.PrimaryScreenWidth;
        var screenHeight = SystemParameters.PrimaryScreenHeight;
        Left = screenWidth - Width - 20;
        Top = screenHeight - Height - 60;
    }

    /// <summary>
    /// 更新状态
    /// </summary>
    public void UpdateStatus(string icon, string text, string stepInfo = "")
    {
        Dispatcher.Invoke(() =>
        {
            StatusIcon.Text = icon;
            StatusText.Text = text;
            StepInfo.Text = stepInfo;
        });
    }

    /// <summary>
    /// 显示窗口（带淡入动画）
    /// </summary>
    public void ShowWithAnimation()
    {
        Show();
        var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300));
        BeginAnimation(OpacityProperty, fadeIn);
    }

    /// <summary>
    /// 隐藏窗口（带淡出动画）
    /// </summary>
    public void HideWithAnimation()
    {
        var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(300));
        fadeOut.Completed += (_, _) => Hide();
        BeginAnimation(OpacityProperty, fadeOut);
    }
}
