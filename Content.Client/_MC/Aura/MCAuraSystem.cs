using Content.Shared._MC.Aura;
using Content.Shared._RMC14.Aura;
using Content.Shared._RMC14.Stealth;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Prototypes;

namespace Content.Client._MC.Aura;

public sealed class MCAuraSystem : Content.Shared._MC.Aura.MCAuraSystem
{
    private static readonly ProtoId<ShaderPrototype> ShaderId = "MCAuraOutline";

    [Dependency] private readonly IPrototypeManager _prototypes = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCAuraComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<MCAuraComponent, ComponentShutdown>(OnShutdown);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var auraQuery = EntityQueryEnumerator<MCAuraComponent, SpriteComponent>();
        while (auraQuery.MoveNext(out _, out var component, out var spriteComponent))
        {
            if (spriteComponent.PostShader is not {} shader)
                continue;

            var count = component.Entries.Count;

            shader.SetParameter("outline_count", (float) count);

            var i = 0;
            foreach (var (_, entry) in component.Entries)
            {
                shader.SetParameter($"outline_color{i}", entry.Color);
                shader.SetParameter($"outline_width{i}", entry.Width);

                i++;
            }
        }
    }

    private void OnStartup(Entity<MCAuraComponent> ent, ref ComponentStartup args)
    {
        if (!TryComp<SpriteComponent>(ent, out var sprite))
            return;

        sprite.PostShader = _prototypes.Index(ShaderId).InstanceUnique();
    }

    private void OnShutdown(Entity<MCAuraComponent> ent, ref ComponentShutdown args)
    {
        if (!TryComp<SpriteComponent>(ent, out var sprite) || TerminatingOrDeleted(ent))
            return;

        sprite.PostShader = null;
    }
}
