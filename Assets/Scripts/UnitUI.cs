using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UnitUI : MonoBehaviour
{
    public Camera _mainCamera;
    public GameObject _statBar;

    private const string statBarsContainerName = "StatBars";
    private static readonly Vector2 _healthBarTranslationOffset = new Vector2(0, -30);
    private static readonly Vector2 _energyBarTranslationOffset = new Vector2(0, -45);
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
        _UIStatBars.transform.parent = GameObject.Find("Canvas").transform;
        _cameraMove = _mainCamera.GetComponent<CameraMovement>();
        _pathogensStatBars = new Dictionary<PlayerUnit, StatBars>();
        List<PlayerUnit> pathogens = FindObjectsOfType<PlayerUnit>().ToList();
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
            GameObject healthBar = pathogenStatBars.Value.Health;
            Vector2 worldToScreenPoint = _mainCamera.WorldToScreenPoint(pathogen.gameObject.transform.position);

            if (pathogen.IsMoving || _cameraMove.IsMoving)
            {
                healthBar.GetComponent<RectTransform>().position = worldToScreenPoint + _healthBarTranslationOffset;
                energyBar.GetComponent<RectTransform>().position = worldToScreenPoint + _energyBarTranslationOffset;
                UpdateStatBarValue(pathogen, nameof(StatBars.Energy));
            }

            if (_cameraMove.IsZooming)
            {
                healthBar.GetComponent<RectTransform>().position = worldToScreenPoint + _healthBarTranslationOffset;
                energyBar.GetComponent<RectTransform>().position = worldToScreenPoint + _energyBarTranslationOffset;
            }
        }
    }

    public void CreateNewStatBars(PlayerUnit pathogen)
    {
        //Health Bar Generation
        GameObject healthBar = Instantiate(_statBar);
        healthBar.name = "HealthBar";
        healthBar.GetComponentInChildren<Image>().color = Color.red;
        healthBar.transform.SetParent(_UIStatBars.transform);

        //Energy Bar Generation
        GameObject energyBar = Instantiate(_statBar);
        energyBar.name = "EnergyBar";
        energyBar.GetComponentInChildren<Image>().color = new Color(0, 0.8f, 0, 1);
        energyBar.transform.SetParent(_UIStatBars.transform);

        Vector2 worldToScreenPoint = _mainCamera.WorldToScreenPoint(pathogen.gameObject.transform.position);
        healthBar.GetComponent<RectTransform>().position = worldToScreenPoint + _healthBarTranslationOffset;
        energyBar.GetComponent<RectTransform>().position = worldToScreenPoint + _energyBarTranslationOffset;

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