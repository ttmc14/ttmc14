using System.Diagnostics.CodeAnalysis;

namespace Content.Shared._MC.AI;

public sealed partial class MCAIMemory
{
    public IReadOnlyDictionary<string, object> Container => _container;

    [ViewVariables]
    private readonly Dictionary<string, object> _container = new();

    public void ContainerRemove<T>(string key) where T : notnull
    {
        if (!_container.TryGetValue(key, out var obj) || obj is not T)
            return;

        _container.Remove(key);
    }

    public void ContainerRemove(List<string> list)
    {
        foreach (var key in list)
        {
            _container.Remove(key);
        }
    }

    public void ContainerSet<T>(string key, T value) where T : notnull
    {
        _container[key] = value;
    }

    public bool ContainerTryGet<T>(string key, [NotNullWhen(true)] out T? value) where T : notnull
    {
        value = default;

        if (!_container.TryGetValue(key, out var obj))
            return false;

        if (obj is not T casted)
            return false;

        value = casted;
        return true;
    }

    private void ContainerClear()
    {
        _container.Clear();
    }
}
