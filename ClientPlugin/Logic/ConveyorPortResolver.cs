using System;
using System.Collections.Generic;
using Sandbox.Definitions;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.Game.Models;
using VRageMath;

namespace ClientPlugin.Logic;

public static class ConveyorPortResolver
{
    private static readonly Dictionary<MyDefinitionId, IReadOnlyList<ConveyorPortInfo>> Cache =
        new Dictionary<MyDefinitionId, IReadOnlyList<ConveyorPortInfo>>();

    public static IReadOnlyList<ConveyorPortInfo> Resolve(MyCubeBlockDefinition def)
    {
        if (def == null)
            return Array.Empty<ConveyorPortInfo>();

        if (Cache.TryGetValue(def.Id, out var cached))
            return cached;

        var result = new List<ConveyorPortInfo>();
        try
        {
            if (string.IsNullOrEmpty(def.Model))
            {
                Cache[def.Id] = Array.Empty<ConveyorPortInfo>();
                return Cache[def.Id];
            }

            float gridSize = MyDefinitionManager.Static != null
                ? MyDefinitionManager.Static.GetCubeSize(def.CubeSize)
                : (def.CubeSize == MyCubeSize.Large ? 2.5f : 0.5f);

            var model = MyModels.GetModelOnlyDummies(def.Model);
            if (model == null)
            {
                Cache[def.Id] = Array.Empty<ConveyorPortInfo>();
                return Cache[def.Id];
            }

            var dummies = new Dictionary<string, IMyModelDummy>();
            ((IMyModel)model).GetDummies(dummies);

            // One visual per unique game conveyor line position (cell + direction).
            var seen = new HashSet<string>();

            foreach (var kv in dummies)
            {
                if (!ConveyorPortMath.IsConveyorDummyName(kv.Key))
                    continue;

                if (!ConveyorPortMath.TryGetPort(
                        kv.Value.Matrix.Translation,
                        gridSize,
                        def.Size,
                        def.Center,
                        def.ModelOffset,
                        out var normal,
                        out var localGrid))
                    continue;

                var key = $"{localGrid.X},{localGrid.Y},{localGrid.Z}:{normal.X},{normal.Y},{normal.Z}";
                if (!seen.Add(key))
                    continue;

                var drawOrigin = ConveyorPortMath.GetPortDrawOrigin(localGrid, normal, gridSize);
                result.Add(new ConveyorPortInfo(drawOrigin, normal, localGrid));
            }
        }
        catch
        {
            // visuals must never break placement
        }

        Cache[def.Id] = result;
        return result;
    }
}
