using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UnitUI : MonoBehaviour
{
    public Camera mainCamera;

    private const string HealthBarContainerName = "HealthBars";
    private const string HealthBarName = "HealthBar";

    private CameraMovement _cameraMove;
    private readonly Vector2 _translationOffset = new Vector2(0, -50);

    [SerializeField]
    //This association is needed so we can relate each health bar in the scene to a specific unit
    private Dictionary<PlayerUnit, GameObject> _pathogenHealthBars;

    private void Start()
    {
        _cameraMove = mainCamera.GetComponent<CameraMovement>();
        _pathogenHealthBars = new Dictionary<PlayerUnit, GameObject>();
        List<PlayerUnit> pathogens = FindObjectsOfType<PlayerUnit>().ToList();

        GameObject healthBars = new GameObject(HealthBarContainerName);
        healthBars.transform.SetParent(FindObjectOfType<Canvas>().transform);
        healthBars.AddComponent<RectTransform>();

        foreach (PlayerUnit pathogen in pathogens)
        {
            GameObject healthBar = new GameObject(HealthBarName);
            healthBar.AddComponent<RectTransform>();
            healthBar.AddComponent<StatBar>();
            healthBar.AddComponent<Slider>();
            healthBar.transform.SetParent(healthBars.transform);

            _pathogenHealthBars.Add(pathogen, healthBar);
        }
    }

    private void Update()
    {
        foreach (KeyValuePair<PlayerUnit, GameObject> pathogenHealthBar in _pathogenHealthBars)
        {
            PlayerUnit pathogen = pathogenHealthBar.Key;

            if (pathogen.IsMoving || _cameraMove.IsMoving)
            {
                GameObject healthBar = pathogenHealthBar.Value;
                Vector2 worldToScreenPoint = mainCamera.WorldToScreenPoint(pathogen.gameObject.transform.position);
                healthBar.GetComponent<RectTransform>().position = worldToScreenPoint + _translationOffset;
            }
        }
    }
}
