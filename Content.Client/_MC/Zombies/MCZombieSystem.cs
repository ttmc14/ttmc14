using System.Linq;
using Content.Shared.Ghost;
using Content.Shared.Humanoid;
using Content.Shared.StatusIcon;
using Content.Shared.StatusIcon.Components;
using Content.Shared._MC.Zombies;
using Robust.Client.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Client._MC.Zombies;

public sealed class MCZombieSystem : SharedMCZombieSystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCZombieComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<MCZombieComponent, GetStatusIconsEvent>(GetZombieIcon);
        SubscribeLocalEvent<MCInitialInfectedComponent, GetStatusIconsEvent>(GetInitialInfectedIcon);
    }

    private void GetZombieIcon(Entity<MCZombieComponent> ent, ref GetStatusIconsEvent args)
    {
        var iconPrototype = _prototype.Index(ent.Comp.StatusIcon);
        args.StatusIcons.Add(iconPrototype);
    }

    private void GetInitialInfectedIcon(Entity<MCInitialInfectedComponent> ent, ref GetStatusIconsEvent args)
    {
        if (HasComp<MCZombieComponent>(ent))
            return;

        var iconPrototype = _prototype.Index(ent.Comp.StatusIcon);
        args.StatusIcons.Add(iconPrototype);
    }

    private void OnStartup(EntityUid uid, MCZombieComponent component, ComponentStartup args)
    {
        // Change their appearance to look like a zombie
        if (TryComp<HumanoidAppearanceComponent>(uid, out var humanoidComp))
        {
            _sprite.SetColor(uid, Color.White);
        }
    }
}
