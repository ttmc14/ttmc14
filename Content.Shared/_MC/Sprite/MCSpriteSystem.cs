using JetBrains.Annotations;
using Robust.Shared.Utility;

namespace Content.Shared._MC.Sprite;

public sealed class MCSpriteSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = null!;

    [PublicAPI]
    public void Change(Entity<MCSpriteChangerComponent?> entity, ResPath path)
    {
        EnsureComp<MCSpriteChangerComponent>(entity).Path = path;
        Update(entity);
    }

    [PublicAPI]
    public void Reset(Entity<MCSpriteChangerComponent?> entity)
    {
        RemComp<MCSpriteChangerComponent>(entity);
        Update(entity);
    }

    private void Update(Entity<MCSpriteChangerComponent?> entity)
    {
        Dirty(entity);

        var appearanceComponent = EnsureComp<AppearanceComponent>(entity);
        _appearance.QueueUpdate(entity.Owner, appearanceComponent);
    }
}
