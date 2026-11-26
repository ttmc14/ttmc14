using System.Numerics;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Prototypes;
using Robust.Shared.Spawners;

namespace Content.Client._MC.Effects;

public sealed class MCWraithWarpSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototypes = null!;
    [Dependency] private readonly SpriteSystem _sprites = null!;

    public override void Initialize()
    {
        SubscribeLocalEvent<MCWraithWarpComponent, ComponentStartup>(OnStartup);
    }

    private void OnStartup(Entity<MCWraithWarpComponent> ent, ref ComponentStartup args)
    {
        ent.Comp.Duration = Comp<TimedDespawnComponent>(ent).Lifetime;
        Comp<SpriteComponent>(ent).PostShader = _prototypes.Index<ShaderPrototype>("MCWraithWarp").InstanceUnique();
    }

    public override void FrameUpdate(float frameTime)
    {
        var query = EntityQueryEnumerator<MCWraithWarpComponent, TimedDespawnComponent, SpriteComponent>();
        while (query.MoveNext(out var uid, out var warp, out var despawn, out var sprite))
        {
            var remaining = Math.Clamp(despawn.Lifetime / warp.Duration, 0f, 1f);
            _sprites.SetScale((uid, sprite), new Vector2(0.1f + 0.9f * remaining));
        }
    }
}
