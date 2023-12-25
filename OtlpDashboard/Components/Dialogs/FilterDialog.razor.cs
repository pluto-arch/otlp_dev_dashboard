// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Dotnetydd.OtlpDevDashboard.Model;
using Dotnetydd.OtlpDevDashboard.Model.Otlp;
using Dotnetydd.OtlpDevDashboard.Otlp.Storage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.FluentUI.AspNetCore.Components;

namespace Dotnetydd.OtlpDevDashboard.Components.Dialogs;

public partial class FilterDialog
{
    private static readonly List<SelectViewModel<FilterCondition>> s_filterConditions = new()
    {
        CreateFilterSelectViewModel(FilterCondition.Equals),
        CreateFilterSelectViewModel(FilterCondition.Contains),
        CreateFilterSelectViewModel(FilterCondition.NotEqual),
        CreateFilterSelectViewModel(FilterCondition.NotContains),
    };

    private static SelectViewModel<FilterCondition> CreateFilterSelectViewModel(FilterCondition condition) =>
        new SelectViewModel<FilterCondition> { Id = condition, Name = LogFilter.ConditionToString(condition) };

    [CascadingParameter]
    public FluentDialog? Dialog { get; set; }

    [Parameter]
    public FilterDialogViewModel Content { get; set; } = default!;

    [Inject]
    public required TelemetryRepository TelemetryRepository { get; set; }

    private LogDialogFormModel _formModel = default!;
    public EditContext EditContext { get; private set; } = default!;

    protected override void OnInitialized()
    {
        _formModel = new LogDialogFormModel();
        EditContext = new EditContext(_formModel);

        if (Content.Filter is { } logFilter)
        {
            _formModel.Parameter = logFilter.Field;
            _formModel.Condition = s_filterConditions.Single(c => c.Id == logFilter.Condition);
            _formModel.Value = logFilter.Value;
        }
        else
        {
            _formModel.Parameter = "Message";
            _formModel.Condition = s_filterConditions.Single(c => c.Id == FilterCondition.Contains);
            _formModel.Value = "";
        }
    }

    public List<string> Parameters => LogFilter.GetAllPropertyNames(Content.LogPropertyKeys);

    private void Cancel()
    {
        Dialog!.CancelAsync();
    }

    private void Delete()
    {
        Dialog!.CloseAsync(DialogResult.Ok(new FilterDialogResult() { Filter = Content.Filter, Delete = true }));
    }

    private void Apply()
    {
        if (Content.Filter is { } logFilter)
        {
            logFilter.Field = _formModel.Parameter!;
            logFilter.Condition = _formModel.Condition!.Id;
            logFilter.Value = _formModel.Value!;

            Dialog!.CloseAsync(DialogResult.Ok(new FilterDialogResult() { Filter = logFilter, Delete = false }));
        }
        else
        {
            var filter = new LogFilter
            {
                Field = _formModel.Parameter!,
                Condition = _formModel.Condition!.Id,
                Value = _formModel.Value!
            };

            Dialog!.CloseAsync(DialogResult.Ok(new FilterDialogResult() { Filter = filter, Add = true }));
        }
    }
}
