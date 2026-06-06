using System.Windows;
using System.Windows.Controls;
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
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        _vm.SaveSettingsCommand.Execute(null);
        Growl.Success("设置已保存");
    }
}
