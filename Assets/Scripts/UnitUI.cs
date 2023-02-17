using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UnitUI : MonoBehaviour
{
    private const string healthBarContainerName = "HealthBars";
    private const string healthBarName = "HealthBar";

    private readonly Vector2 translationOffset = new Vector2(0, -50);

    [SerializeField]
    //This association is needed so we can relate each health bar in the scene to a specific unit
    private Dictionary<PlayerUnit, GameObject> _pathogenHealthBars;

    private void Start()
    {
        _pathogenHealthBars = new Dictionary<PlayerUnit, GameObject>();
        List<PlayerUnit> pathogens = FindObjectsOfType<PlayerUnit>().ToList();

        GameObject healthBars = new GameObject(healthBarContainerName);
        healthBars.transform.SetParent(FindObjectOfType<Canvas>().transform);
        healthBars.AddComponent<RectTransform>();

        foreach (PlayerUnit pathogen in pathogens)
        {
            GameObject healthBar = new GameObject(healthBarName);
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

            if (pathogen.IsMoving)
            {
                GameObject healthBar = pathogenHealthBar.Value;
                Vector2 worldToScreenPoint = Camera.main.WorldToScreenPoint(pathogen.gameObject.transform.position);
                healthBar.GetComponent<RectTransform>().position = worldToScreenPoint + translationOffset;
            }
        }
    }
}
