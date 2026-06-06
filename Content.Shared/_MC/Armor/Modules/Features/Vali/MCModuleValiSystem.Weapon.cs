using Content.Shared._MC.Armor.Modules.Core.Events;
using Content.Shared._MC.Armor.Modules.Features.Vali.Components;
using Content.Shared._MC.Armor.Vali.Events;
using Content.Shared._MC.Weapon.Vali.Events;
using Content.Shared._RMC14.Xenonids;
using Content.Shared.Interaction.Components;

namespace Content.Shared._MC.Armor.Modules.Features.Vali;

public sealed partial class MCModuleValiSystem
{
    private void OnMeleeHit(Entity<MCModuleValiComponent> entity, ref MCArmorModuleRelayedEvent<MCWeaponValiMeleeHitEvent> args)
    {
        if (entity.Comp.ConnectedWeaponUid is null)
            return;

        foreach (var uid in args.Args.HitEntities)
        {
            // I don't like this shit, but it here
            // Later move this better
            // Xd
            if (!HasComp<XenoComponent>(uid))
                continue;

            AddResource(entity, entity.Comp.ConnectedWeaponHarvestAmount);
        }
    }

    private void OnDeattached(Entity<MCModuleValiComponent> entity, ref MCArmorModuleDetachedEvent args)
    {
        TryDeattachWeapon(entity);
    }

    private bool TryDeattachWeapon(Entity<MCModuleValiComponent> entity)
    {
        if (entity.Comp.ConnectedWeaponUid is not { } weaponUid)
            return false;

        entity.Comp.ConnectedWeaponUid = null;
        entity.Comp.ConnectedWeaponHarvestAmount = 0;
        Dirty(entity);

        RemComp<UnremoveableComponent>(weaponUid);

        if (_mcArmorModuleShared.GetUser(entity) is not { } userUid)
            return true;

        ActionSetToggled<MCModuleValiConnectActionEvent>(userUid, false);
        return true;
    }
}
