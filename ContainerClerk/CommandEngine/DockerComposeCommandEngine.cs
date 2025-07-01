namespace ContainerClerk.CommandEngine;

public class DockerComposeCommandEngine : BaseCommandEngine
{
    private async Task DockerComposeAsync(string path, Action<string> logCallback, bool status)
    {
        await RunScriptWithLiveLoggingAsync("dockerComposeInFolder.sh", logCallback, default, ("FOLDER", path), ("STATUS", status ? "up" : "down"));
    }

    public async Task DockerComposeUpAsync(string path, Action<string> logCallback)
    {
        await DockerComposeAsync(path, logCallback, true);
    }

    public async Task DockerComposeDownAsync(string path, Action<string> logCallback)
    {
        await DockerComposeAsync(path, logCallback, false);
    }
}