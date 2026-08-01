using Sandbox.Definitions;
using VRageMath;

namespace ClientPlugin.Placement;

public readonly struct PlacementGhostInfo
{
    public PlacementGhostInfo(MyCubeBlockDefinition definition, MatrixD worldMatrix, float gridSizeMeters)
    {
        Definition = definition;
        WorldMatrix = worldMatrix;
        GridSizeMeters = gridSizeMeters;
    }

    public MyCubeBlockDefinition Definition { get; }
    public MatrixD WorldMatrix { get; }
    public float GridSizeMeters { get; }
}
