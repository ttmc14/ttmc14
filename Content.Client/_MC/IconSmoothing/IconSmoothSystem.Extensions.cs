using Robust.Client.GameObjects;
using Robust.Shared.Map.Components;

// ReSharper disable once CheckNamespace
namespace Content.Client.IconSmoothing;

public sealed partial class IconSmoothSystem
{
    private const int Elements = 47;

    private static readonly byte[] RequiredMask =
    [
        0b000_00_000, //    0
        0b010_00_000, //    1
        0b000_00_010, //    2
        0b010_00_010, //    3
        0b000_01_000, //    4
        0b010_01_000, //    5
        0b000_01_010, //    6
        0b010_01_010, //    7
        0b000_10_000, //    8
        0b010_10_000, //    9
        0b000_10_010, //   10
        0b010_10_010, //   11
        0b000_11_000, //   12
        0b010_11_000, //   13
        0b000_11_010, //   14
        0b010_11_010, //   15

        // Extended
        0b011_01_000, //  21
        0b011_01_010, //  23
        0b011_11_000, //  29
        0b011_11_010, //  31
        0b000_01_011, //  38
        0b010_01_011, //  39
        0b000_11_011, //  46
        0b010_11_011, //  47
        0b011_01_011, //  55
        0b011_11_011, //  63
        0b000_10_110, //  74
        0b010_10_110, //  75
        0b000_11_110, //  78
        0b010_11_110, //  79
        0b011_11_110, //  95
        0b000_11_111, // 110
        0b010_11_111, // 111
        0b011_11_111, // 127
        0b110_10_000, // 137
        0b110_10_010, // 139
        0b110_11_000, // 141
        0b110_11_010, // 143
        0b111_11_000, // 157
        0b111_11_010, // 159
        0b110_11_011, // 175
        0b111_11_011, // 191
        0b110_10_110, // 203
        0b110_11_110, // 207
        0b111_11_110, // 223
        0b110_11_111, // 239
        0b111_11_111, // 255
    ];

    private static readonly byte[] ForbiddenMask =
    [
        0b111_11_111, //   0
        0b000_11_010, //   1
        0b010_11_000, //   2
        0b000_11_000, //   3
        0b010_10_010, //   4
        0b001_10_010, //   5
        0b010_10_001, //   6
        0b001_10_001, //   7
        0b010_01_010, //   8
        0b100_01_010, //   9
        0b010_01_100, //  10
        0b010_01_100, //  11
        0b010_00_010, //  12
        0b101_00_010, //  13
        0b010_00_101, //  14
        0b101_00_101, //  15

        // Extended
        0b000_10_010, //  21
        0b000_10_001, //  23
        0b100_00_010, //  29
        0b100_00_101, //  31
        0b010_10_000, //  38
        0b001_10_000, //  39
        0b010_00_100, //  46
        0b101_00_100, //  47
        0b000_10_000, //  55
        0b100_00_100, //  63
        0b010_01_000, //  74
        0b100_01_000, //  75
        0b010_00_001, //  78
        0b101_00_001, //  79
        0b100_00_001, //  95
        0b010_00_000, // 110
        0b101_00_000, // 111
        0b100_00_000, // 127
        0b000_01_010, // 137
        0b000_01_100, // 139
        0b001_00_010, // 141
        0b001_00_101, // 143
        0b000_00_010, // 157
        0b000_00_101, // 159
        0b001_00_100, // 175
        0b000_00_100, // 191
        0b000_01_000, // 203
        0b001_00_001, // 207
        0b000_00_001, // 223
        0b001_00_000, // 239
        0b000_00_000, // 255
    ];

    private static readonly int[] MappingMask =
    [
        0,
        1,
        2,
        3,
        4,
        5,
        6,
        7,
        8,
        9,
        10,
        11,
        12,
        13,
        14,
        15,
        21,
        23,
        29,
        31,
        38,
        39,
        46,
        47,
        55,
        63,
        74,
        75,
        78,
        79,
        95,
        110,
        111,
        127,
        137,
        139,
        141,
        143,
        157,
        159,
        175,
        191,
        203,
        207,
        223,
        239,
        255,
    ];

    private void CalculateNewSpriteCardinalExtended(Entity<MapGridComponent>? gridEntity, IconSmoothComponent smooth, Entity<SpriteComponent> sprite, TransformComponent xform, EntityQuery<IconSmoothComponent> smoothQuery)
    {
        var dirs = CardinalConnectDirs.None;
        if (gridEntity is null)
        {
            _sprite.LayerSetRsiState(sprite.AsNullable(), 0, $"{smooth.StateBase}{(int)dirs}");
            return;
        }

        var gridUid = gridEntity.Value.Owner;
        var grid = gridEntity.Value.Comp;

        var pos = _mapSystem.TileIndicesFor(gridUid, grid, xform.Coordinates);

        byte mask = 0;
        if (CheckDir(smooth, gridUid, grid, pos, Direction.NorthWest, smoothQuery))
            mask |= 1 << 7;
        if (CheckDir(smooth, gridUid, grid, pos, Direction.North, smoothQuery))
            mask |= 1 << 6;
        if (CheckDir(smooth, gridUid, grid, pos, Direction.NorthEast, smoothQuery))
            mask |= 1 << 5;
        if (CheckDir(smooth, gridUid, grid, pos, Direction.West, smoothQuery))
            mask |= 1 << 4;
        if (CheckDir(smooth, gridUid, grid, pos, Direction.East, smoothQuery))
            mask |= 1 << 3;
        if (CheckDir(smooth, gridUid, grid, pos, Direction.SouthWest, smoothQuery))
            mask |= 1 << 2;
        if (CheckDir(smooth, gridUid, grid, pos, Direction.South, smoothQuery))
            mask |= 1 << 1;
        if (CheckDir(smooth, gridUid, grid, pos, Direction.SouthEast, smoothQuery))
            mask |= 1 << 0;

        _sprite.LayerSetRsiState(sprite.AsNullable(), 0, $"{smooth.StateBase}{ConvertMaskToIndex(mask)}");
    }

    private bool CheckDir(
        IconSmoothComponent smooth,
        EntityUid gridUid,
        MapGridComponent grid,
        Vector2i pos,
        Direction offset,
        EntityQuery<IconSmoothComponent> smoothQuery)
    {
        var enumerator = _mapSystem.GetAnchoredEntitiesEnumerator(gridUid, grid, pos.Offset(offset));
        return MatchingEntity(smooth, enumerator, smoothQuery);
    }

    private static int ConvertMaskToIndex(byte mask)
    {
        for (var i = 0; i < Elements; i++)
        {
            if ((mask & RequiredMask[i]) != RequiredMask[i])
                continue;

            if ((mask & ForbiddenMask[i]) != 0)
                continue;

            return MappingMask[i];
        }

        return 0;
    }
}
