using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Main player unit. A simple pathogen that replicates by cloning itself.
/// </summary>
public class Bacteria : PlayerUnit
{
    private static readonly short _maxHitPoints = 8;
    private static readonly short _maxMovementPoints = 5;
    private static readonly short _visibilityRange = 6;

    private const string DivideBtnName = "DivideBtn";
    private const string EnergyProperty = "Energy";

    private Button _divideBtn;

    private void Start()
    {
        base.Init(_maxHitPoints, _maxMovementPoints, _visibilityRange);

        //Because we need this behaviour is triggered by the UI button, we need to add this trigger dynamically as a listener for each unit that spawns.
        _divideBtn = GameObject.Find(DivideBtnName).GetComponent<Button>();
        _divideBtn.onClick.AddListener(Divide);

        if (Clone)
        {
            JustDivided = true;
            MovementPoints = 0;
        }
    }

    /// <summary>
    /// The main behaviour that bacteria use to create copies of themselves.
    /// </summary>
    private void Divide()
    {
        //The divide command requires all energy and we must only activate it on the selected unit because otherwise all player units would divide.
        if (Selected && MovementPoints == _maxMovementPoints)
        {
            List<Tile> acceptableTiles = new List<Tile>();

            foreach (Tile tile in CurrentTile.NeighbouringTiles)
            {
                //Inline with one of the requirements, no units can inhabit the same tile.
                if (!tile.Inhabited)
                {
                    acceptableTiles.Add(tile);
                }
            }

            //Randomly choose a tile to make it feel more natural.
            Tile chosenTile = acceptableTiles[Random.Range(0, acceptableTiles.Count)];
            Bacteria clone = Instantiate(this, transform.position, Quaternion.identity);
            //Spawns new stat bars for the cloned unit.
            GameObject.Find("Canvas").GetComponent<UnitUI>().CreateNewStatBars(clone);
            JustDivided = true;
            clone.name = "Bacteria";
            clone.Clone = true;

            //Begins the coroutine to animate the movement.
            StartCoroutine(DivideToNewTile(clone, chosenTile.transform.position));
 
            MovementPoints = 0;
            ResetAllTiles(ignoredProps: new string[] { nameof(Tile.Visible), nameof(Tile.Goal) });
            CurrentTile.Current = true;
            UI.CheckButtonsUsable(MovementPoints, MaxMovementPoints, IsFullySurrounded());
            UnitUI.UpdateStatBarValue(this, EnergyProperty);
            return;
        }
    }

    /// <summary>
    /// The animation for moving a newly spawned unit to the current unit's neighbour tile.
    /// </summary>
    /// <param name="clone">The original unit to copy.</param>
    /// <param name="destination">The new tile to clone the unit to.</param>
    /// <returns>An interpolated position vector.</returns>
    private IEnumerator<Vector3> DivideToNewTile(Bacteria clone, Vector3 destination)
    {
        while (Vector3.Distance(clone.transform.position, destination) > 0.03f)
        {
            Vector3 newPos = Vector3.MoveTowards(clone.transform.position, destination, TileMovement.Speed * Time.deltaTime);
            clone.transform.position = newPos;
            yield return newPos;
        }

        clone.transform.position = destination;
        clone.CurrentTile = TileMovement.CalculateCurrentTile(clone);
        UnitUI.UpdateStatBarValue(clone, EnergyProperty);
        UnitUI.UpdateStatBarPositions(clone, Camera.main.WorldToScreenPoint(destination));
        TileMovement.FindVisibleTiles(CurrentTile, new Queue<Tile>(), Visibility);
    }
}