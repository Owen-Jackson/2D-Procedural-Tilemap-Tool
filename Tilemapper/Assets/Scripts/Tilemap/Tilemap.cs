using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class Tilemap : MonoBehaviour {
    [SerializeField]
    private Sprite gridSprite;  //The default sprite for the grid (revert to when deleting a tile from the tilemap)
    private Sprite exitSprite;  //used to debug where the exits are

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
            gridWidth = value;
            TilemapData.Instance.GridWidth = value;
            if (CurrentFloor != null)
            {
                CurrentFloor.FloorWidth = value;
            }
            if (BSPGenerator != null)
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
            gridHeight = value;            
            TilemapData.Instance.GridHeight = value;
            if(CurrentFloor != null)
            {
                CurrentFloor.FloorHeight = value;
            }
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
    public Dungeon ThisDungeon;
    public DungeonFloor CurrentFloor = null; //{ get; set; }

    //Use this for initialization
    void Start()
    {
        LMB = new MouseClick();
        RMB = new MouseClick();
        LMB.CurrentAction = ClickActions.PLACETILE;
        RMB.CurrentAction = ClickActions.REMOVETILE;
        LMB.Type = Tile.TileType.ROOM;
        RMB.Type = Tile.TileType.NONE;
        tileEnums = new List<int>();
        gridSprite = TilemapData.Instance.gridSprite;
        TilemapData.Instance.spriteAtlas.Add(Tile.TileType.NONE, new Sprite[] { gridSprite });
        TilemapData.Instance.spriteAtlas.Add(Tile.TileType.ROOM, Resources.LoadAll<Sprite>("Sprites/DragonheartRooms"));
        TilemapData.Instance.spriteAtlas.Add(Tile.TileType.CORRIDOR, Resources.LoadAll<Sprite>("Sprites/DragonHeartCorridors"));
        //TilemapData.Instance.spriteAtlas.Add(Tile.TileType.EXIT, Resources.LoadAll<Sprite>("Sprites/DragonHeartExits"));

        ThisDungeon = new Dungeon();
        AddNewFloor();

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
                switch (RMB.CurrentAction)
                {
                    case ClickActions.REMOVETILE:
                        {
                            DeleteTile();
                            break;
                        }
                    case ClickActions.PLACETILE:
                        {
                            PlaceTile(RMB);
                            break;
                        }
                    case ClickActions.CREATEEXIT:
                        {
                            MakeTileAnExit();
                            break;
                        }
                    case ClickActions.REMOVEEXIT:
                        {
                            RemoveExit();
                            break;
                        }
                }                
            }
            //Left click
            if (Input.GetMouseButton(0))
            {
                switch (LMB.CurrentAction)
                {
                    case ClickActions.REMOVETILE:
                        {
                            DeleteTile();
                            break;
                        }
                    case ClickActions.PLACETILE:
                        {
                            PlaceTile(LMB);
                            break;
                        }
                    case ClickActions.CREATEEXIT:
                        {
                            MakeTileAnExit();
                            break;
                        }
                    case ClickActions.REMOVEEXIT:
                        {
                            RemoveExit();
                            break;
                        }
                }
            }
        }
    }

    public void BSPGenerate()
    {
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
        ThisDungeon.Floors[CurrentFloor.floorNumber] = CurrentFloor;
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
                Tiles[corridorTile.GetGridPosition()].SetTile((int)Tile.TileType.CORRIDOR);
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
                Tiles[exit].SetIsExit(true);
            }
        }
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
                toAdd.SetTile((int)Tile.TileType.NONE);
                toAdd.transform.position = transform.position + new Vector3(j, i, 0);
                toAdd.SetGridPosition(GetPosFromCoords(j, i));
                Tiles.Add(toAdd);
            }
        }
        CurrentFloor.Tiles = tileEnums;

        for(int i = 0; i < Tiles.Count; i++)
        {
            UpdateTileNeighbours(Tiles[i]);
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
                component.SetTile((int)Tile.TileType.NONE);
                UpdateAdjacentTiles(component.GetGridPosition());
                CurrentFloor.Tiles[component.GetGridPosition()] = 0;
            }
        }
    }

    //manually place a tile
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
                    component.SetTile((int)mouseButton.Type);
                    CurrentFloor.Tiles[gridPos] = (int)mouseButton.Type;
                    UpdateAdjacentTiles(gridPos);
                }
            }
        }
    }

    //makes the tile an exit. link two tiles of different types by making them both exits
    void MakeTileAnExit()
    {
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
        if (hit.collider != null)
        {
            if (hit.collider.tag == "Tile")
            {
                Tile component = hit.collider.GetComponent<Tile>();
                if (!component.CheckIfIsAnExit())
                {
                    component.SetIsExit(true);
                    int gridPos = component.GetGridPosition();
                    UpdateAdjacentTiles(gridPos);
                }
            }
        }
    }

    //removes exit status from the tile
    void RemoveExit()
    {
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
        if (hit.collider != null)
        {
            if (hit.collider.tag == "Tile")
            {
                Tile component = hit.collider.GetComponent<Tile>();
                if (component.CheckIfIsAnExit())
                {
                    component.SetIsExit(false);
                    int gridPos = component.GetGridPosition();
                    UpdateAdjacentTiles(gridPos);
                }
            }
        }
    }

    //creates a rectangle of the given tile type, used for making the rooms and corridors
    public void PlaceRect(int xPos, int yPos, int width, int height, int tileType)
    {
        for(int i = 0; i < height; i++)
        {
            for(int j = 0; j < width;j++)
            {
                try
                {
                    Tiles[GetPosFromCoords(xPos + j, yPos + i)].SetTile(tileType);
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

    //update the tile's stored neighbours
    public void UpdateTileNeighbours(Tile tile)
    {
        int gridPos = tile.GetGridPosition();
        tile.UpdateNeighbours(GetNorthTile(gridPos), GetWestTile(gridPos), GetEastTile(gridPos), GetSouthTile(gridPos));
    }

    //Does a pass of the whole grid to update sprites
    public void FullBitmaskPass()
    {
        //each surrounding tile
        Tile northTile, westTile, eastTile, southTile;

        foreach(Tile tile in Tiles)
        {
            //get north tile
            int north = GetNorth(tile.GetGridPosition());
            if(north < Tiles.Count)
            {
                northTile = Tiles[north];
            }
            else
            {
                northTile = null;
            }

            //get west tile
            int west = GetWest(tile.GetGridPosition());
            if(tile.GetGridPosition() % GridWidth != 0)
                {
                westTile = Tiles[west];
            }
            else
            {
                westTile = null;
            }

            //get east tile
            int east = GetEast(tile.GetGridPosition());
            if (east % GridWidth != 0)
            {
                eastTile = Tiles[east];
            }
            else
            {
                eastTile = null;
            }

            //get south tile
            int south = GetSouth(tile.GetGridPosition());
            if(south >= 0)
            {
                southTile = Tiles[south];
            }
            else
            {
                southTile = null;
            }

            //calculate the bitmask value for this tile
            int bitmaskValue = tile.GetBitmaskValue(northTile, westTile, eastTile, southTile);
            tile.UpdateSprite();
        }
        UpdateTileMapDataTiles();
    }

    public void LoadFloor(int floorNum)
    {
        if (ThisDungeon.Floors.ElementAtOrDefault(floorNum) != null)
        {
            CurrentFloor = ThisDungeon.Floors[floorNum];
            tileEnums = CurrentFloor.Tiles;
            GridWidth = CurrentFloor.FloorWidth;
            GridHeight = CurrentFloor.FloorHeight;
            ResizeGrid();
            for (int i = 0; i < GridWidth * GridHeight; i++)
            {
                Tiles[i].SetTileType(CurrentFloor.Tiles[i]);
            }            
            FullBitmaskPass();
            InitialiseTileEnums();
        }
        else
        {
            Debug.Log("could not retrieve floor number " + floorNum);
        }
    }

    //sets up the tile ID list, used when saving/loading with json
    private void InitialiseTileEnums()
    {
        tileEnums = new List<int>();
        for(int i = 0; i < Tiles.Count; i++)
        {
            tileEnums.Add((int)Tiles[i].GetTileType());
        }
    }

    //tells adjacent tiles to update themselves
    void UpdateAdjacentTiles(int gridPos)
    {
        int north = GetNorth(gridPos);
        int south = GetSouth(gridPos);
        int east = GetEast(gridPos);
        int west = GetWest(gridPos);

        //update the tile above this one
        if (north < Tiles.Count)
        {
            Tiles[north].UpdateSprite();
        }

        //update the tile below this one
        if (south >= 0)
        {
            Tiles[south].UpdateSprite();
        }

        //update the tile to the right of this one
        if (east % GridWidth != 0)
        {
            Tiles[east].UpdateSprite();
        }

        //update the tile to the left of this one
        if (gridPos % GridWidth != 0)
        {
            Tiles[west].UpdateSprite();
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

    //helpers for getting adjacent tiles
    Tile GetNorthTile(int origin)
    {
        int north = GetNorth(origin);
        if(north < Tiles.Count)
        {
            return Tiles[north];
        }
        else
        {
            return null;
        }
    }

    Tile GetSouthTile(int origin)
    {
        int south = GetSouth(origin);
        if (south >= 0)
        {
            return Tiles[south];
        }
        else
        {
            return null;
        }
    }

    Tile GetEastTile(int origin)
    {
        int east = GetEast(origin);
        if (east % GridWidth != 0)
        {
            return Tiles[east];
        }
        else
        {
            return null;
        }
    }

    Tile GetWestTile(int origin)
    {
        if (origin % GridWidth != 0)
        {
            return Tiles[GetWest(origin)];
        }
        else
        {
            return null;
        }
    }

    //returns a list index based on grid coordinates
    int GetPosFromCoords(int x, int y)
    {
        int ind = x + y * GridWidth;
        return ind;
    }

    //calculates a bitmask value based on adjacent tiles
    int GetBitmaskValue(int gridIndex, Tile.TileType type)
    {
        int maskValue = 0;
        int north = 0, east = 0, south = 0, west = 0;

        //check north
        if(gridIndex + GridWidth < tiles.Count)
        {
            if(Tiles[gridIndex + GridWidth] != null)
            {
                if(Tiles[gridIndex + GridWidth].GetTileType() == type)
                {
                    north = 1;
                }
            }
        }

        //check west
        if(gridIndex % GridWidth != 0)
        {
            if(Tiles[gridIndex - 1] != null)
            {
                if(Tiles[gridIndex - 1].GetTileType() == type)
                {
                    west = 1;
                }
            }
        }

        //check east
        if ((gridIndex + 1) % GridWidth != 0)
        {
            if (Tiles[gridIndex + 1] != null)
            {
                if (Tiles[gridIndex + 1].GetTileType() == type)
                {
                    east = 1;
                }
            }
        }

        //check south
        if (gridIndex - GridWidth >= 0)
        {
            if (Tiles[gridIndex - GridWidth] != null)
            {
                if (Tiles[gridIndex - GridWidth].GetTileType() == type)
                {
                    south = 1;
                }
            }
        }

        //calculate the bitmask value with the results of each check
        maskValue += north + west * 2 + east * 4 + south * 8;  //+ edgeHack;
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

    //add a new floor to this dungeon
    public void AddNewFloor()
    {
        //inserts the new floor to be directly after the current one
        DungeonFloor newFloor = new DungeonFloor(GridWidth, GridHeight);
        if (ThisDungeon.Floors.Count > 0)
        {
            if (CurrentFloor.floorNumber + 1 == ThisDungeon.Floors.Count)
            {
                ThisDungeon.Floors.Add(newFloor);
            }
            else
            {
                ThisDungeon.Floors.Insert(CurrentFloor.floorNumber + 1, newFloor);
            }
        }
        else
        {
            ThisDungeon.Floors.Add(newFloor);
        }

        //update the floor numbers
        for(int i = 0; i < ThisDungeon.Floors.Count; i++)
        {
            ThisDungeon.Floors[i].SetFloorNumber(i);
        }

        CurrentFloor = newFloor;
        //create the new empty floor
        InitialiseEmptyTileMap();
        InitialiseTileEnums();
        CurrentFloor.Tiles = tileEnums;
    }

    public void RemoveCurrentFloor()
    {
        if (ThisDungeon.Floors.Count > 1)
        {
            int floorNumber = CurrentFloor.floorNumber;
            if (ThisDungeon.Floors.Count > 1)
            {
                if (floorNumber == 0)
                {
                    LoadFloor(1);
                }
                else
                {
                    LoadFloor(floorNumber - 1);
                }
            }

            ThisDungeon.Floors.RemoveAt(floorNumber);

            //update the floor numbers
            for (int i = 0; i < ThisDungeon.Floors.Count; i++)
            {
                ThisDungeon.Floors[i].SetFloorNumber(i);
            }
        }
    }
}


//int edgeHack = 0;   //this one is used for the exits with a wall on one side
//switch (type)
//{
//    case Tile.TileType.ROOM:
//        //get the north value
//        if (gridIndex + GridWidth < Tiles.Count)
//        {
//            if (Tiles[gridIndex + GridWidth] != null)
//            {
//                if (Tiles[gridIndex + GridWidth].GetTileType() == Tile.TileType.ROOM || Tiles[gridIndex + GridWidth].GetTileType() == Tile.TileType.EXIT)
//                {
//                    north = 1;
//                }
//            }
//        }
//        //get the west value
//        if (gridIndex % GridWidth != 0)
//        {
//            if (Tiles[gridIndex - 1] != null)
//            {
//                if (Tiles[gridIndex - 1].GetTileType() == Tile.TileType.ROOM || Tiles[gridIndex - 1].GetTileType() == Tile.TileType.EXIT)
//                {
//                    west = 1;
//                }
//            }
//        }
//        //get the east value
//        if ((gridIndex + 1) % GridWidth != 0)
//        {
//            if (Tiles[gridIndex + 1] != null)
//            {
//                if (Tiles[gridIndex + 1].GetTileType() == Tile.TileType.ROOM || Tiles[gridIndex + 1].GetTileType() == Tile.TileType.EXIT)
//                {
//                    east = 1;
//                }
//            }
//        }
//        //get the south value
//        if (gridIndex - GridWidth >= 0)
//        {
//            if (Tiles[gridIndex - GridWidth] != null)
//            {
//                if (Tiles[gridIndex - GridWidth].GetTileType() == Tile.TileType.ROOM || Tiles[gridIndex - GridWidth].GetTileType() == Tile.TileType.EXIT)
//                {
//                    south = 1;
//                }
//            }
//        }
//        break;
//    case Tile.TileType.CORRIDOR:
//        //get the north value
//        if (gridIndex + GridWidth < Tiles.Count)
//        {
//            if (Tiles[gridIndex + GridWidth] != null)
//            {
//                if (Tiles[gridIndex + GridWidth].GetTileType() == Tile.TileType.CORRIDOR || Tiles[gridIndex + GridWidth].GetTileType() == Tile.TileType.EXIT)
//                {
//                    north = 1;
//                }
//            }
//        }
//        //get the west value
//        if (gridIndex % GridWidth != 0)
//        {
//            if (Tiles[gridIndex - 1] != null)
//            {
//                if (Tiles[gridIndex - 1].GetTileType() == Tile.TileType.CORRIDOR || Tiles[gridIndex - 1].GetTileType() == Tile.TileType.EXIT)
//                {
//                    west = 1;
//                }
//            }
//        }
//        //get the east value
//        if ((gridIndex + 1) % GridWidth != 0)
//        {
//            if (Tiles[gridIndex + 1] != null)
//            {
//                if (Tiles[gridIndex + 1].GetTileType() == Tile.TileType.CORRIDOR || Tiles[gridIndex + 1].GetTileType() == Tile.TileType.EXIT)
//                {
//                    east = 1;
//                }
//            }
//        }
//        //get the south value
//        if (gridIndex - GridWidth >= 0)
//        {
//            if (Tiles[gridIndex - GridWidth] != null)
//            {
//                if (Tiles[gridIndex - GridWidth].GetTileType() == Tile.TileType.CORRIDOR || Tiles[gridIndex - GridWidth].GetTileType() == Tile.TileType.EXIT)
//                {
//                    south = 1;
//                }
//            }
//        }
//        break;
//    case Tile.TileType.EXIT:
//        //get the north value
//        if (gridIndex + GridWidth < Tiles.Count)
//        {
//            if (Tiles[gridIndex + GridWidth] != null)
//            {
//                if (Tiles[gridIndex + GridWidth].GetTileType() == Tile.TileType.CORRIDOR)
//                {
//                    north = 1;
//                }
//                //if (Tiles[gridIndex + GridWidth].GetTileType() == Tile.TileType.NONE)
//                //{
//                //    edgeHack = -16;
//                //}
//            }
//        }
//        //else
//        //{
//        //    edgeHack = -16;
//        //}
//        //get the west value
//        if (gridIndex % GridWidth != 0)
//        {
//            if (Tiles[gridIndex - 1] != null)
//            {
//                if (Tiles[gridIndex - 1].GetTileType() == Tile.TileType.CORRIDOR)
//                {
//                    west = 1;
//                }
//                //if(Tiles[gridIndex - 1].GetTileType() == Tile.TileType.NONE)
//                //{
//                //    edgeHack = 16;
//                //}
//            }
//        }
//        //else
//        //{
//        //    edgeHack = 16;
//        //}
//        //get the east value
//        if ((gridIndex + 1) % GridWidth != 0)
//        {
//            if (Tiles[gridIndex + 1] != null)
//            {
//                if (Tiles[gridIndex + 1].GetTileType() == Tile.TileType.CORRIDOR)
//                {
//                    east = 1;
//                }
//                //if(Tiles[gridIndex + 1].GetTileType() == Tile.TileType.NONE)
//                //{
//                //    edgeHack = -16;
//                //}
//            }
//        }
//        else
//        //{
//        //    edgeHack = -16;
//        //}
//        //get the south value
//        if (gridIndex - GridWidth >= 0)
//        {
//            if (Tiles[gridIndex - GridWidth] != null)
//            {
//                if (Tiles[gridIndex - GridWidth].GetTileType() == Tile.TileType.CORRIDOR)
//                {
//                    south = 1;
//                }
//                //if (Tiles[gridIndex - GridWidth].GetTileType() == Tile.TileType.NONE)
//                //{
//                //    edgeHack = 16;
//                //}
//            }
//        }
//        //else
//        //{
//        //    edgeHack = 16;
//        //}
//        break;
//}