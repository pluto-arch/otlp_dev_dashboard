// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Dotnetydd.OtlpDevDashboard.Model;

public interface IOutgoingPeerResolver
{
    bool TryResolvePeerName(KeyValuePair<string, string>[] attributes, out string? name);
    IDisposable OnPeerChanges(Func<Task> callback);
}
