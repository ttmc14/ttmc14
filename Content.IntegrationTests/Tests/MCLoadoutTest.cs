#nullable enable
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Content.Client.Storage;
using Content.Shared._MC.Armor.Modules.Core;
using Content.Shared._MC.Armor.Modules.Core.Components;
using Content.Shared._MC.Engineering.Vending;
using Content.Shared._MC.Serialization.Loadout;
using Content.Shared._MC.Serialization.Loadout.Data;
using Content.Shared._MC.Serialization.Loadout.Parts;
using Content.Shared._RMC14.Attachable.Components;
using Content.Shared._RMC14.Attachable.Systems;
using Content.Shared._RMC14.Webbing;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Inventory;
using Content.Shared.Storage;
using Content.Shared.Storage.EntitySystems;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.IntegrationTests.Tests;

[TestFixture]
public sealed class MCLoadoutTest
{
    private const string Prototypes = """
        - type: entity
          id: TestMCLoadoutVendor
          components:
          - type: CMAutomatedVendor
            sections:
            - name: Equipment
              entries:
              - id: MCArmorModularJaegerScout
              - id: MCArmorModuleStorageGeneral
              - id: MCWeaponP23
              - id: MCAttachmentRedDotSight
              - id: MCAidKitFirstFilled
              - id: MCAttachmentGrenadeLauncher
              - id: MCGrenadeHefaM15
              - id: MCTagilaPants
              - id: MCWebbingSurgeryFilled
        """;

    [Test]
    public async Task WebbingRoundTrip()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings { NoLoadTestPrototypes = true });
        await pair.LoadPrototypes([Prototypes]);
        var server = pair.Server;
        var map = await pair.CreateTestMap(tile: "CMFloorPlating");
        var entities = server.EntMan;
        var network = server.ResolveDependency<IRobustSerializer>();
        EntityUid Spawn(string prototype) => entities.SpawnEntity(prototype, map.GridCoords);
        EntityUid user = default, uniform = default, restored = default;
        var location = new ItemStorageLocation(0, (6, 0));
        List<EntProtoId> vendors = ["TestMCLoadoutVendor"];

        await server.WaitAssertion(() =>
        {
            Spawn("TestMCLoadoutVendor");
            user = Spawn("MCMobHuman");
            uniform = Spawn("MCTagilaPants");
            var serializer = entities.System<MCLoadoutSerializerSystem>();
            var vendor = entities.System<MCVendorQuickEquipSystem>();
            var webbing = entities.System<SharedWebbingSystem>();
            var clothing = entities.GetComponent<WebbingClothingComponent>(uniform);
            // The starting vest comes with the pants and is not sold by this vendor.
            var withVest = vendor.VendItem(user, serializer.BuildItem(uniform)!, vendors)!.Value;
            Assert.That(webbing.HasWebbing(withVest, out var vest), Is.True);
            Assert.That(entities.GetComponent<MetaDataComponent>(vest).EntityPrototype!.ID, Is.EqualTo("MCWebbingBlackVest"));

            Assert.That(webbing.TryDetach((uniform, clothing), deferTransfer: false), Is.True);
            var bare = vendor.VendItem(user, serializer.BuildItem(uniform)!, vendors)!.Value;
            Assert.That(webbing.HasWebbing(bare, out _), Is.False);
            Assert.That(entities.HasComponent<StorageComponent>(bare), Is.False);

            Assert.That(webbing.Attach((uniform, clothing), Spawn("MCWebbingSurgeryFilled"), user, out _), Is.True);
            Assert.That(entities.System<InventorySystem>().TryEquip(user, uniform, "jumpsuit", force: true), Is.True);
        });

        await pair.RunTicksSync(3);
        await server.WaitAssertion(() =>
        {
            var fill = entities.GetComponent<StorageComponent>(uniform);
            var scalpel = fill.StoredItems.Keys.Single(uid => entities.GetComponent<MetaDataComponent>(uid).EntityPrototype!.ID == "MCScalpel");
            foreach (var uid in fill.StoredItems.Keys.Where(uid => uid != scalpel).ToArray())
                entities.DeleteEntity(uid);

            Assert.That(entities.System<SharedStorageSystem>().TrySetItemStorageLocation(scalpel, uniform, location), Is.True);
            var serializer = entities.System<MCLoadoutSerializerSystem>();
            using var yaml = new StringWriter();
            serializer.LoadoutToDataNode(serializer.BuildEntity(user)).Write(yaml);
            using var input = new MemoryStream(Encoding.UTF8.GetBytes(yaml.ToString()));
            using var packet = new MemoryStream();
            network.SerializeDirect(packet, serializer.LoadoutFromStream(input));
            packet.Position = 0;
            network.DeserializeDirect(packet, out MCLoadout received);
            var recipient = Spawn("MCMobHuman");
            entities.System<MCVendorQuickEquipSystem>().VendLoadout(recipient, received, vendors);
            Assert.That(entities.System<InventorySystem>().TryGetSlotEntity(recipient, "jumpsuit", out var worn), Is.True);
            restored = worn!.Value;
        });

        await pair.RunTicksSync(3);
        await server.WaitAssertion(() =>
        {
            Assert.That(entities.System<SharedWebbingSystem>().HasWebbing(restored, out var webbing), Is.True);
            Assert.That(entities.GetComponent<MetaDataComponent>(webbing).EntityPrototype!.ID, Is.EqualTo("MCWebbingSurgeryFilled"));
            Assert.That(entities.GetComponent<StorageComponent>(webbing).StoredItems, Is.Empty);
            var (tool, position) = entities.GetComponent<StorageComponent>(restored).StoredItems.Single();
            Assert.That(entities.GetComponent<MetaDataComponent>(tool).EntityPrototype!.ID, Is.EqualTo("MCScalpel"));
            Assert.That(position, Is.EqualTo(location));
            server.System<SharedMapSystem>().DeleteMap(map.MapId);
        });
        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task AttachFilledModuleToWornArmor()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings { Connected = true, NoLoadTestPrototypes = true });
        var server = pair.Server;
        var client = pair.Client;
        var entities = server.EntMan;
        var map = await pair.CreateTestMap(tile: "CMFloorPlating");
        var session = server.PlayerMan.GetSessionById(client.Session!.UserId);
        var previous = session.AttachedEntity;
        EntityUid user = default, armor = default, module = default, grenade = default;
        NetEntity netArmor = default, netGrenade = default;

        await server.WaitAssertion(() =>
        {
            user = entities.SpawnEntity("MCMobHuman", map.GridCoords);
            server.PlayerMan.SetAttachedEntity(session, user);
            armor = entities.SpawnEntity("MCArmorModularJaegerScout", map.GridCoords);
            module = entities.SpawnEntity("MCArmorModuleStorageGrenade", map.GridCoords);
            grenade = entities.SpawnEntity("MCGrenadeHefaM15", map.GridCoords);
            netArmor = entities.GetNetEntity(armor);
            netGrenade = entities.GetNetEntity(grenade);
            Assert.That(entities.System<InventorySystem>().TryEquip(user, armor, "outerClothing", force: true), Is.True);
            Assert.That(entities.System<SharedStorageSystem>().Insert(module, grenade, out _), Is.True);
            Assert.That(entities.System<SharedHandsSystem>().TryPickupAnyHand(user, module), Is.True);
        });
        await pair.RunTicksSync(5);
        await server.WaitAssertion(() =>
        {
            var use = new InteractUsingEvent(user, module, armor, map.GridCoords);
            entities.EventBus.RaiseLocalEvent(armor, use);
            Assert.That(use.Handled, Is.True);
        });
        await pair.RunSeconds(4);

        await server.WaitAssertion(() =>
        {
            Assert.That(entities.GetComponent<StorageComponent>(armor).StoredItems.ContainsKey(grenade), Is.True);
            Assert.That(entities.GetComponent<StorageComponent>(module).StoredItems, Is.Empty);
            entities.System<SharedStorageSystem>().OpenStorageUI(armor, user, doAfter: false);
        });
        await pair.RunTicksSync(5);
        await client.WaitAssertion(() =>
        {
            var clientArmor = client.EntMan.GetEntity(netArmor);
            var ui = client.EntMan.System<SharedUserInterfaceSystem>();
            Assert.That(ui.TryGetOpenUi<StorageBoundUserInterface>(clientArmor, StorageComponent.StorageUiKey.Key, out _), Is.True);
            var storage = client.EntMan.GetComponent<StorageComponent>(clientArmor);
            var item = client.EntMan.GetEntity(netGrenade);
            Assert.That(storage.StoredItems.Keys, Does.Contain(item));
            Assert.That(storage.Container.Contains(item), Is.True);
        });
        await server.WaitAssertion(() =>
        {
            entities.System<MCArmorModuleSharedSystem>().DetachSpecificModule(
                (armor, entities.GetComponent<MCArmorModularClothingComponent>(armor)),
                (module, entities.GetComponent<MCArmorModuleComponent>(module)), user);
            Assert.That(entities.GetComponent<StorageComponent>(module).StoredItems.Keys.Single(), Is.EqualTo(grenade));
            server.PlayerMan.SetAttachedEntity(session, previous);
            server.System<SharedMapSystem>().DeleteMap(map.MapId);
        });
        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task RoundTrip()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings { NoLoadTestPrototypes = true });
        await pair.LoadPrototypes([Prototypes]);
        var server = pair.Server;
        var map = await pair.CreateTestMap(tile: "CMFloorPlating");
        var network = server.ResolveDependency<IRobustSerializer>();
        await server.WaitAssertion(() =>
        {
            var entities = server.EntMan;
            EntityUid Spawn(string prototype) => entities.SpawnEntity(prototype, map.GridCoords);
            var serializer = entities.System<MCLoadoutSerializerSystem>();
            var vendor = entities.System<MCVendorQuickEquipSystem>();
            var inventory = entities.System<InventorySystem>();
            var storage = entities.System<SharedStorageSystem>();
            List<EntProtoId> vendors = ["TestMCLoadoutVendor"];
            Spawn("TestMCLoadoutVendor");
            var user = Spawn("MCMobHuman");
            var armor = Spawn("MCArmorModularJaegerScout");
            var module = Spawn("MCArmorModuleStorageGeneral");
            var gun = Spawn("MCWeaponP23");
            Entity<AttachableHolderComponent> holder = (gun, entities.GetComponent<AttachableHolderComponent>(gun));
            Assert.That(entities.System<AttachableHolderSystem>().Attach(holder, Spawn("MCAttachmentRedDotSight"), user, "rmc-aslot-rail"), Is.True);
            Assert.That(entities.System<ItemSlotsSystem>().TryEject(gun, "gun_magazine", null, out _), Is.True);
            Assert.That(storage.Insert(module, gun, out _), Is.True);
            Assert.That(entities.System<MCArmorModuleSharedSystem>().TryAttachModuleToAnySlot(
                (armor, entities.GetComponent<MCArmorModularClothingComponent>(armor)),
                (module, entities.GetComponent<MCArmorModuleComponent>(module))), Is.True);
            Assert.That(inventory.TryEquip(user, armor, "outerClothing", force: true), Is.True);

            var exported = serializer.BuildEntity(user);
            Assert.That(exported.Version, Is.EqualTo(3));
            using var yaml = new StringWriter();
            serializer.LoadoutToDataNode(exported).Write(yaml);
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(yaml.ToString()));
            var imported = serializer.LoadoutFromStream(stream);
            var recipient = Spawn("MCMobHuman");
            using var packet = new MemoryStream();
            network.SerializeDirect(packet, imported);
            packet.Position = 0;
            network.DeserializeDirect(packet, out MCLoadout received);
            vendor.VendLoadout(recipient, received, vendors);

            Assert.That(inventory.TryGetSlotEntity(recipient, "outerClothing", out var restored), Is.True);
            var item = serializer.BuildItem(restored!.Value)!;
            Assert.That(item.Parts.OfType<MCLoadoutArmorModules>().Single().Slots["storage"].ProtoId.Id, Is.EqualTo("MCArmorModuleStorageGeneral"));
            var restoredGun = item.Parts.OfType<MCLoadoutStorage>().Single().Items.Single().Item;
            Assert.That(restoredGun.ProtoId.Id, Is.EqualTo("MCWeaponP23"));
            var attachments = restoredGun.Parts.OfType<MCLoadoutAttachments>().Single().Slots;
            Assert.That(attachments["rmc-aslot-rail"].ProtoId.Id, Is.EqualTo("MCAttachmentRedDotSight"));
            Assert.That(attachments["rmc-aslot-underbarrel"].ProtoId.Id, Is.EqualTo("MCAttachmentLaserSight"));
            Assert.That(restoredGun.Parts.OfType<MCLoadoutItemSlots>().Single().Slots.ContainsKey("gun_magazine"), Is.False);

            var launcher = Spawn("MCAttachmentGrenadeLauncher");
            var ammo = entities.GetComponent<BallisticAmmoProviderComponent>(launcher);
            Assert.That(entities.System<SharedGunSystem>().TryAmmoInsert(launcher, ammo, Spawn("MCGrenadeHefaM15"), user, launcher, 0), Is.True);
            var restoredLauncher = vendor.VendItem(user, serializer.BuildItem(launcher)!, vendors)!.Value;
            Assert.That(serializer.BuildItem(restoredLauncher)!.Parts.OfType<MCLoadoutAmmo>().Single().Rounds.Single().ProtoId.Id, Is.EqualTo("MCGrenadeHefaM15"));

            var kit = Spawn("MCAidKitFirstFilled");
            var fill = entities.GetComponent<StorageComponent>(kit);
            var keep = fill.StoredItems.Keys.First();
            foreach (var child in fill.StoredItems.Keys.Where(child => child != keep).ToArray())
                entities.DeleteEntity(child);

            var kitData = serializer.BuildItem(kit)!;
            var restoredKit = vendor.VendItem(user, kitData, vendors)!.Value;
            var restoredChild = serializer.BuildItem(restoredKit)!.Parts.OfType<MCLoadoutStorage>().Single().Items.Single();
            Assert.That(restoredChild.Item.ProtoId, Is.EqualTo(kitData.Parts.OfType<MCLoadoutStorage>().Single().Items.Single().Item.ProtoId));
            Assert.That(restoredChild.Location, Is.EqualTo(fill.StoredItems[keep]));

            entities.DeleteEntity(keep);
            var empty = serializer.BuildItem(kit)!;
            var restoredEmpty = vendor.VendItem(user, empty, vendors)!.Value;
            Assert.That(entities.GetComponent<StorageComponent>(restoredEmpty).StoredItems, Is.Empty);

            using var plainYaml = new MemoryStream(Encoding.UTF8.GetBytes("""
                version: 3
                slots:
                - slotName: back
                  item: { protoId: MCAidKitFirstFilled }
                """));
            var plain = serializer.LoadoutFromStream(plainYaml);
            Assert.That(plain.Slots.Single().Item!.Parts, Is.Empty);
            server.System<SharedMapSystem>().DeleteMap(map.MapId);
        });
        await pair.CleanReturnAsync();
    }
}
