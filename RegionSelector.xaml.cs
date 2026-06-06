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
        Loaded += (_, _) => Focus();
        SizeChanged += (_, _) =>
        {
            CrosshairH.X2 = ActualWidth;
            CrosshairV.Y2 = ActualHeight;
        };
    }

    private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed) return;

        // 隐藏确认面板（如果正在重新选择）
        ConfirmPanel.Visibility = Visibility.Collapsed;

        _startPoint = e.GetPosition(this);
        _isSelecting = true;

        SelectionRect.Visibility = Visibility.Visible;
        SizeLabel.Visibility = Visibility.Visible;
        CrosshairH.Visibility = Visibility.Visible;
        CrosshairV.Visibility = Visibility.Visible;

        Canvas.SetLeft(SelectionRect, _startPoint.X);
        Canvas.SetTop(SelectionRect, _startPoint.Y);
        SelectionRect.Width = 0;
        SelectionRect.Height = 0;

        DrawCanvas.CaptureMouse();
        e.Handled = true;
    }

    private void Canvas_MouseMove(object sender, MouseEventArgs e)
    {
        var currentPoint = e.GetPosition(this);

        // 更新十字准线
        if (CrosshairH.Visibility == Visibility.Visible)
        {
            CrosshairH.Y1 = currentPoint.Y;
            CrosshairH.Y2 = currentPoint.Y;
            CrosshairV.X1 = currentPoint.X;
            CrosshairV.X2 = currentPoint.X;
        }

        if (!_isSelecting) return;

        double x = Math.Min(_startPoint.X, currentPoint.X);
        double y = Math.Min(_startPoint.Y, currentPoint.Y);
        double w = Math.Abs(currentPoint.X - _startPoint.X);
        double h = Math.Abs(currentPoint.Y - _startPoint.Y);

        Canvas.SetLeft(SelectionRect, x);
        Canvas.SetTop(SelectionRect, y);
        SelectionRect.Width = w;
        SelectionRect.Height = h;

        // 更新尺寸标签位置和内容
        SizeLabelText.Text = $"{(int)w} × {(int)h}";
        double labelX = x + w / 2 - 40;
        double labelY = y + h + 8;
        // 防止标签超出屏幕底部
        if (labelY + 30 > ActualHeight)
            labelY = y - 35;
        Canvas.SetLeft(SizeLabel, labelX);
        Canvas.SetTop(SizeLabel, labelY);

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

        // 隐藏十字准线
        CrosshairH.Visibility = Visibility.Collapsed;
        CrosshairV.Visibility = Visibility.Collapsed;

        if (SelectedWidth < 10 || SelectedHeight < 10)
        {
            MessageBox.Show("选择区域太小，请重新框选", "提示");
            SelectionRect.Visibility = Visibility.Collapsed;
            SizeLabel.Visibility = Visibility.Collapsed;
            e.Handled = true;
            return;
        }

        // 更新最终尺寸
        SizeLabelText.Text = $"{SelectedWidth} × {SelectedHeight}";

        // 显示确认面板
        double panelX = SelectedX + SelectedWidth / 2 - 90;
        double panelY = SelectedY + SelectedHeight + 12;
        if (panelY + 60 > ActualHeight)
            panelY = SelectedY - 60;
        Canvas.SetLeft(ConfirmPanel, Math.Max(10, panelX));
        Canvas.SetTop(ConfirmPanel, panelY);
        ConfirmPanel.Visibility = Visibility.Visible;

        e.Handled = true;
    }

    private void ConfirmBtn_Click(object sender, RoutedEventArgs e)
    {
        IsConfirmed = true;
        Close();
    }

    private void CancelBtn_Click(object sender, RoutedEventArgs e)
    {
        // 重置选区，允许重新框选
        SelectionRect.Visibility = Visibility.Collapsed;
        SizeLabel.Visibility = Visibility.Collapsed;
        ConfirmPanel.Visibility = Visibility.Collapsed;
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
