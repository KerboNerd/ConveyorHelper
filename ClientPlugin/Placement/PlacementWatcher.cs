using Sandbox.Definitions;
using Sandbox.Game.Entities;
using VRage.Game;
using VRageMath;

namespace ClientPlugin.Placement;

public static class PlacementWatcher
{
    public static bool TryGetGhost(out PlacementGhostInfo ghost)
    {
        ghost = default;
        try
        {
            var builder = MyCubeBuilder.Static;
            if (builder == null || !builder.IsActivated)
                return false;

            var def = builder.CurrentBlockDefinition;
            if (def == null)
                return false;

            // Protected MyCubeBuilder.m_gizmo; public MyGizmoSpaceProperties.m_worldMatrixAdd (via publicizer).
            var space = builder.m_gizmo?.SpaceDefault;
            if (space == null)
                return false;

            MatrixD worldMatrix = space.m_worldMatrixAdd;
            float gridSize = def.CubeSize == MyCubeSize.Large ? 2.5f : 0.5f;
            ghost = new PlacementGhostInfo(def, worldMatrix, gridSize);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
