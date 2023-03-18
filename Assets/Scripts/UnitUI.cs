using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UnitUI : MonoBehaviour
{
    public Camera mainCamera;

    private const string statBarsContainerName = "StatBars";
    private const string HealthBarName = "HealthBar";
    private const string EnergyBarName = "EnergyBar";

    private static readonly Vector2 _healthBarTranslationOffset = new Vector2(0, -50);
    private static readonly Vector2 _energyBarTranslationOffset = new Vector2(0, -70);
    private CameraMovement _cameraMove;
    private GameObject _UIStatBars;

    struct StatBars
    {
        public GameObject Health;
        public GameObject Energy;

        public StatBars(GameObject healthBar, GameObject energyBar)
        {
            Health = healthBar;
            Energy = energyBar;
        }
    }

    [SerializeField]
    //This association is needed so we can relate each health bar in the scene to a specific unit
    private static Dictionary<PlayerUnit, StatBars> _pathogensStatBars;

    private void Start()
    {
        _UIStatBars = new GameObject(statBarsContainerName);
        _cameraMove = mainCamera.GetComponent<CameraMovement>();
        _pathogensStatBars = new Dictionary<PlayerUnit, StatBars>();
        List<PlayerUnit> pathogens = FindObjectsOfType<PlayerUnit>().ToList();

        _UIStatBars.transform.SetParent(FindObjectOfType<Canvas>().transform);
        _UIStatBars.AddComponent<RectTransform>();

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

            if (pathogen.IsMoving || _cameraMove.IsMoving)
            {
                GameObject healthBar = pathogenStatBars.Value.Health;

                Vector2 worldToScreenPoint = mainCamera.WorldToScreenPoint(pathogen.gameObject.transform.position);
                healthBar.GetComponent<RectTransform>().position = worldToScreenPoint + _healthBarTranslationOffset;
                energyBar.GetComponent<RectTransform>().position = worldToScreenPoint + _energyBarTranslationOffset;
                UpdateStatBarValue(pathogen, nameof(StatBars.Energy));
            }
        }
    }

    public void CreateNewStatBars(PlayerUnit pathogen)
    {
        //Health Bar Generation
        GameObject healthBar = new GameObject(HealthBarName);
        healthBar.AddComponent<RectTransform>();
        healthBar.AddComponent<StatBar>();
        healthBar.GetComponent<StatBar>().Color = Color.red;
        healthBar.AddComponent<Slider>();
        healthBar.transform.SetParent(_UIStatBars.transform);

        //Energy Bar Generation
        GameObject energyBar = new GameObject(EnergyBarName);
        energyBar.AddComponent<RectTransform>();
        energyBar.AddComponent<StatBar>();
        energyBar.GetComponent<StatBar>().Color = Color.green;
        energyBar.AddComponent<Slider>();
        energyBar.transform.SetParent(_UIStatBars.transform);

        StatBars pathogenStatBars = new StatBars(healthBar, energyBar);
        _pathogensStatBars.Add(pathogen, pathogenStatBars);
    }

    public void DestroyStatBars(PlayerUnit pathogen)
    {
        foreach (KeyValuePair<PlayerUnit, StatBars> statBars in _pathogensStatBars)
        {
            if (statBars.Key == pathogen)
            {
                _pathogensStatBars.Remove(statBars.Key);

                Destroy(statBars.Value.Health);
                Destroy(statBars.Value.Energy);

                break;
            }
        }
    }

    public static void UpdateStatBarValue(PlayerUnit unit, string statBarName)
    {
        StatBars unitStatBars = FindStatBars(unit);

        if (statBarName == nameof(unitStatBars.Health))
        {
            if (unitStatBars.Health != null)
            {
                unitStatBars.Health.GetComponent<Slider>().value = (float)unit.HitPoints / (float)unit.MaxHitPoints;
            }
        }
        else if (statBarName == nameof(unitStatBars.Energy))
        {
            if (unitStatBars.Energy != null)
            {
                unitStatBars.Energy.GetComponent<Slider>().value = (float)unit.MovementPoints / (float)unit.MaxMovementPoints;
            }
        }
    }

    private static StatBars FindStatBars(PlayerUnit unit)
    {
        foreach (KeyValuePair<PlayerUnit, StatBars> pathogenStatBars in _pathogensStatBars)
        {
            if (pathogenStatBars.Key == unit)
            {
                return pathogenStatBars.Value;
            }
        }

        return new StatBars();
    }

    public static void UpdateStatBarPositions(PlayerUnit pathogen, Vector2 screenPos)
    {
        StatBars unitStatBars = FindStatBars(pathogen);

        unitStatBars.Health.GetComponent<RectTransform>().position = screenPos + _healthBarTranslationOffset;
        unitStatBars.Energy.GetComponent<RectTransform>().position = screenPos + _energyBarTranslationOffset;
    }
}