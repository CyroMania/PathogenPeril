using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.UI.CanvasScaler;

public class Bacteria : PlayerUnit
{
    private static readonly short _maxHitPoints = 8;
    private static readonly short _maxMovementPoints = 5;
    private static readonly short _visibilityRange = 6;

    private const string DivideBtnName = "DivideBtn";
    private Button _divideBtn;

    private bool _clone = false;

    private void Start()
    {
        base.Init(_maxHitPoints, _maxMovementPoints, _visibilityRange);

        _divideBtn = GameObject.Find(DivideBtnName).GetComponent<Button>();
        _divideBtn.onClick.AddListener(Divide);

        if (_clone)
        {
            MovementPoints = 0;
            StopAllCoroutines();
        }
    }

    private void Divide()
    {
        if (Selected && MovementPoints == _maxMovementPoints)
        {
            List<Tile> acceptableTiles = new List<Tile>();

            foreach (Tile tile in CurrentTile.NeighbouringTiles)
            {
                if (!tile.Inhabited)
                {
                    acceptableTiles.Add(tile);
                }
            }

            Tile chosenTile = acceptableTiles.ToArray()[Random.Range(0, acceptableTiles.Count)];
            Bacteria clone = Instantiate(this, transform.position, Quaternion.identity);
            clone.name = "Bacteria";
            clone._clone = true;

            //Make Smoothly move to Tile
            StartCoroutine(DivideToNewTile(clone, chosenTile.transform.position + TileMovement.UnitLayer));
            clone.CurrentTile = chosenTile;
            MovementPoints = 0;
            ResetAllTiles(ignoredProps: new string[] { nameof(Tile.Visible) });
            CurrentTile.Current = true;
            UI.CheckButtonsUsable(MovementPoints, MaxMovementPoints);
            return;
        }
    }

    IEnumerator<Vector3> DivideToNewTile(Bacteria clone, Vector3 destination)
    {
        while (Vector3.Distance(clone.transform.position, destination) > 0.03f)
        {
            Vector3 newPos = Vector3.MoveTowards(clone.transform.position, destination, TileMovement.Speed * Time.deltaTime);
            clone.transform.position = newPos;
            yield return newPos;
        }

        clone.transform.position = destination;
    }
}
