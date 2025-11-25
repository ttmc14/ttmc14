using Content.Shared._RMC14.Emote;

namespace Content.Shared._MC.Xeno.Abilities.Pounce.Emote;

public sealed class MCXenoPounceEmoteSystem : EntitySystem
{
    [Dependency] private readonly SharedRMCEmoteSystem _emote = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoPounceEmoteComponent, MCXenoPounceHitEvent>(OnHit);
    }

    private void OnHit(Entity<MCXenoPounceEmoteComponent> entity, ref MCXenoPounceHitEvent args)
    {
        if (!args.First)
            return;

        _emote.TryEmoteWithChat(entity.Owner, entity.Comp.Emote, forceEmote: true);
    }
}
