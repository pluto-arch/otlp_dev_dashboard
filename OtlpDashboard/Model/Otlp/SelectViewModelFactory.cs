// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Dotnetydd.OtlpDashboard.Otlp.Model;

namespace Dotnetydd.OtlpDashboard.Model.Otlp;

public class SelectViewModelFactory
{
    public static List<SelectViewModel<string>> CreateApplicationsSelectViewModel(List<OtlpApplication> applications)
    {
        return applications.Select(a => new SelectViewModel<string>
        {
            Id = a.InstanceId,
            Name = OtlpApplication.GetResourceName(a, applications)
        }).ToList();
    }
}