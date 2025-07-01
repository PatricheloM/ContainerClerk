using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
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
            
            var logWindow = new LogWindow();
            logWindow.SetButtonEnabled(false);
            logWindow.Show();

            await _dockerCompose.DockerComposeUpAsync(wslAbsolutePath, logWindow.AppendLogLine);
            
            logWindow.SetButtonEnabled(true);
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
    
    private async void StopContainer(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is not Button { DataContext: DockerContainer container }) return;
            
            if (container.GetState())
            {
                await _dockerPs.StopContainer(container.ID);
            }
            else
            {
                await _dockerPs.StartContainer(container.ID);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Unexpected exception! {ex.Message}");
        }
    }
    
    private async void RemoveContainer(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is not Button { DataContext: DockerContainer container }) return;
            
            if (container.GetState())
            {
                await _dockerPs.StopContainer(container.ID);
            }
            
            await _dockerPs.RemoveContainer(container.ID);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Unexpected exception! {ex.Message}");
        }
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
                        ID = "ERROR",
                        Image = ex.Message
                    });
                });
            }

            await Task.Delay(1000);
        }
    }
}