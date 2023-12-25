// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Dotnetydd.OtlpDashboard.Otlp.Model;
using Dotnetydd.OtlpDashboard.Otlp.Storage;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Proto.Collector.Logs.V1;
using OpenTelemetry.Proto.Resource.V1;
using static System.Net.Mime.MediaTypeNames;

namespace Dotnetydd.OtlpDashboard.Otlp.Grpc;

public class OtlpLogsService : LogsService.LogsServiceBase
{
    private readonly ILogger<OtlpLogsService> _logger;
    private readonly TelemetryRepository _telemetryRepository;

    public OtlpLogsService(ILogger<OtlpLogsService> logger, TelemetryRepository telemetryRepository)
    {
        _logger = logger;
        _telemetryRepository = telemetryRepository;
    }

    public override Task<ExportLogsServiceResponse> Export(ExportLogsServiceRequest request, ServerCallContext context)
    {
        var addContext = new AddContext();
        _telemetryRepository.AddLogs(addContext, request.ResourceLogs);
        return Task.FromResult(new ExportLogsServiceResponse
        {
            PartialSuccess = new ExportLogsPartialSuccess
            {
                RejectedLogRecords = addContext.FailureCount
            }
        });
    }
}
