using NLog;

namespace ContainerClerk;

public partial class App
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private App()
    {
        Logger.Info("Application started.");
    }

    ~App()
    {
        Logger.Info("Application stopped.");
    }
}