using Content.Shared._MC.Stamina;
using Content.Shared._MC.Xeno.Hive.Systems;
using Content.Shared.Damage;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Timing;

namespace Content.Shared._MC.Xeno.Abilities.Warlock.PsyCrush;

public sealed class MCXenoPsyCrushSystem : MCXenoAbilitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = null!;
    [Dependency] private readonly SharedStunSystem _stun = null!;
    [Dependency] private readonly SharedPopupSystem _popup = null!;
    [Dependency] private readonly IGameTiming _timing = null!;
    [Dependency] private readonly EntityLookupSystem _lookup = null!;
    [Dependency] private readonly DamageableSystem _damageable = null!;
    [Dependency] private readonly SharedTransformSystem _transform = null!;
    [Dependency] private readonly SharedPhysicsSystem _physics = null!;

    [Dependency] private readonly MCSharedXenoHiveSystem _mcXenoHive = null!;
    [Dependency] private readonly MCStaminaSystem _mcStamina = null!;

    private const float ChannelTick = 0.6f;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoPsyCrushComponent, MCXenoPsyCrushActionEvent>(OnAction);
    }

    private void OnAction(Entity<MCXenoPsyCrushComponent> entity, ref MCXenoPsyCrushActionEvent args)
    {
        if (args.Handled)
            return;

        if (!CanUseAction(entity, args.Action))
            return;
    }
}
