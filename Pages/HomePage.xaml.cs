using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using WechatBot.ViewModels;

namespace WechatBot.Pages;

public partial class HomePage : UserControl
{
    private readonly MainViewModel _vm;
    private bool _animated;

    public HomePage(MainViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        DataContext = _vm;

        _vm.Logs.CollectionChanged += (_, _) => UpdateLogVisibility();
        UpdateLogVisibility();
    }

    private void HomePage_Loaded(object sender, RoutedEventArgs e)
    {
        if (!_animated)
        {
            _animated = true;
            AnimateCardsEntrance();
        }
    }

    private void AnimateCardsEntrance()
    {
        var cards = new FrameworkElement[] { CardTotal, CardConfigured, CardStatus };
        for (int i = 0; i < cards.Length; i++)
        {
            AnimateCardIn(cards[i], i * 100);
        }

        // 进度条宽度动画
        AnimateProgressWidth();
    }

    private void AnimateCardIn(FrameworkElement element, int delayMs)
    {
        var delay = TimeSpan.FromMilliseconds(delayMs);

        var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(400))
        {
            BeginTime = delay,
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        var slideUp = new DoubleAnimation(20, 0, TimeSpan.FromMilliseconds(450))
        {
            BeginTime = delay,
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        element.BeginAnimation(OpacityProperty, fadeIn);
        var transform = element.RenderTransform as TranslateTransform;
        transform?.BeginAnimation(TranslateTransform.YProperty, slideUp);
    }

    private void AnimateProgressWidth()
    {
        // 延迟显示进度条
        var timer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
        timer.Tick += (_, _) =>
        {
            timer.Stop();
            var targetWidth = _vm.ProgressPercent * 2.0; // 简单映射到像素
            var anim = new DoubleAnimation(0, targetWidth, TimeSpan.FromMilliseconds(600))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            ProgressFill.BeginAnimation(WidthProperty, anim);
        };
        timer.Start();
    }

    private void UpdateLogVisibility()
    {
        var hasLogs = _vm.Logs.Count > 0;
        EmptyLogState.Visibility = hasLogs ? Visibility.Collapsed : Visibility.Visible;
        LogItems.Visibility = hasLogs ? Visibility.Visible : Visibility.Collapsed;
    }

    private async void Run_Click(object sender, MouseButtonEventArgs e)
        => await _vm.RunCommand.ExecuteAsync(null);

    private async void Test_Click(object sender, MouseButtonEventArgs e)
        => await _vm.TestCommand.ExecuteAsync(null);

    private void Stop_Click(object sender, MouseButtonEventArgs e)
        => _vm.StopCommand.Execute(null);
}
