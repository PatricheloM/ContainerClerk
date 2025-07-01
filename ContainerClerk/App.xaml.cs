using System.IO;
using NLog;

namespace ContainerClerk;

public partial class App
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private App()
    {
        Logger.Info("Application started.");
        
        Logger.Info("Checking scripts for EOL.");
        foreach (var file in Directory.GetFiles(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Scripts")))
        {
            File.WriteAllText(file,File.ReadAllText(file).Replace("\r\n", "\n"));
        }
        Logger.Info("Scripts have been saved.");
    }

    ~App()
    {
        Logger.Info("Application stopped.");
    }
}