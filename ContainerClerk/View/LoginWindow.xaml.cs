using System.Windows;
using ContainerClerk.CommandEngine;

namespace ContainerClerk.View;

public partial class LoginWindow
{
    private readonly DockerLoginCommandEngine _dockerLogin;
    
    public LoginWindow()
    {
        _dockerLogin = new DockerLoginCommandEngine();
        InitializeComponent();
    }

    private async void LoginClick(object sender, RoutedEventArgs e)
    {
        LoginButton.IsEnabled = false;
        StatusText.Text = "Connecting...";
        
        var registryUrl = UrlTextBox.Text;
        var username = UsernameTextBox.Text;
        var password = PasswordBox.Password;

        if (string.IsNullOrWhiteSpace(registryUrl) || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            StatusText.Text = "Please fill in all fields.";
            return;
        }
        
        var status = await _dockerLogin.LoginAsync(registryUrl, username, password);
        
        StatusText.Text = status ? "Login successful." : "Login failed.";
        
        LoginButton.IsEnabled = true;
    }
}