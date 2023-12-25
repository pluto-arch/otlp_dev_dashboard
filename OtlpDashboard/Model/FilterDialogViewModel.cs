// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Dotnetydd.OtlpDashboard.Model.Otlp;

namespace Dotnetydd.OtlpDashboard.Model;

public sealed class FilterDialogViewModel
{
    public required LogFilter? Filter { get; init; }
    public required List<string> LogPropertyKeys { get; init; }
}
