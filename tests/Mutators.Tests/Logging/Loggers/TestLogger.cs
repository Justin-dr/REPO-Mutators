using System.Collections.ObjectModel;
using Mutators.Logging.Loggers;

namespace Mutators.Tests.Logging.Loggers
{
    public class TestLogger : IMutatorsLogger
    {
        private readonly IList<string> _fatalLogs = [];
        private readonly IList<string> _errorLogs = [];
        private readonly IList<string> _warningLogs = [];
        private readonly IList<string> _messageLogs = [];
        private readonly IList<string> _infoLogs = [];
        private readonly IList<string> _debugLogs = [];
        
        public IReadOnlyList<string> FatalLogs => new ReadOnlyCollection<string>(_fatalLogs);
        public IReadOnlyList<string> ErrorLogs => new ReadOnlyCollection<string>(_errorLogs);
        public IReadOnlyList<string> WarningLogs => new ReadOnlyCollection<string>(_warningLogs);
        public IReadOnlyList<string> MessageLogs => new ReadOnlyCollection<string>(_messageLogs);
        public IReadOnlyList<string> InfoLogs => new ReadOnlyCollection<string>(_infoLogs);
        public IReadOnlyList<string> DebugLogs => new ReadOnlyCollection<string>(_debugLogs);
        
        public void LogFatal(object data)
        {
            _fatalLogs.Add(data.ToString()!);
            Console.WriteLine(data);
        }

        public void LogError(object data)
        {
            _errorLogs.Add(data.ToString()!);
            Console.WriteLine(data);
        }

        public void LogWarning(object data)
        {
            _warningLogs.Add(data.ToString()!);
            Console.WriteLine(data);
        }

        public void LogMessage(object data)
        {
            _messageLogs.Add(data.ToString()!);
            Console.WriteLine(data);
        }

        public void LogInfo(object data)
        {
            _infoLogs.Add(data.ToString()!);
            Console.WriteLine(data);
        }

        public void LogDebug(object data)
        {
            _debugLogs.Add(data.ToString()!);
            Console.WriteLine(data);
        }
    }
}