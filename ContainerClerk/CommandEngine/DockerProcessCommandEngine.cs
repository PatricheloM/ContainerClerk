using System.Text.Json;
using ContainerClerk.Model;

namespace ContainerClerk.CommandEngine;

public class DockerProcessCommandEngine : BaseCommandEngine
{
    public async Task<List<DockerContainer>> GetDockerContainers()
    {
        var jsonLines = await RunScript("dockerPsJson.sh");

        return jsonLines.Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(line => JsonSerializer.Deserialize<DockerContainer>(line)).OfType<DockerContainer>().ToList();
    }
}