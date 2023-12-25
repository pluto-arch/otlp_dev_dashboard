using System.Diagnostics;
using System.Collections;

namespace Dotnetydd.OtlpDashboard.ApplicationModels;

[DebuggerDisplay("Count = {Count}")]
[DebuggerTypeProxy(typeof(ApplicationResourceCollectionDebugView))]
internal sealed class ResourceCollection : IResourceCollection
{
    private readonly List<IResource> _resources = [];

    public IResource this[int index] { get => _resources[index]; set => _resources[index] = value; }
    public int Count => _resources.Count;
    public bool IsReadOnly => false;
    public void Add(IResource item) => _resources.Add(item);
    public void Clear() => _resources.Clear();
    public bool Contains(IResource item) => _resources.Contains(item);
    public void CopyTo(IResource[] array, int arrayIndex) => _resources.CopyTo(array, arrayIndex);
    public IEnumerator<IResource> GetEnumerator() => _resources.GetEnumerator();
    public int IndexOf(IResource item) => _resources.IndexOf(item);
    public void Insert(int index, IResource item) => _resources.Insert(index, item);
    public bool Remove(IResource item) => _resources.Remove(item);
    public void RemoveAt(int index) => _resources.RemoveAt(index);
    IEnumerator IEnumerable.GetEnumerator() => _resources.GetEnumerator();

    private sealed class ApplicationResourceCollectionDebugView(ResourceCollection collection)
    {
        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public IResource[] Items => collection.ToArray();
    }
}