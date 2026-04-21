namespace Content.Shared._MC.AI;

public sealed partial class MCAIMemory
{
    [ViewVariables]
    private readonly Dictionary<string, bool> _state = new();

    [ViewVariables]
    private int _stateHash = -1;

    #region States

    public void StateSet(string key, bool value)
    {
        if (_state.TryGetValue(key, out var oldValue) && oldValue == value)
            return;

        _state[key] = value;
        _stateHash = -1;
    }

    public void StateSet(Dictionary<string, bool> state)
    {
        _state.Clear();
        _stateHash = -1;

        foreach (var (key, value) in state)
        {
            _state[key] = value;
        }
    }

    public bool StateHas(string key, bool value = true)
    {
        return _state.TryGetValue(key, out var current) && current == value;
    }

    public void StateRemove(string key)
    {
        _state.Remove(key);
        _stateHash = -1;
    }

    public void StateClear()
    {
        _state.Clear();
        _stateHash = -1;
    }

    public Dictionary<string, bool> StateCopy()
    {
        return new Dictionary<string, bool>(_state);
    }

    #endregion

    public void StateWriteKeys(Dictionary<string, bool> preconditions)
    {
        foreach (var (key, _) in preconditions)
        {
            _state[key] = false;
        }
    }

    public bool StateCheckPreconditions(Dictionary<string, bool> preconditions)
    {
        foreach (var (key, value) in preconditions)
        {
            if (!_state.TryGetValue(key, out var current) || current != value)
                return false;
        }

        return true;
    }

    public int StateGetHashCode()
    {
        if (_stateHash != -1)
            return _stateHash;

        var hash = 17;
        foreach (var (_, value) in _state)
        {
            hash = hash * 31 + (value ? 1 : 0);
        }

        _stateHash = hash;
        return _stateHash;
    }
}
