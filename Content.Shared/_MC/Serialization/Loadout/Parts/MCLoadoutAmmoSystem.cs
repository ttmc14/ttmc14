using Content.Shared._MC.Serialization.Loadout.Data;
using Content.Shared._RMC14.Attachable.Components;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Serialization.Loadout.Parts;

[Serializable, NetSerializable]
public sealed partial class MCLoadoutAmmo : MCLoadoutPart
{
    [DataField]
    public List<MCLoadoutItem> Rounds = new();

    public override IEnumerable<MCLoadoutItem> GetContents() => Rounds;
}

public sealed class MCLoadoutAmmoSystem : MCLoadoutPartSystem<BallisticAmmoProviderComponent, MCLoadoutAmmo>
{
    [Dependency] private readonly SharedGunSystem _gun = null!;

    protected override MCLoadoutAmmo? Capture(Entity<BallisticAmmoProviderComponent> entity)
    {
        if (!HasComp<AttachableComponent>(entity))
            return null;

        var part = new MCLoadoutAmmo();
        foreach (var round in entity.Comp.Entities)
        {
            if (Serializer.BuildItem(round) is { } child)
                part.Rounds.Add(child);
        }

        return part;
    }

    protected override void Restore(Entity<BallisticAmmoProviderComponent> entity, MCLoadoutAmmo part, MCLoadoutRestoreContext context)
    {
        if (!HasComp<AttachableComponent>(entity))
            return;

        foreach (var round in part.Rounds)
        {
            if (entity.Comp.Count >= entity.Comp.Capacity)
                break;

            if (context.Vend(round) is { } child)
                _gun.TryAmmoInsert(entity, entity.Comp, child, context.User, entity, 0);
        }
    }
}
