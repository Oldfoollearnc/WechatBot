using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using WechatBot.ViewModels;
using MessageBox = System.Windows.MessageBox;

namespace WechatBot.Pages;

public partial class StepsPage : UserControl
{
    private readonly MainViewModel _vm;
    private bool _animated;

    public StepsPage(MainViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        DataContext = _vm;
    }

    private void StepsPage_Loaded(object sender, RoutedEventArgs e)
    {
        if (!_animated)
        {
            _animated = true;
            // 步骤卡片交错入场动画在列表滚动时会自动触发，
            // 这里仅标记已加载
        }
    }

    private async void Capture_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: StepViewModel step })
            await _vm.CaptureTemplateAsync(step);
    }

    private async void TestRecognition_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: StepViewModel step })
            await step.TestRecognitionCommand.ExecuteAsync(null);
    }

    private void Reset_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: StepViewModel step })
        {
            if (MessageBox.Show($"确定重置 [{step.Name}] 的模板吗？", "确认",
                    MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                _vm.ResetTemplate(step);
            }
        }
    }
}
