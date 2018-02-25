using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class Tilemap : MonoBehaviour {
    /*
    //Used to differentiate which tile types to place
    public enum TileMode
    {
        ROOM = 0,
        CORRIDOR = 1,
        DELETE = 2
    };
    */

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

    //Previous width, used when resizing
    public int PreviousGridWidth { get; set; }

    //previous height, used when resizing
    public int PreviousGridHeight { get; set; }

    //Width of the tile grid
    [SerializeField]
    private int gridWidth;
    public int GridWidth
    {
        get
        {
            return gridWidth;
        }
        set
        {
            PreviousGridWidth = gridWidth;
            gridWidth = value;
            TilemapData.Instance.GridWidth = value;
            if(BSPGenerator != null)
            {
                BSPGenerator.GridWidth = value;
            }
        }
    }

    //Height of the tile grid
    [SerializeField]
    private int gridHeight;
    public int GridHeight
    {
        get
        {
            return gridHeight;
        }
        set
        {
            PreviousGridHeight = gridHeight;
            gridHeight = value;
            TilemapData.Instance.GridHeight = value;
            if (BSPGenerator != null)
            {
                BSPGenerator.GridHeight = value;
            }
        }
    }

    [SerializeField]
    int tileSize;
    [SerializeField]
    private List<Tile> tiles;
    public List<Tile> Tiles
    {
        get { return tiles; }
        set { tiles = value; }
    }

    public List<int> tileEnums;

    public MouseClick LMB { get; set; }
    public MouseClick RMB { get; set; }

    [SerializeField]
    BSPDungeon BSPGenerator;

    //stores all of the floors in the current dungeon
    public Dungeon ThisDungeon { get; set; }
    public DungeonFloor CurrentFloor { get; set; }

    // Use this for initialization
    void Start()
    {
        LMB = new MouseClick();
        RMB = new MouseClick();
        LMB.Type = Tile.TileType.ROOM;
        RMB.Type = Tile.TileType.NONE;
        gridSprite = Resources.Load<Sprite>("Sprites/GridCell");
        roomSprites = Resources.LoadAll<Sprite>("Sprites/DragonheartRooms");
        corridorSprites = Resources.LoadAll<Sprite>("Sprites/DragonHeartCorridors");
        exitSprites = Resources.LoadAll<Sprite>("Sprites/DragonHeartExits");
        //create a map for the exit sprites
        exitSpriteIndexMap = new Dictionary<int, int>()
        {
            {-8, 0 },
            {-12, 1 },
            {-14, 2 },
            {-15, 3 },
            {1, 4 },
            {2, 5 },
            {3, 6 },
            {4, 7 },
            {5, 8 },
            {8, 9 },
            {10, 10 },
            {12, 11 },
            {17, 12 },
            {18, 13 },
            {20, 14 },
            {24, 15 }
        };
        InitialiseEmptyTileMap();
        //RandomiseTileMap();
        if (GetComponent<BSPDungeon>())
        {
            BSPGenerator = GetComponent<BSPDungeon>();
        }
        BSPGenerator.SetGridSize(GridWidth, GridHeight);
    }

    // Update is called once per frame
    void Update()
    {
        //check that the mouse isn't over a UI element
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            //Right click
            if (Input.GetMouseButton(1))
            {
                switch (RMB.Type)
                {
                    case Tile.TileType.NONE:
                        DeleteTile();
                        break;
                    case Tile.TileType.ROOM:
                        PlaceRoom();
                        break;
                    case Tile.TileType.CORRIDOR:
                        PlaceTile(RMB);
                        break;
                }
            }
            //Left click
            if (Input.GetMouseButton(0))
            {
                switch (LMB.Type)
                {
                    case Tile.TileType.NONE:
                        DeleteTile();
                        break;
                    case Tile.TileType.ROOM:
                        PlaceRoom();
                        break;
                    case Tile.TileType.CORRIDOR:
                        PlaceTile(LMB);
                        break;
                }
            }
        }
    }

    public void BSPGenerate()
    {
        //Loop 100 times for testing purposes
        //for (int i = 0; i < 100; i++)
        //{
        ThisDungeon = new Dungeon();
        CurrentFloor = new DungeonFloor();
        ClearGrid();
        BSPGenerator.BuildTree();
        AddRooms();
        AddCorridors();
        AddExits();
        FullBitmaskPass();

        tileEnums = new List<int>();
        for(int i = 0; i < GridWidth * GridHeight; i++)
        {
            tileEnums.Add((int)Tiles[i].GetTileType());
        }

        CurrentFloor.Tiles = tileEnums;
        ThisDungeon.Floors.Add(CurrentFloor);
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

    public void ResizeGrid()
    {
        InitialiseEmptyTileMap();
        UpdateTileMapDataTiles();

        tileEnums = new List<int>();
        for (int i = 0; i < GridWidth * GridHeight; i++)
        {
            tileEnums.Add((int)Tiles[i].GetTileType());
        }

        CurrentFloor.Tiles = tileEnums;
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
        if (Tiles != null)
        {
            for (int i = 0; i < Tiles.Count; i++)
            {
                if (Tiles[i].gameObject != null)
                {
                    Destroy(Tiles[i].gameObject);
                }
            }
            Tiles.Clear();
        }
        Tiles = new List<Tile>();
        for (int i = 0; i < GridHeight; i++)
        {
            for (int j = 0; j < GridWidth; j++)
            {
                Tile toAdd = Instantiate(Resources.Load<GameObject>("Prefabs/BaseTile").GetComponent<Tile>(), transform);
                toAdd.SetTile(gridSprite, 0);
                toAdd.transform.position = transform.position + new Vector3(j, i, 0);
                toAdd.SetGridPosition(GetPosFromCoords(j, i));
                Tiles.Add(toAdd);
            }
        }
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
                Tile component = hit.collider.GetComponent<Tile>();
                Tile.TileType oldType = component.GetTileType();
                component.SetTile(gridSprite, 0);
                UpdateAdjacentTiles(component.GetGridPosition(), oldType);
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

    void PlaceTile(MouseClick mouseButton)
    {
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
        if (hit.collider != null)
        {
            if (hit.collider.tag == "Tile")
            {
                Tile component = hit.collider.GetComponent<Tile>();
                if (component.GetTileType() != mouseButton.Type)
                {
                    int gridPos = component.GetGridPosition();
                    //Debug.Log("adding room tile at " + gridPos);
                    component.SetTile(corridorSprites[GetBitmaskValue(gridPos, mouseButton.Type)], (int)mouseButton.Type);
                    UpdateAdjacentTiles(gridPos, mouseButton.Type);
                    component.SetTile(corridorSprites[GetBitmaskValue(gridPos, mouseButton.Type)], (int)mouseButton.Type);
                    Debug.Log("placing: " + (int)mouseButton.Type);

                }
                //hit.collider.GetComponent<Tile>().SetTile(corridorSprites[0], 2);
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

    public void LoadFromFile()
    {
        for(int i = 0; i < GridWidth * GridHeight; i++)
        {
            Tiles[i].SetTileType(CurrentFloor.Tiles[i]);
        }
        FullBitmaskPass();
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
                if (type == Tile.TileType.ROOM)
                {
                    Tiles[north].SetTile(roomSprites[GetBitmaskValue(north, type)], (int) type);
                }
                else if(type == Tile.TileType.CORRIDOR)
                {
                    Tiles[north].SetTile(corridorSprites[GetBitmaskValue(north, type)], (int)type);
                }
            }
        }

        if (south >= 0)
        {
            if (Tiles[south].GetTileType() == type)
            {
                if (type == Tile.TileType.ROOM)
                {
                    Tiles[south].SetTile(roomSprites[GetBitmaskValue(south, type)], (int)type);
                }
                else if (type == Tile.TileType.CORRIDOR)
                {
                    Tiles[south].SetTile(corridorSprites[GetBitmaskValue(south, type)], (int)type);
                }
            }
        }

        if (east % GridWidth != 0)
        {
            if (Tiles[east].GetTileType() == type)
            {
                if (type == Tile.TileType.ROOM)
                {
                    Tiles[east].SetTile(roomSprites[GetBitmaskValue(east, type)], (int)type);
                }
                else if (type == Tile.TileType.CORRIDOR)
                {
                    Tiles[east].SetTile(corridorSprites[GetBitmaskValue(east, type)], (int)type);
                }
            }
        }

        if (gridPos % GridWidth != 0)
        {
            if (Tiles[west].GetTileType() == type)
            {
                if (type == Tile.TileType.ROOM)
                {
                    Tiles[west].SetTile(roomSprites[GetBitmaskValue(west, type)], (int)type);
                }
                else if (type == Tile.TileType.CORRIDOR)
                {
                    Tiles[west].SetTile(corridorSprites[GetBitmaskValue(west, type)], (int)type);
                }
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

    //Make it just for room tiles initially
    int GetBitmaskValue(int gridIndex, Tile.TileType type)
    {
        int maskValue = 0;
        int north = 0, east = 0, south = 0, west = 0;
        int edgeHack = 0;   //this one is used for the exits with a wall on one side
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
                        if (Tiles[gridIndex + GridWidth].GetTileType() == Tile.TileType.NONE)
                        {
                            edgeHack = -16;
                        }
                    }
                }
                else
                {
                    edgeHack = -16;
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
                        if(Tiles[gridIndex - 1].GetTileType() == Tile.TileType.NONE)
                        {
                            edgeHack = 16;
                        }
                    }
                }
                else
                {
                    edgeHack = 16;
                }
                //get the east value
                if ((gridIndex + 1) % GridWidth != 0)
                {
                    if (Tiles[gridIndex + 1] != null)
                    {
                        if (Tiles[gridIndex + 1].GetTileType() == Tile.TileType.CORRIDOR)
                        {
                            east = 1;
                        }
                        if(Tiles[gridIndex + 1].GetTileType() == Tile.TileType.NONE)
                        {
                            edgeHack = -16;
                        }
                    }
                }
                else
                {
                    edgeHack = -16;
                }
                //get the south value
                if (gridIndex - GridWidth >= 0)
                {
                    if (Tiles[gridIndex - GridWidth] != null)
                    {
                        if (Tiles[gridIndex - GridWidth].GetTileType() == Tile.TileType.CORRIDOR)
                        {
                            south = 1;
                        }
                        if (Tiles[gridIndex - GridWidth].GetTileType() == Tile.TileType.NONE)
                        {
                            edgeHack = 16;
                        }
                    }
                }
                else
                {
                    edgeHack = 16;
                }
                break;
        }
        maskValue += north + west * 2 + east * 4 + south * 8 + edgeHack;
        return maskValue;
    }

    public Sprite GetGridSprite()
    {
        return gridSprite;
    }

    public List<int> GetFloor(int floorNum)
    {
        if(floorNum < ThisDungeon.Floors.Count && floorNum >= 0)
        {
            return ThisDungeon.Floors[floorNum].Tiles;
        }
        return null;
    }
}
