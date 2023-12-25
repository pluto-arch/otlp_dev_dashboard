// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Dotnetydd.OtlpDevDashboard.Model.Otlp;

public class FilterDialogResult
{
    public LogFilter? Filter { get; set; }
    public bool Delete { get; set; }
    public bool Add { get; set; }
}
