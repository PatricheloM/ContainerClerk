using System.Windows;
using System.Windows.Controls;

namespace ContainerClerk.View;

public partial class ProjectSelectWindow
{
    private readonly Action<string> _callback;
    
    public ProjectSelectWindow(IEnumerable<string> projectList, Action<string> callback, bool state)
    {
        _callback = callback;
        InitializeComponent();
        DropdownComboBox.ItemsSource = projectList;
        Submit.Content = state ? "Start" : "Stop";
    }

    private void SubmitButton(object sender, RoutedEventArgs e)
    {
        if (DropdownComboBox.SelectedItem is string projectName)
        {
            Submit.IsEnabled = false;
            _callback(projectName);
            Submit.IsEnabled = true;
        }
        else
        {
            MessageBox.Show("No item selected.");
        }
    }
}