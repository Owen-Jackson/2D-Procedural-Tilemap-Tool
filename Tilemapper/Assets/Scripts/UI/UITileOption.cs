using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UITileOption : MonoBehaviour, IPointerClickHandler {

    public Tile.TileType type;
    public Sprite sprite;
    public Image image;
    public Tilemap tilemap;
    
    void Start()
    {
        image = GetComponent<Image>();
        sprite = image.sprite;

    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            tilemap.LMB.Type = type;
            tilemap.LMB.TileSprite = sprite;
            GetComponentInParent<TilemapUI>().SetLMBSprite(sprite);
        }
        else if(eventData.button == PointerEventData.InputButton.Right)
        {
            tilemap.RMB.Type = type;
            tilemap.RMB.TileSprite = sprite;
            GetComponentInParent<TilemapUI>().SetRMBSprite(sprite);
        }
    }
    
}
