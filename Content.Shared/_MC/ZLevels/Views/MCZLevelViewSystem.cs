using Content.Shared._CE.ZLevels.Core.Components;
using Content.Shared._CE.ZLevels.Core.EntitySystems;
using Robust.Shared.Map;

namespace Content.Shared._MC.ZLevels.Views;

public sealed class MCZLevelViewSystem : EntitySystem
{
    [Dependency] private readonly CESharedZLevelsSystem _zLevels = null!;

    public int GetRequestedShotOffset(EntityUid uid, EntityCoordinates targetCoordinates)
    {
        if (!TryComp<CEZLevelViewerComponent>(uid, out var viewerComponent))
            return 0;

        if (viewerComponent.LookUp)
            return 1;

        if (_zLevels.IsVoidAtCoordinates(targetCoordinates, out _))
            return -1;

        return 0;
    }
}
