using System.Linq;
using Content.Shared._MC.Xeno.Abilities.General.Skin;
using Content.Shared._MC.Xeno.Abilities.General.Skin.UI;
using JetBrains.Annotations;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Map;
using Robust.Shared.Serialization.TypeSerializers.Implementations;
using Robust.Shared.Utility;

namespace Content.Client._MC.Xeno.Abilities.Skin;

[UsedImplicitly]
public sealed class MCXenoSkinBui(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    private const float RotationStep = 90f;

    [Dependency] private readonly IEntityManager _entityManager = null!;
    [Dependency] private readonly IResourceCache _resourceCache = null!;

    [ViewVariables] private MCXenoSkinWindow? _window;

    private TransformSystem _transform = null!;
    private SpriteSystem _spriteSystem = null!;

    private List<(string Id, ResPath Path)> _skins = new();
    private int _currentIndex = -1;

    private Angle _rotation = Angle.Zero;
    private EntityUid? _previewEntity;
    private RSI? _defaultRsi;

    protected override void Open()
    {
        base.Open();

        _transform = _entityManager.System<TransformSystem>();
        _spriteSystem = _entityManager.System<SpriteSystem>();

        _window = this.CreateWindow<MCXenoSkinWindow>();

        _window.PreviousButton.OnPressed += _ => EntityRotate(-1);
        _window.NextButton.OnPressed += _ => EntityRotate(1);
        _window.ResetPreviewButton.OnPressed += _ => EntityResetRotation();
        _window.ResetButton.OnPressed += _ => ResetSelection();
        _window.ApplyButton.OnPressed += _ => ApplySkin();

        EntitySpawn();
        Load();
    }

    private void ApplySkin()
    {
        SendMessage(_currentIndex == -1
            ? new MCXenoSkinResetBuiMessage()
            : new MCXenoSkinSelectBuiMessage(_skins[_currentIndex].Id));
    }

    private void ResetSelection()
    {
        _currentIndex = -1;

        if (_previewEntity is { } entity)
            _spriteSystem.SetBaseRsi(entity, _defaultRsi);
    }

    private void Load()
    {
        if (_window is null)
            return;

        if (!_entityManager.TryGetComponent<MCXenoSkinComponent>(Owner, out var component))
            return;

        _skins = component.Skins
            .Select(x => (x.Key, x.Value))
            .ToList();

        RebuildList();
    }

    private void RebuildList()
    {
        if (_window is null)
            return;

        var list = _window.SkinList;
        list.DisposeAllChildren();

        for (var i = 0; i < _skins.Count; i++)
        {
            var index = i;
            var skin = _skins[i];

            var btn = new Button
            {
                Text = skin.Id,
                HorizontalExpand = true,
            };

            btn.OnPressed += _ => ShowSkin(index);

            list.AddChild(btn);
        }
    }

    private void ShowSkin(int index)
    {
        if (_window is null || _skins.Count == 0)
            return;

        _currentIndex = int.Clamp(index, 0, _skins.Count - 1);
        EntitySet(_skins[_currentIndex].Path);
    }

    private void EntityRotate(int dir)
    {
        if (_previewEntity is not { } entity)
            return;

        _rotation += Angle.FromDegrees(RotationStep * dir);
        _transform.SetLocalRotation(entity, _rotation);
    }

    private void EntityResetRotation()
    {
        if (_previewEntity is not { } entity)
            return;

        _transform.SetLocalRotation(entity, Angle.Zero);
    }

    private void EntitySet(ResPath resPath)
    {
        if (_previewEntity is not { } entity)
            return;

        if (!_resourceCache.TryGetResource(SpriteSpecifierSerializer.TextureRoot / resPath, out RSIResource? resource))
            return;

        _transform.SetLocalRotation(_previewEntity.Value, _rotation);
        _spriteSystem.SetBaseRsi(entity, resource.RSI);
    }

    private void EntitySpawn()
    {
        if (_window is null)
            return;

        if (!_entityManager.TryGetComponent<SpriteComponent>(Owner, out var component))
            return;

        _previewEntity = _entityManager.Spawn(_entityManager.GetComponent<MetaDataComponent>(Owner).EntityPrototype?.ID, MapCoordinates.Nullspace);
        _entityManager.EnsureComponent<SpriteComponent>(_previewEntity.Value);

        _spriteSystem.SetBaseRsi(_previewEntity.Value, component.BaseRSI);

        _window.SkinPreview.SetEntity(_previewEntity);

        _defaultRsi = component.BaseRSI;
    }
}
