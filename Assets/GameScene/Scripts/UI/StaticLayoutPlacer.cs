using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class StaticLayoutPlacer : MonoBehaviour
{
    [Range(0, 24)]
    public int Gap = 8;

    [Range(0, 24)]
    public int Margin = 8;

    void OnEnable()
    {
        Layout();
    }

#if UNITY_EDITOR
    private int[] childrenHashCodes;

    void OnValidate()
    {
        Layout();
    }

    void Update()
    {
        if (Time.renderedFrameCount % 5 == 0)
        {
            Layout();
        }
    }

    public bool DrawGrid = false;
    void OnDrawGizmos()
    {
        if (!DrawGrid) return;
        var canvasMod = GetComponentInParent<Canvas>().transform.localScale;
        var pos = transform.position;
        var (itemSize, itemsPerRow) = GetItemSizing();
        itemSize.width *= canvasMod.x;
        itemSize.height *= canvasMod.x;
        var selfWidth = GetSelfWidth() * canvasMod.x;
        var selfHeight = GetSelfHeight() * canvasMod.x;
        var color = Gizmos.color;

        var margin = Margin * canvasMod.x;
        var gap = Gap * canvasMod.x;

        
        Gizmos.color = new Color(159/255f, 43 / 255f, 104 / 255f);

        for (int col = 0; col < itemsPerRow; col++)
        {
            Gizmos.DrawLine(new Vector3(pos.x + margin + (itemSize.width + gap) * col, pos.y, pos.z), new Vector3(pos.x + margin + (itemSize.width + gap) * col, pos.y - selfHeight, pos.z));
            Gizmos.DrawLine(new Vector3(pos.x + margin + (itemSize.width + gap) * col + itemSize.width, pos.y, pos.z), new Vector3(pos.x + margin + (itemSize.width + gap) * col + itemSize.width, pos.y - selfHeight, pos.z));
        }
        for (int row = 0; row < Mathf.FloorToInt(selfHeight / itemSize.height) - 1; row++)
        {
            Gizmos.DrawLine(new Vector3(pos.x, pos.y - (margin + (itemSize.height + gap) * row), pos.z), new Vector3(pos.x + selfWidth, pos.y - (margin + (itemSize.height + gap) * row), pos.z));
            Gizmos.DrawLine(new Vector3(pos.x, pos.y - (margin + (itemSize.height + gap) * row + itemSize.height), pos.z), new Vector3(pos.x + selfWidth, pos.y - (margin + (itemSize.height + gap) * row + itemSize.height), pos.z));
        }

        Gizmos.color = new Color(1, 165 / 255f, 0);
        Gizmos.DrawLine(new Vector3(pos.x + margin, pos.y, pos.z), new Vector3(pos.x + margin, pos.y - selfHeight, pos.z));
        Gizmos.DrawLine(new Vector3(pos.x, pos.y - margin, pos.z), new Vector3(pos.x + selfWidth, pos.y - margin, pos.z));
        Gizmos.DrawLine(new Vector3(pos.x + selfWidth - margin, pos.y, pos.z), new Vector3(pos.x + selfWidth - margin, pos.y - selfHeight, pos.z));
        Gizmos.DrawLine(new Vector3(pos.x, pos.y - selfHeight + margin, pos.z), new Vector3(pos.x + selfWidth, pos.y - selfHeight + margin, pos.z));


        Gizmos.color = color;
    }
#endif

    void Layout()
    {
        var (itemSize, itemsPerRow) = GetItemSizing();
        int index = 0;
        foreach (Transform c in transform)
        {
            if (c.TryGetComponent<RectTransform>(out var rect))
            {
                var col = index % itemsPerRow;
                var row = index / itemsPerRow;
                rect.localPosition = new Vector3(Margin + (itemSize.width + Gap) * col, - (Margin + (itemSize.height + Gap) * row), rect.localPosition.z);
                index++;
            }
        }
    }

    private (Rect, int) GetItemSizing()
    {
        var selfWidth = GetSelfWidth();
        var widthWithoutMargins = selfWidth - Margin * 2;
        var itemSize = transform.GetChild(0).GetComponent<RectTransform>().rect;
        int itemsPerRow = Mathf.FloorToInt((widthWithoutMargins + Gap) / (itemSize.width + Gap));
        return (itemSize, Math.Max(1, itemsPerRow));
    }

    private float GetSelfWidth()
    {
        var current = GetComponent<RectTransform>();
        var modifier = 1f;
        while (current.rect.width == 0)
        {
            var anchorDiff = current.anchorMax.x - current.anchorMin.x;
            if (anchorDiff == 0) anchorDiff = 1;
            modifier *= anchorDiff;
            if (!current.parent.TryGetComponent(out current)) throw new System.Exception("Something is no yes");
        }
        return modifier * current.rect.width;
    }

    private float GetSelfHeight()
    {
        var current = GetComponent<RectTransform>();
        var modifier = 1f;
        while (current.rect.height == 0)
        {
            var anchorDiff = current.anchorMax.y - current.anchorMin.y;
            if (anchorDiff == 0) anchorDiff = 1;
            modifier *= anchorDiff;
            if (!current.parent.TryGetComponent(out current)) throw new System.Exception("Something is no yes");
        }
        return modifier * current.rect.height;
    }
}
