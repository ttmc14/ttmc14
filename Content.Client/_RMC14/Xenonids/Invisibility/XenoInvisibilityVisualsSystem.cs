using Content.Shared._RMC14.Xenonids.Invisibility;
using Robust.Client.GameObjects;

namespace Content.Client._RMC14.Xenonids.Invisibility;

public sealed class XenoInvisibilityVisualsSystem : EntitySystem
{
    private EntityQuery<XenoActiveInvisibleComponent> _activeInvisibleQuery;

    public override void Initialize()
    {
        _activeInvisibleQuery = GetEntityQuery<XenoActiveInvisibleComponent>();
    }

    public override void Update(float frameTime)
    {
        // MC Changes
        var invisible = EntityQueryEnumerator<SpriteComponent>();
        while (invisible.MoveNext(out var uid, out var sprite))
        {
            var opacity = _activeInvisibleQuery.TryComp(uid, out var invisibleComponent) ? invisibleComponent.Opacity ?? 1 : 1;
            sprite.Color = Color.Transparent.WithAlpha(opacity);
        }
        // MC End
    }
}
