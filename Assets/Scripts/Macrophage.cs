using System.Collections.Generic;
using UnityEngine;

public class Macrophage : ImmuneCell
{
    private static readonly short _maxHitPoints = 50;
    private static readonly short _maxMovementPoints = 5;
    private static readonly short _visibiliyRange = 10;

    private readonly Vector3 _scaleOffset = new Vector3(0.9f, 0.9f, 0.9f);

    private void Start()
    {
        base.Init(_maxHitPoints, _maxMovementPoints, _visibiliyRange);
    }

    public void Phagocytosis(PlayerUnit targetPathogen)
    {
        Debug.Log("About to Run Phagocytosis");
        StartCoroutine(AnimatePhagocytosis(targetPathogen));
    }

    IEnumerator<Vector3> AnimatePhagocytosis(PlayerUnit pathogen)
    {
        while (Vector3.Distance(transform.position, pathogen.transform.position) > 0.03)
        {
            Vector3 newPos = Vector3.MoveTowards(pathogen.transform.position, transform.position, TileMovement.Speed / 2 * Time.deltaTime);
            pathogen.transform.position = newPos;
            yield return newPos;
        }

        pathogen.transform.position = transform.position;

        while (pathogen.transform.localScale.magnitude >= 0.05f)
        {
            Vector3 newScale = Vector3.Scale(pathogen.transform.localScale, _scaleOffset);
            pathogen.transform.localScale = newScale;
            yield return newScale;
        }

        GameObject.Find("Canvas").GetComponent<UnitUI>().DestroyStatBars(pathogen);
    }
} 