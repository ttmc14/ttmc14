using Robust.Shared.GameStates;

namespace Content.Shared._MC.ASRS.Systems;

public sealed class MCASRSSystem : MCEntitySystemSingleton<MCASRSSingletonComponent>
{
    public int Points
    {
        get => Inst.Comp.Points;
        set
        {
            Inst.Comp.Points = value;
            Dirty(Inst);
        }
    }

    public void RemovePoints(int cost)
    {
        Points -= cost;
    }
}

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCASRSSingletonComponent : Component
{
#if FULL_RELEASE
    [DataField, AutoNetworkedField]
    public int Points;
#else
    [DataField, AutoNetworkedField]
    public int Points = 1000000;
#endif
}
