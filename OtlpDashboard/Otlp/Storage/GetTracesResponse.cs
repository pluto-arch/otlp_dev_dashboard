﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Dotnetydd.OtlpDashboard.Otlp.Model;

namespace Dotnetydd.OtlpDashboard.Otlp.Storage;

public sealed class GetTracesResponse
{
    public required PagedResult<OtlpTrace> PagedResult { get; init; }
    public required TimeSpan MaxDuration { get; init; }
}
