namespace Content.Server._MC;

public sealed class MCRelayedSystem : EntitySystem
{
    private readonly List<Action> _actions = new();

    public void Relay(Action action)
    {
        _actions.Add(action);
    }

    public override void Update(float frameTime)
    {
        foreach (var action in _actions)
        {
            action();
        }

        _actions.Clear();
    }
}
