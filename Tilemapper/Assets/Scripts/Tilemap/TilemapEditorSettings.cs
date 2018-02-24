using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TilemapEditorSettings : MonoBehaviour {

    public Tilemap tilemap;
    public Slider widthSlider;
    public Slider heightSlider;

    private void Awake()
    {
        tilemap.GridWidth = (int)widthSlider.value;
        tilemap.GridHeight = (int)heightSlider.value;
    }

    public void SetGridWidth(float width)
    {
        tilemap.GridWidth = (int)width;
        tilemap.ResizeGrid();
    }

    public void SetGridHeight(float height)
    {
        tilemap.GridHeight = (int)height;
        tilemap.ResizeGrid();
    }

    public void ResizeGrid()
    {
        tilemap.ResizeGrid();
    }
}
