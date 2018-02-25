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

    //Stores a Dictionaru of all tile types and their default sprites
    public Dictionary<Tile.TileType, Sprite> tileSet;

    void Init()
    {
        tileSet = new Dictionary<Tile.TileType, Sprite>();
    }
}
