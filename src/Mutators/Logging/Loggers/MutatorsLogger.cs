using BepInEx.Logging;

namespace Mutators.Logging.Loggers
{
    internal sealed class MutatorsLogger(ManualLogSource logSource) : IMutatorsLogger
    {
        public void LogFatal(object data)
        {
            logSource.LogFatal(data);
        }

        public void LogError(object data)
        {
            logSource.LogError(data);
        }

        public void LogWarning(object data)
        {
            logSource.LogWarning(data);
        }

        public void LogMessage(object data)
        {
            logSource.LogMessage(data);
        }

        public void LogInfo(object data)
        {
            logSource.LogInfo(data);
        }

        public void LogDebug(object data)
        {
            logSource.LogDebug(data);
        }
    }
}