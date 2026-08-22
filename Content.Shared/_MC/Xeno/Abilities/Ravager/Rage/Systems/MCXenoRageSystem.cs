using Content.Shared._MC.Xeno.Abilities.Ravager.Rage.Components;

namespace Content.Shared._MC.Xeno.Abilities.Ravager.Rage.Systems;

public sealed partial class MCXenoRageSystem : MCXenoAbilitySystem
{
    private EntityQuery<MCXenoRageComponent> _query;
    private EntityQuery<MCXenoRageActiveComponent> _queryActive;

    public override void Initialize()
    {
        base.Initialize();

        _query = GetEntityQuery<MCXenoRageComponent>();
        _queryActive = GetEntityQuery<MCXenoRageActiveComponent>();
    }
}
