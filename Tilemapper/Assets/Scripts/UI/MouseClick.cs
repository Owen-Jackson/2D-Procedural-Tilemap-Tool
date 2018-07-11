using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ClickActions
{
    NONE = 0,
    PLACETILE,
    REMOVETILE,
    CREATEEXIT,
    REMOVEEXIT
}

//This class stores the sprite and tile type for placing a tile
[System.Serializable]
public class MouseClick {
    public ClickActions CurrentAction;
    public Tile.TileType Type;
    public Sprite TileSprite;
}
