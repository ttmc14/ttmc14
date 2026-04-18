using Content.Shared._MC.Armor.Events;
using Content.Shared.StatusEffectNew;

namespace Content.Shared._MC.StatusEffects.Shatter;

public sealed class MCShatterSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCShatterComponent, MCArmorModifyEvent>(OnArmorGet);
        SubscribeLocalEvent<MCShatterComponent, StatusEffectRelayedEvent<MCArmorModifyEvent>>(OnArmorGetRelayed);
    }

    private static void OnArmorGet(Entity<MCShatterComponent> entity, ref MCArmorModifyEvent args)
    {
        args.SoftArmor *= entity.Comp.Modifier;
    }

    private static void OnArmorGetRelayed(Entity<MCShatterComponent> entity, ref StatusEffectRelayedEvent<MCArmorModifyEvent> args)
    {
        args.Args.SoftArmor *= entity.Comp.Modifier;
    }
}
