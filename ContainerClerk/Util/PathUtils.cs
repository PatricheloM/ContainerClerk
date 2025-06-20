using System.IO;
using System.Text.RegularExpressions;

namespace ContainerClerk.Util;

public static partial class PathUtils
{
    public static string ConvertToLinuxMountPath(string absolutePath)
    {
        if (IsWslPath(absolutePath)) throw new ArgumentException("WSL path is not supported.");
        
        if (!IsWindowsPath(absolutePath)) throw new ArgumentException("Absolute path is not recognized.");

        return ConvertWindowsPathToWsl(absolutePath);
    }

    public static string GetScriptPath(string scriptName) => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Scripts", scriptName);

    [GeneratedRegex(@"^[a-zA-Z]:\\")]
    private static partial Regex DriveRegex();
    
    private static bool IsWslPath(string path) => !string.IsNullOrWhiteSpace(path) && path.StartsWith(@"\\wsl$\") || path.StartsWith(@"\\wsl.localhost\");

    private static bool IsWindowsPath(string path) => !string.IsNullOrWhiteSpace(path) && DriveRegex().IsMatch(path);

    private static string ConvertWindowsPathToWsl(string windowsPath)
    {
        var driveLetter = char.ToLower(windowsPath[0]).ToString();
        var restOfPath = windowsPath[2..].Replace('\\', '/');

        return $"/mnt/{driveLetter}{restOfPath}";
    }
}
