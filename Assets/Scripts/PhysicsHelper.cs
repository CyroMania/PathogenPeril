using UnityEngine;

public static class PhysicsHelper
{
    /// <summary>
    /// Creates a Unity Raycast.
    /// </summary>
    /// <param name="targetLayer">The layer we want to hit colliders in.</param>
    /// <param name="raycastOrigin">The location where the raycast start.</param>
    /// <returns>A RaycastHit2D with the first collider hit on that layer.</returns>
    internal static RaycastHit2D GenerateRaycast(string targetLayer, Vector3 raycastOrigin)
    {
        int unitMask = 1 << LayerMask.NameToLayer(targetLayer);
        return Physics2D.Raycast(raycastOrigin, Vector2.zero, 0, unitMask);
    }
}