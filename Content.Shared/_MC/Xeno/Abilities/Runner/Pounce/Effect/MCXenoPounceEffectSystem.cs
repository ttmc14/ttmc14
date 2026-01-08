using System.Numerics;
using Robust.Shared.Network;

namespace Content.Shared._MC.Xeno.Abilities.Runner.Pounce.Effect;

public sealed class MCXenoPounceEffectSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = null!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoPounceEffectComponent, MCXenoPounceStartEvent>(OnStart);
    }

    private void OnStart(Entity<MCXenoPounceEffectComponent> entity, ref MCXenoPounceStartEvent args)
    {
        if (_net.IsClient)
            return;

        var effectUid = Spawn(entity.Comp.EntityId, args.Origin.Offset(args.Direction * args.Distance / 2), rotation: args.Direction.ToAngle());
        var scale = float.Max(1, args.Distance * 32f / 200f);

        EnsureComp<ScaleVisualsComponent>(effectUid);

        _appearance.SetData(effectUid, ScaleVisuals.Scale, Vector2.Max(Vector2.One, args.Direction) * scale);
    }
}
