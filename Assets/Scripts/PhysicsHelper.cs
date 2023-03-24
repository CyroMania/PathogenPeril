using UnityEngine;

public static class PhysicsHelper
{
    internal static RaycastHit2D GenerateRaycast(string targetLayer, Vector3 raycastOrigin)
    {
        int unitMask = 1 << LayerMask.NameToLayer(targetLayer);
        return Physics2D.Raycast(raycastOrigin, Vector2.zero, 0, unitMask);
    }
}