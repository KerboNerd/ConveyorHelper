using System;
using VRageMath;

namespace ClientPlugin.Logic;

public static class ConveyorPortMath
{
    public static bool IsConveyorDummyName(string dummyName)
    {
        if (string.IsNullOrEmpty(dummyName))
            return false;

        // Same rule as MyMultilineConveyorEndpoint.GetLinePositions: Contains("detector_conveyor")
        return dummyName.IndexOf("detector_conveyor", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    /// <summary>
    /// Mirrors Sandbox.Game MyMultilineConveyorEndpoint.GetLinePositions dummy → direction logic.
    /// </summary>
    public static bool TryGetPort(
        Vector3 localDummyPosition,
        float gridSize,
        Vector3I blockSize,
        Vector3I blockCenter,
        Vector3 modelOffset,
        out Vector3 localNormal,
        out Vector3I localGridPosition)
    {
        localNormal = Vector3.Zero;
        localGridPosition = Vector3I.Zero;

        if (gridSize <= 0f || blockSize.X <= 0 || blockSize.Y <= 0 || blockSize.Z <= 0)
            return false;

        // Model space is center-origin; game shifts by half block extents into corner-origin cube space.
        var halfExtents = new Vector3(blockSize) * 0.5f * gridSize;
        var pos = localDummyPosition + modelOffset + halfExtents;

        var cell = Vector3I.Floor(pos / gridSize);
        cell = Vector3I.Max(Vector3I.Zero, cell);
        cell = Vector3I.Min(blockSize - Vector3I.One, cell);

        var cellCenter = (new Vector3(cell) + Vector3.Half) * gridSize;
        var delta = pos - cellCenter;
        var projected = Vector3.DominantAxisProjection(delta);
        if (projected.LengthSquared() < 1e-8f)
            return false;

        localNormal = Vector3.Normalize(projected);
        localGridPosition = cell - blockCenter;
        return true;
    }

    /// <summary>
    /// Face-centered draw origin in model space for a resolved port.
    /// </summary>
    public static Vector3 GetPortDrawOrigin(Vector3I localGridPosition, Vector3 localNormal, float gridSize)
    {
        // Cell center in center-origin model space, then push to the port face.
        var cellCenter = new Vector3(localGridPosition) * gridSize;
        return cellCenter + localNormal * (gridSize * 0.5f);
    }
}
