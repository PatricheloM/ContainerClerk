using System.Windows;
using System.ComponentModel;

namespace ContainerClerk.View;

public partial class LogWindow
{
    private bool IsCloseable { get; set; } = false;
    
    public LogWindow()
    {
        InitializeComponent();
    }
    
    public void AppendLogLine(string line)
    {
        Dispatcher.Invoke(() =>
        {
            LogTextBox.AppendText(line + Environment.NewLine);
            LogTextBox.ScrollToEnd();
        });
    }

    public void SetButtonEnabled(bool enabled)
    {
        OkButton.IsEnabled = enabled;
        IsCloseable = enabled;
    }

    private void OkButtonClick(object sender, RoutedEventArgs e)
    {
        Close();
    }
    
    private void WindowClosing(object sender, CancelEventArgs e)
    {
        e.Cancel = !IsCloseable;
    }
}