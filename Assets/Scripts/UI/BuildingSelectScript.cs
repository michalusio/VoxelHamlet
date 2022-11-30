using Assets.Scripts.Utilities;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class BuildingSelectScript : MonoBehaviour
    {
        public GameObject BuildingButtonPrefab;
        public GameObject ScrollContentPanel;

        public void PopulateWithBuildings(string searchPath)
        {
            foreach (Transform ch in ScrollContentPanel.transform)
            {
                Destroy(ch.gameObject);
            }
            var assets = Directory.GetFiles(searchPath, "*.building")
                .Select(a => 
                {
                    var container = ScriptableObject.CreateInstance<BuildingContainer>();
                    BuildingContainer.LoadByteData(container, File.ReadAllBytes(a));
                    return (a, container);
                })
                .ToList();
            int i = 0;
            var buttonList = new List<GameObject>();
            foreach ((var p, var b) in assets)
            {
                var button = Instantiate(BuildingButtonPrefab, ScrollContentPanel.transform);
                buttonList.Add(button);

                var rect = button.GetComponent<RectTransform>();
                rect.localPosition = new Vector3(rect.localPosition.x, -i * 256);

                button.GetComponent<RawImage>().texture = b.Icon;

                var building = b;
                var path = p;
                button.GetComponent<Button>().onClick.AddListener(() =>
                {
                    GlobalSettings.Instance.EditMode.LoadBuilding(building);
                });

                button.transform.GetChild(0).GetComponent<Button>().onClick.AddListener(() =>
                {
                    var index = buttonList.IndexOf(button);
                    buttonList.RemoveAt(index);
                    for (int moveIndex = index; moveIndex < buttonList.Count; moveIndex++)
                    {
                        var moveRect = buttonList[moveIndex].GetComponent<RectTransform>();
                        moveRect.localPosition = new Vector3(moveRect.localPosition.x, -moveIndex * 256);
                    }
                    File.Delete(path);
                    Destroy(button);
                    
                    var scrollRect = ScrollContentPanel.GetComponent<RectTransform>();
                    scrollRect.sizeDelta = new Vector2(scrollRect.sizeDelta.x, scrollRect.sizeDelta.y - 256);
                });

                i++;
            }
            {
                var scrollRect = ScrollContentPanel.GetComponent<RectTransform>();
                scrollRect.sizeDelta = new Vector2(scrollRect.sizeDelta.x, i * 256);
            }
        }
    }
}
