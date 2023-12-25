using System.Diagnostics;

namespace Dotnetydd.OtlpDevDashboard.ApplicationModels;


[DebuggerDisplay("Name = {Name}, Resources = {Resources.Count}")]
public class DistributedApplicationModel(IResourceCollection resources)
{
    /// <summary>
    /// Gets the collection of resources associated with the distributed application.
    /// </summary>
    public IResourceCollection Resources { get; } = resources;

    /// <summary>
    /// Gets or sets the name of the distributed application.
    /// </summary>
    public string? Name { get; set; }
}