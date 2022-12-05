using System;
using UnityEngine;
using UnityEngine.UI;

public class SpeedPanelScript : MonoBehaviour
{
    public Text fpsText;

    void Update()
    {
        var gc = GC.GetTotalMemory(false);
        var bytes = gc % 1024;
        var kilobytes = (gc / 1024) % 1024;
        var megabytes = (gc / (1024 * 1024)) % 1024;
        fpsText.text = $"{(int)(1 / Time.unscaledDeltaTime)} Fps\nGC: {megabytes}MB {kilobytes}KB {bytes}B";
    }

    public void Pause()
    {
        Time.timeScale = 0;
    }

    public void Speed1X()
    {
        Time.timeScale = 1;
    }

    public void Speed2X()
    {
        Time.timeScale = 2;
    }

    public void Speed3X()
    {
        Time.timeScale = 3;
    }
}
