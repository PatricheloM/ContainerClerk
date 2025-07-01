using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
using ContainerClerk.Model;

namespace ContainerClerk.CommandEngine;

public partial class DockerProcessCommandEngine : BaseCommandEngine
{
    public async Task<List<DockerContainer>> GetDockerContainersAsync()
    {
        var jsonLines = await RunScriptAsync("dockerPsJson.sh");

        return jsonLines.Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(line =>
        {
            var obj = JsonSerializer.Deserialize<DockerContainer>(line);

            if (obj is not null)
            {
                obj.ComposeProject = obj.Labels.Split(',').FirstOrDefault(x => x.StartsWith("com.docker.compose.project="))?.Split('=')[1] ?? "";
                
                var matches = PortRegex().Matches(obj.Ports);
                
                obj.Ports = $"{matches.LastOrDefault()?.Groups[0].Value ?? ""}";
            }
            
            return obj;
        }).OfType<DockerContainer>().ToList();
    }

    public async Task GetShellAsync(string id)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = $"/K wsl docker exec -i {id} sh",
            UseShellExecute = true,
            CreateNoWindow = false
        };
        
        using var process = new Process();
        process.StartInfo = psi;
        process.Start();

        await Task.Yield();
    }

    public async Task StopContainerAsync(string id)
    {
        await RunScriptAsync("dockerStartStop.sh", ("ID", id), ("STATE", "stop"));
    }

    public async Task StartContainerAsync(string id)
    {
        await RunScriptAsync("dockerStartStop.sh", ("ID", id), ("STATE", "start"));
    }

    public async Task RemoveContainerAsync(string id)
    {
        await RunScriptAsync("dockerRemove.sh", ("ID", id));
    }

    [GeneratedRegex(@"(\d+)->(\d+)")]
    private static partial Regex PortRegex();
}