namespace Content.Shared._MC.AI.Modules;

[ImplicitDataDefinitionForInheritors, Serializable]
public sealed partial class MCGoal
{
    [DataField]
    public float Priority = 1f;

    [DataField]
    public Dictionary<string, bool> Preconditions = new();

    [DataField]
    public Dictionary<string, bool> DesiredState = new();

    [DataField]
    public TimeSpan Delay = TimeSpan.Zero;

    public TimeSpan LastActivationTime = TimeSpan.Zero;
}
