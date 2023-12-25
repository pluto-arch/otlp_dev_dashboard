namespace Dotnetydd.OtlpDashboard.ApplicationModels;

public interface IResource
{
    /// <summary>
    /// Gets the name of the resource.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the annotations associated with the resource.
    /// </summary>
    ResourceMetadataCollection Annotations { get; }
}