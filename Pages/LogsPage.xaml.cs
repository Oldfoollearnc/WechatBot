using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using WechatBot.ViewModels;

namespace WechatBot.Pages;

public partial class LogsPage : UserControl
{
    private readonly MainViewModel _vm;
    private string _filterLevel = "All";
    private string _searchText = "";

    public LogsPage(MainViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        DataContext = _vm;

        // 新日志自动滚动到底部
        _vm.Logs.CollectionChanged += (_, _) =>
        {
            if (LogListBox.Items.Count > 0)
                LogListBox.ScrollIntoView(LogListBox.Items[^1]);
        };
    }

    private void Filter_Click(object sender, RoutedEventArgs e)
    {
        if (sender is RadioButton { Tag: string tag })
        {
            _filterLevel = tag;
            ApplyFilter();
        }
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        _searchText = SearchBox.Text;
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var view = CollectionViewSource.GetDefaultView(_vm.Logs);
        view.Filter = item =>
        {
            if (item is not LogEntry log) return false;

            if (_filterLevel != "All")
            {
                if (_filterLevel == "Info" && log.Level != LogLevel.Info) return false;
                if (_filterLevel == "Warn" && log.Level != LogLevel.Warn) return false;
                if (_filterLevel == "Error" && log.Level != LogLevel.Error) return false;
            }

            if (!string.IsNullOrEmpty(_searchText)
                && !log.Message.Contains(_searchText, StringComparison.OrdinalIgnoreCase))
                return false;

            return true;
        };
    }

    private void Clear_Click(object sender, RoutedEventArgs e)
    {
        _vm.Logs.Clear();
    }

    private void OpenLogDir_Click(object sender, RoutedEventArgs e)
    {
        var logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
        Directory.CreateDirectory(logDir);
        Process.Start("explorer.exe", logDir);
    }
}
