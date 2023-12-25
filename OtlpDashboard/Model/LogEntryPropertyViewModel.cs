// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Dotnetydd.OtlpDashboard.Model;

public sealed class LogEntryPropertyViewModel
{
    public required string Name { get; init; }
    public required string Value { get; init; }
}
