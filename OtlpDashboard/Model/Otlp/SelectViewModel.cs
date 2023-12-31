// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Dotnetydd.OtlpDevDashboard.Model.Otlp;

public class SelectViewModel<T>
{
    public required string Name { get; init; }
    public required T? Id { get; init; }
}
