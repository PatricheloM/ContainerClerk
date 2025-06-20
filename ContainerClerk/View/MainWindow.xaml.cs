using System.Collections.ObjectModel;
using System.Windows;
using ContainerClerk.CommandEngine;
using ContainerClerk.Model;
using ContainerClerk.Util;
using Microsoft.Win32;

namespace ContainerClerk.View;

public partial class MainWindow
{
    private readonly DockerComposeCommandEngine _dockerCompose;
    private readonly DockerProcessCommandEngine _dockerPs;
    
    private readonly ObservableCollection<DockerContainer> _dockerContainers = [];
    
    public MainWindow()
    {
        _dockerCompose = new DockerComposeCommandEngine();
        _dockerPs = new DockerProcessCommandEngine();
        InitializeComponent();
        DockerGrid.ItemsSource = _dockerContainers;
        
        _ = StartDockerRefreshLoopAsync();
    }

    private async void AddComposeProject(object sender, RoutedEventArgs e)
    {
        try
        {
            var openFileDialog = new OpenFileDialog { Filter = "All (*.*)|*.*" };

            if (openFileDialog.ShowDialog() != true) return;

            var wslAbsolutePath = PathUtils.ConvertToLinuxMountPath(openFileDialog.FileName);

            await _dockerCompose.DockerComposeUpAsync(wslAbsolutePath, AppendLogLine);
        }
        catch (ArgumentException ex)
        {
            MessageBox.Show($"Wrong file! {ex.Message}");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Unexpected exception! {ex.Message}");
        }
    }
    
    private void AppendLogLine(string line)
    {
        Dispatcher.Invoke(() =>
        {
            LogTextBox.AppendText(line + Environment.NewLine);
            LogTextBox.ScrollToEnd();
        });
    }
    
    private async Task StartDockerRefreshLoopAsync()
    {
        while (true)
        {
            try
            {
                var latest = await _dockerPs.GetDockerContainers();

                Dispatcher.Invoke(() =>
                {
                    _dockerContainers.Clear();
                    foreach (var container in latest)
                        _dockerContainers.Add(container);
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => {
                    _dockerContainers.Clear();
                    _dockerContainers.Add(new DockerContainer
                    {
                        ID = "HIBA",
                        Image = ex.Message
                    });
                });
            }

            await Task.Delay(1000);
        }
    }
}