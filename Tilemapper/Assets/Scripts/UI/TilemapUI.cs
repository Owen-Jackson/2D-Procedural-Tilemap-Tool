using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TilemapUI : MonoBehaviour {

    public Tilemap tilemap;
    public Slider widthSlider;
    public Slider heightSlider;
    public Image LMBSprite;
    public Image RMBSprite;

    private void Awake()
    {
        tilemap.GridWidth = (int)widthSlider.value;
        tilemap.GridHeight = (int)heightSlider.value;
        UITileOption[] tileOptions = GetComponentsInChildren<UITileOption>();
        for(int i = 0; i < tileOptions.Length; i++)
        {
            tileOptions[i].tilemap = tilemap;
        }
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

    public void SetLMBSprite(Sprite sprite)
    {
        LMBSprite.sprite = sprite;
    }

    public void SetRMBSprite(Sprite sprite)
    {
        RMBSprite.sprite = sprite;
    }
}
