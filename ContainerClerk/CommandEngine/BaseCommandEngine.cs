using System.Diagnostics;
using System.Text;
using ContainerClerk.Util;
using NLog;

namespace ContainerClerk.CommandEngine;

public abstract class BaseCommandEngine
{
    protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    
    protected async Task<string> RunScriptAsync(string scriptName, params ValueTuple<string, string>[] args)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = $"/c type {PathUtils.GetScriptPath(scriptName)} | wsl {ConvertToEnvironmentVariableChain(args)} bash",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        
        Logger.Info($"Process arguments: {psi.Arguments}");

        using var process = new Process();
        
        process.StartInfo = psi;
        process.EnableRaisingEvents = true;
        
        process.Start();
        
        Logger.Info($"Running process on PID {process.Id}");
        
        var output = await process.StandardOutput.ReadToEndAsync();
        
        await process.WaitForExitAsync();
        
        Logger.Info($"Command output: {output}");
        Logger.Info($"Command exitcode: {process.ExitCode}");

        return output;
    }
    
    protected async Task RunScriptWithLiveLoggingAsync(string scriptName, Action<string> logCallback, CancellationToken cancellationToken = default, params ValueTuple<string, string>[] args)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = $"/c type {PathUtils.GetScriptPath(scriptName)} | wsl {ConvertToEnvironmentVariableChain(args)} bash",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        
        Logger.Info($"Process arguments: {psi.Arguments}");

        using var process = new Process();
        
        process.StartInfo = psi;
        process.EnableRaisingEvents = true;
        
        process.OutputDataReceived += (_, e) => { if (e.Data != null) logCallback(e.Data); };
        process.ErrorDataReceived += (_, e) => { if (e.Data != null) logCallback(e.Data); };

        process.Start();
        
        Logger.Info($"Running process on PID {process.Id}");

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync(cancellationToken);
    }

    private static string ConvertToEnvironmentVariableChain(ValueTuple<string, string>[] args)
    {
        var sb = new StringBuilder();
        
        args.ToList().ForEach(arg => sb.Append($"{arg.Item1}={arg.Item2} "));
        
        var s = sb.ToString();
        
        Logger.Info($"Environment variable chain: {s}");
        
        return s;
    }
}