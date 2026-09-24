using Robust.Client.GameObjects;
using Robust.Client.ResourceManagement;
using Robust.Shared.ContentPack;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations;
using Robust.Shared.Utility;

namespace Content.Client._MC;

public abstract class MCVisualizerSystem<T> : VisualizerSystem<T>
    where T: Component
{
    protected readonly ResPath TextureRoot = SpriteSpecifierSerializer.TextureRoot;

    [Dependency] protected readonly IPrototypeManager PrototypeManager = null!;
    [Dependency] protected readonly IResourceManager ResourceManager = null!;
    [Dependency] protected readonly IResourceCache ResourceCache = null!;

    protected virtual void OnAppearanceChange(Entity<T> entity, ref AppearanceChangeEvent args)
    {
    }

    protected override void OnAppearanceChange(EntityUid uid, T component, ref AppearanceChangeEvent args)
    {
        OnAppearanceChange((uid, component), ref args);
    }
}
