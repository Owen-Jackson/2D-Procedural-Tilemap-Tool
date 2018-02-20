using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Tilemap : MonoBehaviour {
    enum CorridorTileIDs
    {
        CORNERNORTHEAST = 0,
        CORNERNORTHWEST,
        CORNERSOUTHEAST,
        CORNERSOUTHWEST,
        CORRIDORHORIZONTAL,
        CORRIDORSURROUNDED,     //Walls on all four sides
        CORRIDORVERTICAL,
        CROSSROAD,
        DEADENDEAST,
        DEADENDNORTH,
        DEADENDSOUTH,
        DEADENDWEST,
        TJUNCTIONEAST,
        TJUNCTIONNORTH,
        TJUNCTIONSOUTH,
        TJUNCTIONWEST
    };

    //Used to differentiate which tile types to place
    enum TileMode
    {
        ROOM = 0,
        CORRIDOR = 1,
        DELETE = 2
    };

    [SerializeField]
    private Sprite[] roomSprites;
    [SerializeField]
    private Sprite[] corridorSprites;
    [SerializeField]
    private Sprite[] exitSprites;
    //this dictionary maps the bitmask value to the sprite index
    private Dictionary<int, int> exitSpriteIndexMap;
    [SerializeField]
    private Sprite gridSprite;  //The default sprite for the grid (revert to when deleting a tile from the tilemap)

    //Can store a grid of tiles
    [SerializeField]
    public int GridWidth { get; set; }
    [SerializeField]
    public int GridHeight { get; set; }
    [SerializeField]
    int tileSize;
    [SerializeField]
    public List<Tile> Tiles { get; set; }

    [SerializeField]
    TileMode LMB;
    [SerializeField]
    TileMode RMB;

    [SerializeField]
    BSPDungeon BSPGenerator;


    // Use this for initialization
    void Start()
    {
        //WARNING HARD CODED, MAKE CHANGABLE IN UI LATER
        GridWidth = 30;
        GridHeight = 30;
        TilemapData.Instance.GridWidth = GridWidth;
        TilemapData.Instance.GridHeight = GridHeight;

        gridSprite = Resources.Load<Sprite>("Sprites/GridCell");
        roomSprites = Resources.LoadAll<Sprite>("Sprites/DragonheartRooms");
        corridorSprites = Resources.LoadAll<Sprite>("Sprites/DragonHeartCorridors");
        exitSprites = Resources.LoadAll<Sprite>("Sprites/DragonHeartExits");
        //create a map for the exit sprites
        exitSpriteIndexMap = new Dictionary<int, int>()
        {
            {1, 0 },
            {2, 1 },
            {3, 2 },
            {4, 3 },
            {5, 4 },
            {8, 5 },
            {10, 6 },
            {12, 7 }
        };
        InitialiseEmptyTileMap();
        //RandomiseTileMap();
        LMB = TileMode.ROOM;
        RMB = TileMode.DELETE;
        if (GetComponent<BSPDungeon>())
        {
            BSPGenerator = GetComponent<BSPDungeon>();
        }
        BSPGenerator.SetGridSize(GridWidth, GridHeight);
    }

    // Update is called once per frame
    void Update()
    {
        //Right click
        if (Input.GetMouseButton(1))
        {
            switch (RMB)
            {
                case TileMode.DELETE:
                    DeleteTile();
                    break;
                case TileMode.ROOM:
                    PlaceRoom();
                    break;
                case TileMode.CORRIDOR:
                    PlaceCorridor();
                    break;
            }
        }
        //Left click
        if (Input.GetMouseButton(0))
        {
            switch (LMB)
            {
                case TileMode.DELETE:
                    DeleteTile();
                    break;
                case TileMode.ROOM:
                    PlaceRoom();
                    break;
                case TileMode.CORRIDOR:
                    PlaceCorridor();
                    break;
            }
        }
    }

    public void BSPGenerate()
    {
        //Loop 100 times for testing purposes
        //for (int i = 0; i < 100; i++)
        //{
        ClearGrid();
        BSPGenerator.BuildTree();
        AddRooms();
        AddCorridors();
        AddExits();
        FullBitmaskPass();
        //}
    }

    public void AddRooms()
    {
        List<Room> roomNodes = BSPGenerator.GetNodesWithRooms();
        if(roomNodes.Count > 0)
        {
            //Debug.Log(roomNodes.Count);
            foreach (Room room in roomNodes)
            {
                PlaceRect(room.XPos, room.YPos, room.Width, room.Height, (int)Tile.TileType.ROOM);
            }
        }
        UpdateTileMapDataTiles();
    }

    public void AddCorridors()
    {
        BSPGenerator.CreateCorridors();
        List<Tile> corridors = BSPGenerator.GetCorridors();
        if(corridors.Count > 0)
        {
            foreach(Tile corridorTile in corridors)
            {
                Tiles[corridorTile.GetGridPosition()].SetTile(corridorSprites[0], 2);
                //PlaceRect((int)corridor.x, (int)corridor.y, (int)corridor.width, (int)corridor.height, (int)Tile.TileType.CORRIDOR);
            }
        }
        UpdateTileMapDataTiles();
    }

    public void AddExits()
    {
        List<int> exits = BSPGenerator.GetExits();
        if(exits.Count > 0)
        {
            foreach(int exit in exits)
            {
                Tiles[exit].SetTile(exitSprites[0], 3);
            }
        }
    }

    public void RandomiseTileMap()
    {
        if (Tiles == null)
        {
            InitialiseEmptyTileMap();
        }
        int count = 0;
        for (int i = 0; i < GridHeight * GridWidth; i++)
        {
            Tiles[i].DeleteMe();
            if (Random.Range(0f, 1f) > 0.5f)
            {
                count++;
                Debug.Log("random succeeded");
                Tiles[i].SetTile(roomSprites[0], 1);
            }
        }
        Debug.Log("num of successes: " + count);
        FullBitmaskPass();
    }

    public void ClearGrid()
    {
        if (Tiles == null)
        {
            InitialiseEmptyTileMap();
        }
        for (int i = 0; i < GridHeight * GridWidth; i++)
        {
            Tiles[i].DeleteMe();
        }
    }

    public void InitialiseEmptyTileMap()
    {
        Tiles = new List<Tile>();
        for (int i = 0; i < GridHeight; i++)
        {
            for (int j = 0; j < GridWidth; j++)
            {
                Tile toAdd = Instantiate(Resources.Load<GameObject>("Prefabs/BaseTile").GetComponent<Tile>(), transform);
                toAdd.SetTile(gridSprite, 0);
                toAdd.transform.position = transform.position + new Vector3(j, i, 0);
                toAdd.SetGridPosition(j + i * GridHeight);
                Tiles.Add(toAdd);
            }
        }
        UpdateTileMapDataTiles();
    }

    public void UpdateWidth(int width)
    {
        GridWidth = width;
        TilemapData.Instance.GridWidth = width;
    }

    public void UpdateHeight(int height)
    {
        GridHeight = height;
        TilemapData.Instance.GridHeight = height;
    }

    private void UpdateTileMapDataTiles()
    {
        TilemapData.Instance.TilesList = Tiles;
    }

    void DeleteTile()
    {
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
        if (hit.collider != null)
        {
            if (hit.collider.tag == "Tile")
            {
                hit.collider.GetComponent<Tile>().SetTile(gridSprite, 0);
                UpdateAdjacentTiles(hit.collider.GetComponent<Tile>().GetGridPosition(), Tile.TileType.ROOM);
            }
        }
    }

    void PlaceRoom()
    {
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
        if (hit.collider != null)
        {
            if (hit.collider.tag == "Tile")
            {
                Tile component = hit.collider.GetComponent<Tile>();
                if (component.GetTileType() != Tile.TileType.ROOM)
                {
                    int gridPos = component.GetGridPosition();
                    //Debug.Log("adding room tile at " + gridPos);
                    component.SetTile(roomSprites[GetBitmaskValue(gridPos, Tile.TileType.ROOM)], 1);
                    UpdateAdjacentTiles(gridPos, Tile.TileType.ROOM);
                    component.SetTile(roomSprites[GetBitmaskValue(gridPos, Tile.TileType.ROOM)], 1);
                }
            }
        }
    }

    public void PlaceRect(int xPos, int yPos, int width, int height, int tileType)
    {
        for(int i = 0; i < height; i++)
        {
            for(int j = 0; j < width;j++)
            {
                try
                {
                    Tiles[GetPosFromCoords(xPos + j, yPos + i)].SetTile(roomSprites[0], tileType);
                }
                catch (System.ArgumentOutOfRangeException e)
                {
                    Debug.Log("this index is out of bounds: " + GetPosFromCoords(xPos + j, yPos + i));
                    Debug.Log("Input values: xPos = " + xPos + " , yPos = " + yPos + " , width = " + width + " , height = " + height);
                    throw new System.ArgumentOutOfRangeException("Index out of bounds: ", e);
                }
            }
        }
    }

    //Does a pass of the whole grid to update sprites
    public void FullBitmaskPass()
    {
        foreach(Tile tile in Tiles)
        {
            if (tile.GetTileType() == Tile.TileType.ROOM)
            {
                tile.SetTile(roomSprites[GetBitmaskValue(tile.GetGridPosition(), Tile.TileType.ROOM)], 1);
            }
            if(tile.GetTileType() == Tile.TileType.CORRIDOR)
            {
                tile.SetTile(corridorSprites[GetBitmaskValue(tile.GetGridPosition(), Tile.TileType.CORRIDOR)], 2);
            }
            if (tile.GetTileType() == Tile.TileType.EXIT)
            {
                tile.SetTile(exitSprites[exitSpriteIndexMap[GetBitmaskValue(tile.GetGridPosition(), Tile.TileType.EXIT)]], 3);
            }
        }
        UpdateTileMapDataTiles();
    }

    //tells adjacent tiles to update themselves
    void UpdateAdjacentTiles(int gridPos, Tile.TileType type)
    {
        int north = GetNorth(gridPos);
        int south = GetSouth(gridPos);
        int east = GetEast(gridPos);
        int west = GetWest(gridPos);

        if (north < Tiles.Count)
        {
            if (Tiles[north].GetTileType() == type)
            {
                Tiles[north].SetTile(roomSprites[GetBitmaskValue(north, type)], 1);
            }
        }

        if (south >= 0)
        {
            if (Tiles[south].GetTileType() == type)
            {
                Tiles[south].SetTile(roomSprites[GetBitmaskValue(south, type)], 1);
            }
        }

        if (east % GridWidth != 0)
        {
            if (Tiles[east].GetTileType() == type)
            {
                Tiles[east].SetTile(roomSprites[GetBitmaskValue(east, type)], 1);
            }
        }

        if (gridPos % GridWidth != 0)
        {
            if (Tiles[west].GetTileType() == type)
            {
                Tiles[west].SetTile(roomSprites[GetBitmaskValue(west, type)], 1);
            }
        }
    }

    //Helper functions for grid positions
    int GetNorth(int origin)
    {
        return origin + GridWidth;
    }

    int GetSouth(int origin)
    {
        return origin - GridWidth;
    }

    int GetEast(int origin)
    {
        return origin + 1;
    }

    int GetWest(int origin)
    {
        return origin - 1;
    }

    //returns a list index based on grid coordinates
    int GetPosFromCoords(int x, int y)
    {
        int ind = x + y * GridWidth;
        return ind;
    }

    void PlaceCorridor()
    {
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
        if (hit.collider != null)
        {
            if (hit.collider.tag == "Tile")
            {
                Debug.Log("adding corridor tile");
                hit.collider.GetComponent<Tile>().SetTile(corridorSprites[0], 2);
            }
        }
    }

    //Make it just for room tiles initially
    int GetBitmaskValue(int gridIndex, Tile.TileType type)
    {
        int maskValue = 0;
        int north = 0, east = 0, south = 0, west = 0;
        int flipMultiplier = 1;
        switch (type)
        {
            case Tile.TileType.ROOM:
                //get the north value
                if (gridIndex + GridWidth < Tiles.Count)
                {
                    if (Tiles[gridIndex + GridWidth] != null)
                    {
                        if (Tiles[gridIndex + GridWidth].GetTileType() == Tile.TileType.ROOM || Tiles[gridIndex + GridWidth].GetTileType() == Tile.TileType.EXIT)
                        {
                            north = 1;
                        }
                    }
                }
                //get the west value
                if (gridIndex % GridWidth != 0)
                {
                    if (Tiles[gridIndex - 1] != null)
                    {
                        if (Tiles[gridIndex - 1].GetTileType() == Tile.TileType.ROOM || Tiles[gridIndex - 1].GetTileType() == Tile.TileType.EXIT)
                        {
                            west = 1;
                        }
                    }
                }
                //get the east value
                if ((gridIndex + 1) % GridWidth != 0)
                {
                    if (Tiles[gridIndex + 1] != null)
                    {
                        if (Tiles[gridIndex + 1].GetTileType() == Tile.TileType.ROOM || Tiles[gridIndex + 1].GetTileType() == Tile.TileType.EXIT)
                        {
                            east = 1;
                        }
                    }
                }
                //get the south value
                if (gridIndex - GridWidth >= 0)
                {
                    if (Tiles[gridIndex - GridWidth] != null)
                    {
                        if (Tiles[gridIndex - GridWidth].GetTileType() == Tile.TileType.ROOM || Tiles[gridIndex - GridWidth].GetTileType() == Tile.TileType.EXIT)
                        {
                            south = 1;
                        }
                    }
                }
                break;
            case Tile.TileType.CORRIDOR:
                //get the north value
                if (gridIndex + GridWidth < Tiles.Count)
                {
                    if (Tiles[gridIndex + GridWidth] != null)
                    {
                        if (Tiles[gridIndex + GridWidth].GetTileType() == Tile.TileType.CORRIDOR || Tiles[gridIndex + GridWidth].GetTileType() == Tile.TileType.EXIT)
                        {
                            north = 1;
                        }
                    }
                }
                //get the west value
                if (gridIndex % GridWidth != 0)
                {
                    if (Tiles[gridIndex - 1] != null)
                    {
                        if (Tiles[gridIndex - 1].GetTileType() == Tile.TileType.CORRIDOR || Tiles[gridIndex - 1].GetTileType() == Tile.TileType.EXIT)
                        {
                            west = 1;
                        }
                    }
                }
                //get the east value
                if ((gridIndex + 1) % GridWidth != 0)
                {
                    if (Tiles[gridIndex + 1] != null)
                    {
                        if (Tiles[gridIndex + 1].GetTileType() == Tile.TileType.CORRIDOR || Tiles[gridIndex + 1].GetTileType() == Tile.TileType.EXIT)
                        {
                            east = 1;
                        }
                    }
                }
                //get the south value
                if (gridIndex - GridWidth >= 0)
                {
                    if (Tiles[gridIndex - GridWidth] != null)
                    {
                        if (Tiles[gridIndex - GridWidth].GetTileType() == Tile.TileType.CORRIDOR || Tiles[gridIndex - GridWidth].GetTileType() == Tile.TileType.EXIT)
                        {
                            south = 1;
                        }
                    }
                }
                break;
            case Tile.TileType.EXIT:
                //get the north value
                if (gridIndex + GridWidth < Tiles.Count)
                {
                    if (Tiles[gridIndex + GridWidth] != null)
                    {
                        if (Tiles[gridIndex + GridWidth].GetTileType() == Tile.TileType.CORRIDOR)
                        {
                            north = 1;
                        }
                    }
                }
                //get the west value
                if (gridIndex % GridWidth != 0)
                {
                    if (Tiles[gridIndex - 1] != null)
                    {
                        if (Tiles[gridIndex - 1].GetTileType() == Tile.TileType.CORRIDOR)
                        {
                            west = 1;
                        }
                    }
                }
                //get the east value
                if ((gridIndex + 1) % GridWidth != 0)
                {
                    if (Tiles[gridIndex + 1] != null)
                    {
                        if (Tiles[gridIndex + 1].GetTileType() == Tile.TileType.CORRIDOR || Tiles[gridIndex + 1].GetTileType() == Tile.TileType.NONE)
                        {
                            east = 1;
                        }
                        //else if(Tiles[gridIndex + 1].GetTileType() == Tile.TileType.NONE)
                        //{
                          //  flipMultiplier = -1;
                        //}
                    }
                }
                //get the south value
                if (gridIndex - GridWidth >= 0)
                {
                    if (Tiles[gridIndex - GridWidth] != null)
                    {
                        if (Tiles[gridIndex - GridWidth].GetTileType() == Tile.TileType.CORRIDOR || Tiles[gridIndex - GridWidth].GetTileType() == Tile.TileType.NONE)
                        {
                            south = 1;
                        }
                        //else if(Tiles[gridIndex - GridWidth].GetTileType() == Tile.TileType.NONE)
                        //{
                          //  flipMultiplier = -1;
                        //}
                    }
                }
                break;
        }
        maskValue = north + west * 2 + east * 4 + south * 8;
        return maskValue;
    }

    public Sprite GetGridSprite()
    {
        return gridSprite;
    }
}
