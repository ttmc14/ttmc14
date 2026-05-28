using System.Numerics;
using Content.Client.Damage.Systems;
using Content.Shared._CE.ZLevels.Core.Components;
using Content.Shared._CE.ZLevels.Core.EntitySystems;
using Content.Shared.Camera;
using Content.Shared.Damage.Components;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;

namespace Content.Client._CE.ZLevels.Core;

/// <summary>
/// Only process Eye offset and drawdepth on clientside
/// </summary>
public sealed partial class CEClientZLevelsSystem : CESharedZLevelsSystem
{
    [Dependency] private readonly IOverlayManager _overlay = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;
    [Dependency] private readonly IEyeManager _eye = default!;

    public override void Initialize()
    {
        base.Initialize();
        _overlay.AddOverlay(new CEZLevelBlurOverlay());

        SubscribeLocalEvent<CEZPhysicsComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<CEZPhysicsComponent, GetEyeOffsetEvent>(OnEyeOffset);
    }

    private void OnEyeOffset(Entity<CEZPhysicsComponent> ent, ref GetEyeOffsetEvent args)
    {
        Angle rotation = _eye.CurrentEye.Rotation * -1;
        var localPosition = ent.Comp.LocalPosition;
        var offset = rotation.RotateVec(new Vector2(0, localPosition * ZLevelOffset));
        args.Offset += offset;
    }
    
    private void OnStartup(Entity<CEZPhysicsComponent> ent, ref ComponentStartup args)
    {
        if (!TryComp<SpriteComponent>(ent, out var sprite))
            return;

        if (sprite.SnapCardinals)
            return;

        ent.Comp.NoRotDefault = sprite.NoRotation;
        ent.Comp.DrawDepthDefault = sprite.DrawDepth;
        ent.Comp.SpriteOffsetDefault = sprite.Offset;
    }

    public override void Shutdown()
    {
        base.Shutdown();
        _overlay.RemoveOverlay<CEZLevelBlurOverlay>();
    }
}

internal sealed class CEClientZLevelsPreAnimSystem : EntitySystem
{
    [Dependency] private readonly SpriteSystem _sprite = default!;

    public override void Initialize()
    {
        base.Initialize();
        UpdatesBefore.Add(typeof(AnimationPlayerSystem));
    }

    public override void FrameUpdate(float frameTime)
    {
        var query = EntityQueryEnumerator<CEZPhysicsComponent, SpriteComponent>();
        while (query.MoveNext(out var uid, out var zPhys, out var sprite))
        {
            var localPosition = zPhys.LocalPosition;
            sprite.NoRotation = localPosition != 0 || zPhys.NoRotDefault;
            _sprite.SetOffset((uid, sprite), zPhys.SpriteOffsetDefault);
            _sprite.SetDrawDepth((uid, sprite), localPosition > 0 ? (int)Shared.DrawDepth.DrawDepth.OverMobs : zPhys.DrawDepthDefault);
        }
    }
}

internal sealed class CEClientZLevelsPostAnimSystem : EntitySystem
{
    [Dependency] private readonly SpriteSystem _sprite = default!;

    public override void Initialize()
    {
        base.Initialize();
        UpdatesAfter.Add(typeof(AnimationPlayerSystem));
    }

    public override void FrameUpdate(float frameTime)
    {
        var query = EntityQueryEnumerator<CEZPhysicsComponent, SpriteComponent>();
        while (query.MoveNext(out var uid, out var zPhys, out var sprite))
        {
            var zOffset = new Vector2(0, zPhys.LocalPosition * CESharedZLevelsSystem.ZLevelOffset);
            _sprite.SetOffset((uid, sprite), sprite.Offset + zOffset);
        }
    }
}
