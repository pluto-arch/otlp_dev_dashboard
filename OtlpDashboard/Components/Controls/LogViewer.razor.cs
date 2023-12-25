// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Dotnetydd.OtlpDashboard.ConsoleLogs;
using Dotnetydd.OtlpDashboard.Model;
using Dotnetydd.OtlpDashboard.Utils;
using Microsoft.JSInterop;

namespace Dotnetydd.OtlpDashboard.Components;

/// <summary>
/// A log viewing UI component that shows a live view of a log, with syntax highlighting and automatic scrolling.
/// </summary>
public sealed partial class LogViewer
{
    private readonly TaskCompletionSource _whenDomReady = new();
    private readonly CancellationSeries _cancellationSeries = new();
    private IJSObjectReference? _jsModule;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _jsModule ??= await JS.InvokeAsync<IJSObjectReference>("import", "/_content/Dotnetydd.OtlpDashboard/Components/Controls/LogViewer.razor.js");

            _whenDomReady.TrySetResult();
        }
    }

    internal async Task SetLogSourceAsync(IAsyncEnumerable<IReadOnlyList<(string Content, bool IsErrorMessage)>> batches, bool convertTimestampsFromUtc)
    {
        var cancellationToken = await _cancellationSeries.NextAsync();
        var logParser = new LogParser(convertTimestampsFromUtc);

        // Ensure we are able to write to the DOM.
        await _whenDomReady.Task;

        await foreach (var batch in batches.WithCancellation(cancellationToken))
        {
            if (batch.Count is 0)
            {
                continue;
            }

            List<LogEntry> entries = new(batch.Count);

            foreach (var (content, isErrorOutput) in batch)
            {
                entries.Add(logParser.CreateLogEntry(content, isErrorOutput));
            }

            await _jsModule!.InvokeVoidAsync("addLogEntries", cancellationToken, entries);
        }
    }

    internal async Task ClearLogsAsync(CancellationToken cancellationToken = default)
    {
        await _cancellationSeries.ClearAsync();

        if (_jsModule is not null)
        {
            await _jsModule.InvokeVoidAsync("clearLogs", cancellationToken);
        }
    }

    public async ValueTask DisposeAsync()
    {
        _whenDomReady.TrySetCanceled();

        await _cancellationSeries.ClearAsync();

        try
        {
            if (_jsModule is not null)
            {
                await _jsModule.DisposeAsync();
            }
        }
        catch (JSDisconnectedException)
        {
            // Per https://learn.microsoft.com/aspnet/core/blazor/javascript-interoperability/?view=aspnetcore-7.0#javascript-interop-calls-without-a-circuit
            // this is one of the calls that will fail if the circuit is disconnected, and we just need to catch the exception so it doesn't pollute the logs
        }
    }
}
