// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Dotnetydd.OtlpDevDashboard.Otlp.Storage;

public sealed class GetTracesRequest
{
    public required string? ApplicationServiceId { get; init; }
    public required int StartIndex { get; init; }
    public required int? Count { get; init; }
    public required string FilterText { get; init; }
}
