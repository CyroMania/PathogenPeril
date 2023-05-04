using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UnitUI : MonoBehaviour
{
    private const string statBarsContainerName = "StatBars";

    //This dictionary is needed so we can relate each health bar in the scene to a specific unit
    private static readonly Vector2 _energyBarTranslationOffset = new Vector2(0, -60);
    private static Dictionary<PlayerUnit, StatBars> _pathogensStatBars;

    [SerializeField]
    private GameObject _statBar;
    private Camera _mainCamera;
    private CameraMovement _cameraMove;
    private GameObject _UIStatBars;

    private struct StatBars
    {
        //Comments below show how the Health Bar will exist as part of the construct when its needed in the future.
        //public GameObject Health;
        public GameObject Energy;

        public StatBars(GameObject energyBar)
        {
            //Health = healthBar;
            Energy = energyBar;
        }
    }

    private void Start()
    {
        _mainCamera = Camera.main;
        _cameraMove = _mainCamera.GetComponent<CameraMovement>();
        _pathogensStatBars = new Dictionary<PlayerUnit, StatBars>();
        List<PlayerUnit> pathogens = FindObjectsOfType<PlayerUnit>().ToList();

        _UIStatBars = new GameObject(statBarsContainerName)
        {
            layer = LayerMask.NameToLayer("UI")
        };

        _UIStatBars.transform.parent = GameObject.Find("Canvas").transform;

        //The collection needs to be setup as a UI element with its own canvas.
        //This way it can updated separately to the main canvas.
        _UIStatBars.AddComponent<RectTransform>();
        _UIStatBars.AddComponent<Canvas>();
        _UIStatBars.GetComponent<Canvas>().overrideSorting = true;
        _UIStatBars.GetComponent<Canvas>().sortingLayerID = 0;

        //On start we must generate the new stat bar for each unit that is already on the game board.
        foreach (PlayerUnit pathogen in pathogens)
        {
            CreateNewStatBars(pathogen);
        }
    }

    private void Update()
    {
        foreach (KeyValuePair<PlayerUnit, StatBars> pathogenStatBars in _pathogensStatBars)
        {
            PlayerUnit pathogen = pathogenStatBars.Key;
            GameObject energyBar = pathogenStatBars.Value.Energy;
            Vector2 worldToScreenPoint = _mainCamera.WorldToScreenPoint(pathogen.gameObject.transform.position);

            //Check constantly if the unit or the camera is moving and update values accordingly.
            if (pathogen.IsMoving || _cameraMove.IsMoving)
            {
                energyBar.GetComponent<RectTransform>().position = worldToScreenPoint + _energyBarTranslationOffset;
                UpdateStatBarValue(pathogen, nameof(StatBars.Energy));
            }

            //Also need tp reposition them if we zoom in and out.
            if (_cameraMove.IsZooming)
            {
                energyBar.GetComponent<RectTransform>().position = worldToScreenPoint + _energyBarTranslationOffset;
            }
        }
    }

    /// <summary>
    /// Creates new stat bars for a player unit.
    /// </summary>
    /// <param name="pathogen">The player unit to create stat bars for.</param>
    public void CreateNewStatBars(PlayerUnit pathogen)
    {
        //Health Bar Generation will go here when health is requried for gameplay.

        //Energy Bar Generation.
        GameObject energyBar = Instantiate(_statBar);
        energyBar.name = "EnergyBar";
        energyBar.GetComponentInChildren<Image>().color = new Color(0, 0.8f, 0, 1);
        energyBar.transform.SetParent(_UIStatBars.transform);

        Vector2 worldToScreenPoint = _mainCamera.WorldToScreenPoint(pathogen.gameObject.transform.position);
        energyBar.GetComponent<RectTransform>().position = worldToScreenPoint + _energyBarTranslationOffset;

        StatBars pathogenStatBars = new StatBars(energyBar);
        _pathogensStatBars.Add(pathogen, pathogenStatBars);
    }

    /// <summary>
    /// Destroys stat bars associated with a player unit.
    /// </summary>
    /// <param name="pathogen">The player unit whose stat bars we need to destroy.</param>
    public void DestroyStatBars(PlayerUnit pathogen)
    {
        foreach (KeyValuePair<PlayerUnit, StatBars> statBars in _pathogensStatBars)
        {
            if (statBars.Key == pathogen)
            {
                _pathogensStatBars.Remove(statBars.Key);
                Destroy(statBars.Value.Energy);
                break;
            }
        }
    }

    /// <summary>
    /// Alters UI stat bar value.
    /// </summary>
    /// <param name="pathogen">The player unit whose stat bar we need to alter.</param>
    /// <param name="statBarName">The type of stat bar.</param>
    public static void UpdateStatBarValue(PlayerUnit pathogen, string statBarName)
    {
        StatBars unitStatBars = FindStatBars(pathogen);

        //This method is future proofed for Health bar value updates.
        if (statBarName == nameof(unitStatBars.Energy))
        {
            if (unitStatBars.Energy != null)
            {
                //the float conversion is needed otherwise it doesn't assign a value.
                unitStatBars.Energy.GetComponent<Slider>().value = (float)pathogen.MovementPoints / (float)pathogen.MaxMovementPoints;
            }
        }
    }

    /// <summary>
    /// Alters the stat bar position in screen space.
    /// </summary>
    /// <param name="pathogen">The player unit whose stat bar we need to alter.</param>
    /// <param name="screenPos">The screen position we need to set it to.</param>
    public static void UpdateStatBarPositions(PlayerUnit pathogen, Vector2 screenPos)
    {
        StatBars unitStatBars = FindStatBars(pathogen);
        unitStatBars.Energy.GetComponent<RectTransform>().position = screenPos + _energyBarTranslationOffset;
    }

    //Finds the stat bars associated with a specific player unit for manipulation.
    private static StatBars FindStatBars(PlayerUnit pathogen)
    {
        foreach (KeyValuePair<PlayerUnit, StatBars> pathogenStatBars in _pathogensStatBars)
        {
            if (pathogenStatBars.Key == pathogen)
            {
                return pathogenStatBars.Value;
            }
        }

        //Error message in case we can't find the stat bar we need.
        //It should always exist but we need to return a value.
        Debug.Log("No Stat Bar found.");
        return new StatBars();
    }
}