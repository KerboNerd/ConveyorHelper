using VRageMath;

namespace ClientPlugin.Logic;

public readonly struct ConveyorPortInfo
{
    public ConveyorPortInfo(Vector3 localPosition, Vector3 localNormal, Vector3I localGridPosition)
    {
        LocalPosition = localPosition;
        LocalNormal = localNormal;
        LocalGridPosition = localGridPosition;
    }

    public Vector3 LocalPosition { get; }
    public Vector3 LocalNormal { get; }
    public Vector3I LocalGridPosition { get; }
}
