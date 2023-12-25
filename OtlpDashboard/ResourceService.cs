using Dotnetydd.OtlpDashboard.ApplicationModels;
using Dotnetydd.OtlpDashboard.Model;
using Dotnetydd.OtlpDashboard.Publisher;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Dotnetydd.OtlpDashboard;

public class ApplicationResourceService: IResourceService, IAsyncDisposable
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly ApplicationResourcePublisher _resourcePublisher;
    private readonly ConsoleLogPublisher _consoleLogPublisher;

    public ApplicationResourceService(IHostEnvironment hostEnvironment)
    {
        ApplicationName = ComputeApplicationName(hostEnvironment.ApplicationName);

        _resourcePublisher = new ApplicationResourcePublisher(_cancellationTokenSource.Token);
        _consoleLogPublisher = new ConsoleLogPublisher(_resourcePublisher);

        static string ComputeApplicationName(string applicationName)
        {
            const string AppHostSuffix = ".AppHost";

            if (applicationName.EndsWith(AppHostSuffix, StringComparison.OrdinalIgnoreCase))
            {
                applicationName = applicationName[..^AppHostSuffix.Length];
            }
            return applicationName;
        }
    }

    public string ApplicationName { get; }

    public ResourceSubscription SubscribeResources()
    {
        return _resourcePublisher.Subscribe();
    }


    public IAsyncEnumerable<IReadOnlyList<(string Content, bool IsErrorMessage)>>? SubscribeConsoleLogs(string resourceName, CancellationToken cancellationToken)
    {
        var subscription = _consoleLogPublisher.Subscribe(resourceName);

        return subscription is null ? null : Enumerate();

        async IAsyncEnumerable<IReadOnlyList<(string Content, bool IsErrorMessage)>> Enumerate()
        {
            using var token = CancellationTokenSource.CreateLinkedTokenSource(_cancellationTokenSource.Token, cancellationToken);

            await foreach (var group in subscription.WithCancellation(token.Token))
            {
                yield return group;
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _cancellationTokenSource.CancelAsync().ConfigureAwait(false);
    }


}