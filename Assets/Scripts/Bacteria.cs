using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
            Bacteria clone = Instantiate(this, chosenTile.transform.position, Quaternion.identity);
            clone.name = "Bacteria";
            clone._clone = true;
            clone.CurrentTile = TileMovement.CalculateCurrentTile(clone);
            MovementPoints = 0;
            ResetAllTiles(ignoredProps: new string[] { nameof(Tile.Visible) });
            CurrentTile.Current = true;
            UI.CheckButtonsUsable(MovementPoints, MaxMovementPoints);
            return;
        }
    }
}
