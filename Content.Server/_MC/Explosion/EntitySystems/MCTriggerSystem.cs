using Content.Server._MC.Bomb.Components;
using Content.Server._MC.Bomb.Systems;
using Content.Server.Explosion.Components;
using Content.Server.Explosion.EntitySystems;
using Content.Server.Popups;
using Content.Shared._MC.Bomb.Components;
using Content.Shared.Examine;
using Content.Shared.Explosion.Components;
using Content.Shared.Interaction.Events;
using Content.Shared.Popups;
using Content.Shared.Sticky;
using Content.Shared.Verbs;
using Robust.Shared.GameObjects;
using Robust.Shared.Random;

namespace Content.Server.Explosion.EntitySystems;

public sealed partial class MCTriggerSystem : EntitySystem
{
    [Dependency] private readonly MCBombPasswordSystem _bombPassword = default!;
    [Dependency] private readonly MCDefusableSystem _defusable = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly TriggerSystem _triggerSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        InitializeOnUse();
    }
}
