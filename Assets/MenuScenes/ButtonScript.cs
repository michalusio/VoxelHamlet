using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.UI.Button;

[ExecuteAlways]
public class ButtonScript : MonoBehaviour
{
    public string Text;
    public ButtonClickedEvent OnClick = new ButtonClickedEvent();
    void Start()
    {
        UpdateData();
    }

    void OnValidate()
    {
        UpdateData();
    }

    void UpdateData()
    {
        var text = GetComponentInChildren<Text>();
        text.text = Text;
        var button = GetComponentInChildren<Button>();
        button.onClick = OnClick;
    }
}
