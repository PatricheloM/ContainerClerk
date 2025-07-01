using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using ContainerClerk.CommandEngine;
using ContainerClerk.Model;
using ContainerClerk.Util;
using Microsoft.Win32;
using NLog;

namespace ContainerClerk.View;

public partial class MainWindow
{
    private readonly DockerComposeCommandEngine _dockerCompose;
    private readonly DockerProcessCommandEngine _dockerPs;
    private readonly DockerDiagnosticCommandEngine _dockerDiagnostic;
    
    private readonly ObservableCollection<DockerContainer> _dockerContainers = [];
    
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    
    public MainWindow()
    {
        _dockerCompose = new DockerComposeCommandEngine();
        _dockerPs = new DockerProcessCommandEngine();
        _dockerDiagnostic = new DockerDiagnosticCommandEngine();
        InitializeComponent();
        DockerGrid.ItemsSource = _dockerContainers;
        
        _ = StartDockerRefreshLoopAsync();
    }

    private async void AddComposeProject(object sender, RoutedEventArgs e)
    {
        try
        {
            var openFileDialog = new OpenFileDialog { Filter = "YAML Files (*.yaml, *.yml)|*.yaml;*.yml" };

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
            Logger.Error(ex, "ArgumentException!");
            MessageBox.Show($"Wrong file! {ex.Message}");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Unexpected exception!");
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
                await _dockerPs.StopContainerAsync(container.ID);
            }
            else
            {
                await _dockerPs.StartContainerAsync(container.ID);
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Unexpected exception!");
            MessageBox.Show($"Unexpected exception! {ex.Message}");
        }
    }
    
    private async void RemoveContainer(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is not Button { DataContext: DockerContainer container }) return;

            if (MessageBox.Show($"Are you sure you want to delete {container.Names}?", "Delete container", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;

            if (container.GetState())
            {
                await _dockerPs.StopContainerAsync(container.ID);
            }
            
            await _dockerPs.RemoveContainerAsync(container.ID);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Unexpected exception!");
            MessageBox.Show($"Unexpected exception! {ex.Message}");
        }
    }
    
    private async void LogContainer(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is not Button { DataContext: DockerContainer container }) return;

            var cts = new CancellationTokenSource();

            var logWindow = new LogWindow();

            logWindow.Closing += (_, _) => cts.Cancel();
            logWindow.SetButtonEnabled(true);
            logWindow.Show();

            await _dockerDiagnostic.ViewLogsAsync(container.ID, logWindow.AppendLogLine, cts.Token);
        }
        catch (TaskCanceledException)
        {
            Logger.Info("Container log process SIGTERM.");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Unexpected exception!");
            MessageBox.Show($"Unexpected exception! {ex.Message}");
        }
    }
    
    private async Task StartDockerRefreshLoopAsync()
    {
        while (true)
        {
            try
            {
                var latest = await _dockerPs.GetDockerContainersAsync();

                Dispatcher.Invoke(() =>
                {
                    _dockerContainers.Clear();
                    foreach (var container in latest)
                        _dockerContainers.Add(container);
                });
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unexpected exception while fetching docker containers!");
                
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

    private void OpenLogin(object sender, RoutedEventArgs e)
    {
        new LoginWindow().ShowDialog();
    }

    private async void OpenTerminal(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is not Button { DataContext: DockerContainer container }) return;

            await _dockerPs.GetShellAsync(container.ID);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Unexpected exception!");
            MessageBox.Show($"Unexpected exception! {ex.Message}");
        }
    }
}