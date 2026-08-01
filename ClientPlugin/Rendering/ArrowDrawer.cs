using System.Collections.Generic;
using ClientPlugin.Logic;
using ClientPlugin.Placement;
using VRageMath;
using VRageRender;

namespace ClientPlugin.Rendering;

public static class ArrowDrawer
{
    private static readonly Color Cyan = new Color(0, 255, 255, 255);

    public static void Draw(in PlacementGhostInfo ghost, IReadOnlyList<ConveyorPortInfo> ports)
    {
        if (ports == null || ports.Count == 0)
            return;

        // Long thick shafts: ~0.9 of a cube, radius scales with grid.
        float length = MathHelper.Clamp(ghost.GridSizeMeters * 0.9f, 0.45f, 2.5f);
        float radius = MathHelper.Clamp(ghost.GridSizeMeters * 0.06f, 0.03f, 0.18f);
        var world = ghost.WorldMatrix;

        foreach (var port in ports)
        {
            var startLocal = port.LocalPosition;
            var endLocal = port.LocalPosition + port.LocalNormal * length;
            var start = Vector3D.Transform(startLocal, world);
            var end = Vector3D.Transform(endLocal, world);

            MyRenderProxy.DebugDrawCapsule(start, end, radius, Cyan, depthRead: false, shaded: true);
            MyRenderProxy.DebugDrawSphere(end, radius * 1.35f, Cyan, 1f, depthRead: false, smooth: true);
        }
    }
}
