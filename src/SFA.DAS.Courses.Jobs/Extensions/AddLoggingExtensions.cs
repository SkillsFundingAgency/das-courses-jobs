using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using System.Reflection;

namespace SFA.DAS.Courses.Jobs.Extensions
{
    public static class AddLoggingExtensions
    {
        public static void AddLogging(this ILoggingBuilder logBuilder)
        {
            // all logging is filtered out by defualt
            logBuilder.AddFilter(typeof(Program).Namespace, LogLevel.Information);
            var rootDirectory = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, ".."));
            var files = Directory.GetFiles(rootDirectory, "nlog.config", SearchOption.AllDirectories)[0];
            logBuilder.AddNLog(files);
        }
    }
}