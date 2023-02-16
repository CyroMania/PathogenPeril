using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UnitUI : MonoBehaviour
{
    private const string healthBarContainerName = "HealthBars";
    private const string healthBarName = "HealthBar";

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
            healthBar.AddComponent<HealthBar>();
            healthBar.transform.SetParent(healthBars.transform);

            _pathogenHealthBars.Add(pathogen, healthBar);
        }
    }

    private void Update()
    {

    }
}
