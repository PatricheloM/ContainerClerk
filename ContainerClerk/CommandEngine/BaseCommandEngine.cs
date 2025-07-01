using System.Diagnostics;
using System.Text;
using ContainerClerk.Util;

namespace ContainerClerk.CommandEngine;

public abstract class BaseCommandEngine
{
    protected async Task<string> RunScript(string scriptName, params ValueTuple<string, string>[] args)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = $"/c type {PathUtils.GetScriptPath(scriptName)} | wsl {ConvertToEnvironmentVariableChain(args)} bash",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process();
        
        process.StartInfo = psi;
        process.EnableRaisingEvents = true;
        
        process.Start();
        
        var output = await process.StandardOutput.ReadToEndAsync();
        
        await process.WaitForExitAsync();

        return output;
    }
    
    protected async Task RunScriptWithLiveLogging(string scriptName, Action<string> logCallback, params ValueTuple<string, string>[] args)
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

        using var process = new Process();
        
        process.StartInfo = psi;
        process.EnableRaisingEvents = true;
        
        process.OutputDataReceived += (_, e) => { if (e.Data != null) logCallback(e.Data); };
        process.ErrorDataReceived += (_, e) => { if (e.Data != null) logCallback(e.Data); };

        process.Start();

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync();
        
        
    }

    private static string ConvertToEnvironmentVariableChain(ValueTuple<string, string>[] args)
    {
        var sb = new StringBuilder();
        
        args.ToList().ForEach(arg => sb.Append($"{arg.Item1}={arg.Item2} "));
        
        return sb.ToString();
    }
}