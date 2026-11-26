using System.Linq;
using System.Numerics;
using Content.Shared._MC.Aura;
using Content.Shared._MC.Aura.Components;
using Content.Shared._RMC14.Stealth;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Shared.Prototypes;

namespace Content.Client._MC.Aura;

/// <summary>
/// Composites MC aura layers into one sprite shader. Each affected entity owns its shader parameters.
/// </summary>
public sealed class MCAuraVisualizerSystem : EntitySystem
{
    // Uniform capacity of the aura shader. Slots are assigned by priority for each sprite.
    private const int MaxOutlines = 16;
    private static readonly IComparer<string> LayerNameComparer = Comparer<string>.Create(string.CompareOrdinal);

    [Dependency] private readonly IPrototypeManager _prototypes = null!;
    [Dependency] private readonly IResourceCache _resources = null!;

    private readonly Dictionary<EntityUid, AuraVisual> _visuals = [];
    private Texture? _rayTexture;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MCAuraComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<MCAuraComponent, AfterAutoHandleStateEvent>(OnState);
        SubscribeLocalEvent<MCAuraComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<MCAuraComponent, BeforePostShaderRenderEvent>(OnBeforeRender);
    }

    private void OnStartup(Entity<MCAuraComponent> ent, ref ComponentStartup args)
    {
        UpdateShader(ent);
    }

    private void OnState(Entity<MCAuraComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        UpdateShader(ent);
    }

    private void UpdateShader(Entity<MCAuraComponent> ent)
    {
        if (!_visuals.TryGetValue(ent, out var visual))
        {
            visual = new AuraVisual(_prototypes.Index<ShaderPrototype>("MCAura").InstanceUnique());
            _visuals.Add(ent, visual);
        }

        var count = 0;
        var wave = false;
        var rays = false;
        var tint = 0f;
        var effects = ent.Comp.Effects;
        var ordered = effects.Select(pair => (Source: pair.Key, Effect: pair.Value, Preset: _prototypes.Index(pair.Value.Preset)))
            .OrderBy(entry => entry.Preset.Priority)
            .ThenBy(entry => entry.Source.Layer, LayerNameComparer)
            .ThenBy(entry => entry.Source.Emitter.Id);
        foreach (var entry in ordered)
        {
            wave |= entry.Preset.Wave;
            rays |= entry.Preset.Rays;
            tint = Math.Max(tint, entry.Effect.BloodTint);
            var width = entry.Preset.Width * entry.Effect.Strength;
            if (width <= 0 || count >= MaxOutlines)
                continue;

            visual.Colors[count] = entry.Preset.Color;
            visual.Widths[count++] = width;
        }

        var shader = visual.Shader;
        if (rays)
        {
            _rayTexture ??= _resources.GetResource<TextureResource>("/Textures/_MC/Effects/Xeno/carnage_rays.png").Texture;
        }

        visual.Rays = rays;
        shader.SetParameter("outlineCount", count);
        shader.SetParameter("outlineColors", visual.Colors);
        shader.SetParameter("outlineWidths", visual.Widths);
        shader.SetParameter("wave", wave);
        shader.SetParameter("rays", rays);
        shader.SetParameter("tint", tint);
    }

    private void OnBeforeRender(EntityUid uid, MCAuraComponent component, BeforePostShaderRenderEvent args)
    {
        if (!_visuals.TryGetValue(uid, out var visual) || !visual.Rays || _rayTexture is null ||
            args.Sprite.PostShader != visual.Shader || args.Viewport.Eye is not { } eye)
            return;

        var size = (Vector2) _rayTexture.Size * Vector2.Abs(args.Sprite.Scale) * eye.Scale * args.Viewport.RenderScale;
        // Clyde shares texture units between shaders. Another draw may have replaced the ray texture.
        visual.Shader.SetParameter("rayTexture", _rayTexture);
        visual.Shader.SetParameter("raySize", size);
    }

    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);
        foreach (var (uid, visual) in _visuals)
        {
            if (!TryComp<SpriteComponent>(uid, out var sprite))
                continue;

            if (HasComp<EntityActiveInvisibleComponent>(uid))
            {
                if (sprite.PostShader == visual.Shader)
                {
                    sprite.PostShader = null;
                    if (visual.EnabledShaderEvent)
                    {
                        sprite.RaiseShaderEvent = false;
                        visual.EnabledShaderEvent = false;
                    }
                }
                continue;
            }

            sprite.PostShader ??= visual.Shader;
            if (sprite.PostShader != visual.Shader)
                continue;

            if (visual.Rays && !sprite.RaiseShaderEvent)
            {
                sprite.RaiseShaderEvent = true;
                visual.EnabledShaderEvent = true;
            }

            if (!visual.Rays && visual.EnabledShaderEvent)
            {
                sprite.RaiseShaderEvent = false;
                visual.EnabledShaderEvent = false;
            }
        }
    }

    private void OnShutdown(Entity<MCAuraComponent> ent, ref ComponentShutdown args)
    {
        if (!_visuals.Remove(ent, out var visual))
            return;

        if (TryComp<SpriteComponent>(ent, out var sprite) && sprite.PostShader == visual.Shader)
        {
            sprite.PostShader = null;
            if (visual.EnabledShaderEvent)
            {
                sprite.RaiseShaderEvent = false;
            }
        }

        visual.Shader.Dispose();
    }

    private sealed class AuraVisual(ShaderInstance shader)
    {
        public readonly ShaderInstance Shader = shader;
        public readonly Color[] Colors = new Color[MaxOutlines];
        public readonly float[] Widths = new float[MaxOutlines];
        public bool Rays;
        public bool EnabledShaderEvent;
    }
}
