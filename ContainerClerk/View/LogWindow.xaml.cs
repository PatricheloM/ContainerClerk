using System.Windows;

namespace ContainerClerk.View;

public partial class LogWindow
{
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
    }

    private void OkButtonClick(object sender, RoutedEventArgs e)
    {
        Close();
    }
}