using System.Collections.Generic;
using UnityEngine;

public class Macrophage : ImmuneCell
{
    private const float MinPlayerUnitDistance = 0.03f;
    private const float MinPlayerUnitScale = 0.05f;

    //The macrophage's default stats when initialised.
    private static readonly short _maxHitPoints = 50;
    private static readonly short _maxMovementPoints = 5;
    private static readonly short _visibiliyRange = 10;

    private readonly Vector3 _scaleOffset = new Vector3(0.9f, 0.9f, 0.9f);

    private void Start()
    {
        base.Init(_maxHitPoints, _maxMovementPoints, _visibiliyRange);
    }

    /// <summary>
    /// The attack macrophages use if they are close enough to their target bacteria.
    /// </summary>
    /// <param name="targetPathogen">The player unit being consumed by this macrophage.</param>
    public void Phagocytosis(PlayerUnit targetPathogen)
    {
        Debug.Log("About to Run Phagocytosis");

        if (targetPathogen != null)
        {
            StartCoroutine(AnimatePhagocytosis(targetPathogen));
        }
    }

    /// <summary>
    /// Provides a smooth animation for Phagocytosis.
    /// </summary>
    /// <param name="pathogen">The unit being consumed by this macrophage.</param>
    /// <returns>An interpolated position or scale for the unit being consumed.</returns>
    private IEnumerator<Vector3> AnimatePhagocytosis(PlayerUnit pathogen)
    {
        pathogen.IsDead = true;

        //Null checks were added due to null ref exceptions.
        //We need to explore why this issue was happening more thoroughly as it's still confusing.
        //Pathogen should never be null.
        while (pathogen != null && Vector3.Distance(transform.position, pathogen.transform.position) > MinPlayerUnitDistance)
        {
            Vector3 newPos = Vector3.MoveTowards(pathogen.transform.position, transform.position, TileMovement.Speed / 2 * Time.deltaTime);
            pathogen.transform.position = newPos;
            yield return newPos;
        }

        //Assign the same position as this macrophage for the next part of the animation.
        if (pathogen != null)
        {
            pathogen.transform.position = transform.position;
        }

        while (pathogen != null && pathogen.transform.localScale.magnitude >= MinPlayerUnitScale)
        {
            //We convert the player unit to %90 of its previous scale each frame
            Vector3 newScale = Vector3.Scale(pathogen.transform.localScale, _scaleOffset);
            pathogen.transform.localScale = newScale;
            yield return newScale;
        }

        //When the animations has completed, the player unit is dead so we can eliminate them.
        pathogen.Kill();
    }
}