using System.Windows;
using System.Windows.Input;
using WechatBot.ViewModels;

namespace WechatBot.Pages;

public partial class HomePage : System.Windows.Controls.UserControl
{
    private readonly MainViewModel _vm;

    public HomePage(MainViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        DataContext = _vm;
    }

    private async void Run_Click(object sender, MouseButtonEventArgs e)
        => await _vm.RunCommand.ExecuteAsync(null);

    private async void Test_Click(object sender, MouseButtonEventArgs e)
        => await _vm.TestCommand.ExecuteAsync(null);

    private void Stop_Click(object sender, MouseButtonEventArgs e)
        => _vm.StopCommand.Execute(null);
}
