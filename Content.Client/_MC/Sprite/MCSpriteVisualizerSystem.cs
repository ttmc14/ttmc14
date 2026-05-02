using Content.Shared._MC.Sprite;
using Content.Shared.Clothing.Components;
using Content.Shared.Item;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Shared.Serialization.TypeSerializers.Implementations;

namespace Content.Client._MC.Sprite;

public sealed class MCSpriteVisualizerSystem : VisualizerSystem<MCSpriteChangerComponent>
{
    [Dependency] private readonly IResourceCache _resource = null!;

    private readonly List<EntityUid> _toRemove = new();
    private readonly Dictionary<EntityUid, RSI> _originalPaths = new();

    protected override void OnAppearanceChange(EntityUid uid, MCSpriteChangerComponent component, ref AppearanceChangeEvent args)
    {
        base.OnAppearanceChange(uid, component, ref args);

        if (!_originalPaths.ContainsKey(uid) && args.Sprite?.BaseRSI is { } originalRsi)
            _originalPaths[uid] = originalRsi;

#pragma warning disable RA0002
        if (args.Sprite?.BaseRSI is not null && _resource.TryGetResource(SpriteSpecifierSerializer.TextureRoot / component.Path, out RSIResource? baseRsi))
            SpriteSystem.SetBaseRsi(uid, baseRsi.RSI);

        if (TryComp<ClothingComponent>(uid, out var clothingComponent))
            clothingComponent.RsiPath = component.Path.ToString();

        if (TryComp<ItemComponent>(uid, out var itemComponent))
            itemComponent.RsiPath = component.Path.ToString();
#pragma warning restore RA0002
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        _toRemove.Clear();
        foreach (var (uid, rsi) in _originalPaths)
        {
            if (TerminatingOrDeleted(uid))
            {
                _toRemove.Add(uid);
                continue;
            }

            if (HasComp<MCSpriteChangerComponent>(uid))
                continue;

            _toRemove.Add(uid);

            if (!TryComp<SpriteComponent>(uid, out var spriteComponent))
                continue;

            SpriteSystem.SetBaseRsi((uid, spriteComponent), rsi);
        }

        foreach (var uid in _toRemove)
        {
            _originalPaths.Remove(uid);
        }
    }

    public override void Shutdown()
    {
        _toRemove.Clear();
        _originalPaths.Clear();
    }
}
