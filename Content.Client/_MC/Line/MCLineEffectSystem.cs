using System.Numerics;
using Content.Shared._MC.Line;
using Robust.Client.GameObjects;

namespace Content.Client._MC.Line;

public sealed class MCLineEffectSystem : EntitySystem
{
    [Dependency] private readonly SpriteSystem _sprite = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<MCLineEffectEvent>(OnEffect);
    }

    private void OnEffect(MCLineEffectEvent ev)
    {
        foreach (var data in ev.Data)
        {
            var coords = GetCoordinates(data.Coordinates);
            var uid = Spawn(data.ProtoId, coords);

            if (!TryComp<SpriteComponent>(uid, out var sprite))
                continue;

            var entity = (uid, sprite);

            _sprite.LayerSetSprite(entity, 0, data.Sprite);
            _sprite.SetRotation(entity, data.Angle);
            _sprite.SetScale(entity, new Vector2(data.Scale, 1f));
        }
    }
}
