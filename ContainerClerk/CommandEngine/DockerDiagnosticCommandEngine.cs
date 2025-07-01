namespace ContainerClerk.CommandEngine;

public class DockerDiagnosticCommandEngine : BaseCommandEngine
{
    public async Task ViewLogsAsync(string id, Action<string> logCallback, CancellationToken cancellationToken = default)
    {
        await RunScriptWithLiveLoggingAsync("dockerLogs.sh", logCallback, cancellationToken, ("ID", id));
    }
}