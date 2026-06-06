using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WechatBot;

public partial class RegionSelector : Window
{
    private System.Windows.Point _startPoint;
    private bool _isSelecting;

    public int SelectedX { get; private set; }
    public int SelectedY { get; private set; }
    public int SelectedWidth { get; private set; }
    public int SelectedHeight { get; private set; }
    public bool IsConfirmed { get; private set; }

    public RegionSelector()
    {
        InitializeComponent();
        // 确保窗口获得焦点
        Loaded += (_, _) => Focus();
    }

    private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed) return;

        _startPoint = e.GetPosition(this);
        _isSelecting = true;

        SelectionRect.Visibility = Visibility.Visible;
        Canvas.SetLeft(SelectionRect, _startPoint.X);
        Canvas.SetTop(SelectionRect, _startPoint.Y);
        SelectionRect.Width = 0;
        SelectionRect.Height = 0;

        // 捕获鼠标
        DrawCanvas.CaptureMouse();
        e.Handled = true;
    }

    private void Canvas_MouseMove(object sender, MouseEventArgs e)
    {
        if (!_isSelecting) return;

        var currentPoint = e.GetPosition(this);

        double x = Math.Min(_startPoint.X, currentPoint.X);
        double y = Math.Min(_startPoint.Y, currentPoint.Y);
        double w = Math.Abs(currentPoint.X - _startPoint.X);
        double h = Math.Abs(currentPoint.Y - _startPoint.Y);

        Canvas.SetLeft(SelectionRect, x);
        Canvas.SetTop(SelectionRect, y);
        SelectionRect.Width = w;
        SelectionRect.Height = h;

        e.Handled = true;
    }

    private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (!_isSelecting) return;

        _isSelecting = false;
        DrawCanvas.ReleaseMouseCapture();

        var currentPoint = e.GetPosition(this);

        SelectedX = (int)Math.Min(_startPoint.X, currentPoint.X);
        SelectedY = (int)Math.Min(_startPoint.Y, currentPoint.Y);
        SelectedWidth = (int)Math.Abs(currentPoint.X - _startPoint.X);
        SelectedHeight = (int)Math.Abs(currentPoint.Y - _startPoint.Y);

        if (SelectedWidth < 10 || SelectedHeight < 10)
        {
            MessageBox.Show("选择区域太小，请重新框选", "提示");
            SelectionRect.Visibility = Visibility.Collapsed;
            e.Handled = true;
            return;
        }

        IsConfirmed = true;
        e.Handled = true;
        Close();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            IsConfirmed = false;
            Close();
        }
        base.OnKeyDown(e);
    }
}
