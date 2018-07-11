using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TilemapData {

    private static TilemapData instance;
    public static TilemapData Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new TilemapData();
                instance.Init();
            }            
            return instance;
        }
    }

    public int GridWidth { get; set; }
    public int GridHeight { get; set; }
    public List<Tile> TilesList { get; set; }

    //returns a list index based on grid coordinates
    public int GetPosFromCoords(int x, int y)
    {
        int ind = x + y * GridWidth;
        return ind;
    }

    //Stores a Dictionary of all tile types and their default sprites
    public Dictionary<Tile.TileType, Sprite[]> spriteAtlas;
    public Dictionary<Tile.TileType, Sprite[]> exitSpriteAtlas;
    public Sprite exitSprite;
    public Sprite gridSprite;

    void Init()
    {
        spriteAtlas = new Dictionary<Tile.TileType, Sprite[]>();
        exitSpriteAtlas = new Dictionary<Tile.TileType, Sprite[]>();
        exitSprite = Resources.Load<Sprite>("Sprites/ExitSign");
        gridSprite = Resources.Load<Sprite>("Sprites/GridCell");
    }
}
