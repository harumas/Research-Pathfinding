using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Visualizer.MapEditor;

public class DisplayScaler : MonoBehaviour
{
    public void Scale(Vector2 displaySize, Vector2 size)
    {
        Vector3 scale = new Vector3(displaySize.x / size.x, 1, displaySize.y / size.y);
        transform.localScale = scale;
        transform.localPosition = new Vector3(-displaySize.x / 2f, -displaySize.y / 2f, 0);
    }
}