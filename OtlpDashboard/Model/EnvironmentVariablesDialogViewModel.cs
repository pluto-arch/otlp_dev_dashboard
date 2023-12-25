// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Dotnetydd.OtlpDevDashboard.Model;

public class EnvironmentVariablesDialogViewModel
{
    public required List<EnvironmentVariableViewModel> EnvironmentVariables { get; init; }
    public bool ShowSpecOnlyToggle { get; set; }
}
