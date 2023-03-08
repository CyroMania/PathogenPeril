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

    private CameraMovement _cameraMove;
    private GameObject _UIStatBars;
    private readonly Vector2 _healthBarTranslationOffset = new Vector2(0, -50);
    private readonly Vector2 _energyBarTranslationOffset = new Vector2(0, -70);

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
    private Dictionary<PlayerUnit, StatBars> _pathogensStatBars;

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

            if (pathogen.IsMoving || _cameraMove.IsMoving)
            {
                GameObject healthBar = pathogenStatBars.Value.Health;
                GameObject energyBar = pathogenStatBars.Value.Energy;

                Vector2 worldToScreenPoint = mainCamera.WorldToScreenPoint(pathogen.gameObject.transform.position);
                healthBar.GetComponent<RectTransform>().position = worldToScreenPoint + _healthBarTranslationOffset;
                energyBar.GetComponent<RectTransform>().position = worldToScreenPoint + _energyBarTranslationOffset;
            }
        }
    }

    public void CreateNewStatBars(PlayerUnit newPathogen)
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
        _pathogensStatBars.Add(newPathogen, pathogenStatBars);
    }
}