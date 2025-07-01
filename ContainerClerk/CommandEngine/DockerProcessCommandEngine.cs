using System.Text.Json;
using ContainerClerk.Model;

namespace ContainerClerk.CommandEngine;

public class DockerProcessCommandEngine : BaseCommandEngine
{
    public async Task<List<DockerContainer>> GetDockerContainers()
    {
        var jsonLines = await RunScriptAsync("dockerPsJson.sh");

        return jsonLines.Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(line => JsonSerializer.Deserialize<DockerContainer>(line)).OfType<DockerContainer>().ToList();
    }

    public async Task StopContainer(string id)
    {
        await RunScriptAsync("dockerStartStop.sh", ("ID", id), ("STATE", "stop"));
    }

    public async Task StartContainer(string id)
    {
        await RunScriptAsync("dockerStartStop.sh", ("ID", id), ("STATE", "start"));
    }

    public async Task RemoveContainer(string id)
    {
        await RunScriptAsync("dockerRemove.sh", ("ID", id));
    }
}