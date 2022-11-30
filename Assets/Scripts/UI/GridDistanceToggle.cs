using Assets.Scripts.Utilities;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class GridDistanceToggle : MonoBehaviour
{
    private Toggle toggle;

    void Start()
    {
        toggle = GetComponent<Toggle>();
        toggle.isOn = true;
    }

    public void ToggleGrid(bool grid)
    {
        GlobalSettings.Instance.MapMaterial.SetFloat("_Grid", grid ? 1 : 0);
        GlobalSettings.Instance.EditMapMaterial.SetFloat("_Grid", grid ? 1 : 0);
    }
}
