// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;

namespace Dotnetydd.OtlpDevDashboard.Model.Otlp;

public class LogDialogFormModel
{
    [Required]
    public string? Parameter { get; set; }
    [Required]
    public SelectViewModel<FilterCondition>? Condition { get; set; }
    [Required]
    public string? Value { get; set; }
}
