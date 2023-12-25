using System.Collections.ObjectModel;

namespace Dotnetydd.OtlpDevDashboard.ApplicationModels;

/// <summary>
/// Represents a collection of resource metadata annotations.
/// </summary>
public sealed class ResourceMetadataCollection : Collection<IResourceAnnotation>
{
}