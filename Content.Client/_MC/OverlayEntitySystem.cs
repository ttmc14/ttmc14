using Robust.Client.Graphics;

namespace Content.Client._MC;

public abstract class OverlayEntitySystem<T> : EntitySystem where T : Overlay
{
    [Dependency] private readonly IOverlayManager _overlay = null!;

    public abstract T Overlay { get; }

    public override void Initialize()
    {
        if (!_overlay.HasOverlay<T>())
            _overlay.AddOverlay(Overlay);

        // Ha-ha funny
        // [ERRO] res.typecheck: Sandbox violation: Access to type not allowed: [System.Runtime]System.Activator
        //
        // _overlay.AddOverlay(new T());
    }

    public override void Shutdown()
    {
        _overlay.RemoveOverlay<T>();
    }
}
