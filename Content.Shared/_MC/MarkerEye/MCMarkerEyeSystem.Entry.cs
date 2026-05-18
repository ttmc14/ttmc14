using Content.Shared._MC.MarkerEye.Components;
using Content.Shared.Interaction;

namespace Content.Shared._MC.MarkerEye;

public sealed partial class MCMarkerEyeSystem
{
    private void InitializeEntry()
    {
        SubscribeLocalEvent<MCMarkerEyeStartOnInteractComponent, InteractHandEvent>(OnInteractEntry);
    }

    private void OnInteractEntry(Entity<MCMarkerEyeStartOnInteractComponent> entity, ref InteractHandEvent args)
    {
        TryStartWatch(args.User, entity.Comp.EyePrototype);
    }
}
