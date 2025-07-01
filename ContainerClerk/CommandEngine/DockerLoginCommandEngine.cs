namespace ContainerClerk.CommandEngine;

public class DockerLoginCommandEngine : BaseCommandEngine
{
    public async Task<bool> LoginAsync(string registry, string username, string password)
    {
        var output = await RunScriptAsync("dockerLogin.sh", ("URL", registry), ("USERNAME", username), ("PW", password));
        return output.Contains("Login Succeeded");
    }
}