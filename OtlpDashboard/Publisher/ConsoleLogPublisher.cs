using Dotnetydd.OtlpDashboard.DataSource;
using Dotnetydd.OtlpDashboard.Model;

namespace Dotnetydd.OtlpDashboard.Publisher;

public class ConsoleLogPublisher(ApplicationResourcePublisher resourcePublisher)
{
    internal IAsyncEnumerable<IReadOnlyList<(string Content, bool IsErrorMessage)>>? Subscribe(string resourceName)
    {
        if (!resourcePublisher.TryGetResource(resourceName, out var resource))
        {
            throw new ArgumentException($"Unknown resource {resourceName}.", nameof(resourceName));
        }


        // Obtain logs using the relevant approach.
        // Note, we would like to obtain these logs via DCP directly, rather than sourcing them in the dashboard.
        return resource switch
        {
            ExecutableViewModel executable => SubscribeExecutable(executable),
            _ => throw new NotSupportedException($"Unsupported resource type {resource.GetType()}.")
        };


        static FileLogSource? SubscribeExecutable(ExecutableViewModel executable)
        {
            if (executable.StdOutFile is null || executable.StdErrFile is null)
            {
                return null;
            }

            return new FileLogSource(executable.StdOutFile, executable.StdErrFile);
        }
    }
}