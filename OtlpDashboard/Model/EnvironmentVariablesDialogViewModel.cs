// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Dotnetydd.OtlpDashboard.Model;

public class EnvironmentVariablesDialogViewModel
{
    public required List<EnvironmentVariableViewModel> EnvironmentVariables { get; init; }
    public bool ShowSpecOnlyToggle { get; set; }
}
