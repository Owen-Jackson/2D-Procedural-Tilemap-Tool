using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Tilemap : MonoBehaviour {
    //Links the IDs of each tile to the sprite to load in
    enum RoomTileIDs
    {
        DEADENDEAST = 0,
        DEADENDNORTH,
        DEADENDSOUTH,
        DEADENDWEST,
        EXITEAST,
        EXITNORTH,
        EXITNORTHEAST,
        EXITNORTHWEST,
        EXITSOUTH,
        EXITSOUTHEAST,
        EXITSOUTHWEST,
        EXITWEST,
        ROOMEAST,
        ROOMNORTH,
        ROOMNORTHEAST,
        ROOMNORTHWEST,
        ROOMOPEN,       //Plain floor tile
        ROOMSOUTH,
        ROOMSOUTHEAST,
        ROOMSURROUNDED, //Walls on all four sides
        ROOMSOUTHWEST,
        ROOMWEST
        /*
        CORRIDORHORIZONTAL = 0,
        CORRIDORVERTICAL = 1,
        CROSSROAD = 2,
        DEADENDEAST = 3,
        DEADENDNORTH = 4,
        DEADENDSOUTH = 5,
        DEADENDWEST = 6,
        EXITEAST = 7,
        EXITNORTH = 8,
        EXITSOUTH = 9,
        EXITWEST = 10,
        OPENFLOOR = 11,
        WALLEAST = 12,
        WALLNORTH = 13,
        WALLNORTHEAST = 14,
        WALLNORTHWEST = 15,
        WALLSOUTH = 16,
        WALLSOUTHEAST = 17,
        WALLSOUTHWEST = 18,
        WALLWEST = 19,
        CORNERNORTHEAST = 20,
        CORNERNORTHWEST = 21,
        CORNERSOUTHEAST = 22,
        CORNERSOUTHWEST = 23,
        CORRIDORSURROUNDED = 24,
        EXITNORTHEAST = 25,
        EXITNORTHWEST = 26,
        EXITSOUTHEAST = 27,
        EXITSOUTHWEST = 28
        */
    };

    enum CorridorTileIDs
    {
        CORNERNORTHEAST = 0,
        CORNERNORTHWEST,
        CORNERSOUTHEAST,
        CORNERSOUTHWEST,
        CORRIDORHORIZONTAL,
        CORRIDORSURROUNDED,     //Walls on all four sides (same sprite as room)
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
    private Sprite gridSprite;  //The default sprite for the grid (revert to when deleting a tile from the tilemap)

    //Can store a grid of tiles
    [SerializeField]
    int gridWidth;
    [SerializeField]
    int gridHeight;
    [SerializeField]
    int tileSize;
    [SerializeField]
    List<Tile> tiles;

    [SerializeField]
    TileMode LMB;
    [SerializeField]
    TileMode RMB;

    [SerializeField]
    BSPDungeon BSPGenerator;

    public void BSPGenerate()
    {
        ClearGrid();
        BSPGenerator.BuildTree();
        AddRooms();
        FullBitmaskPass();
    }

    public void AddRooms()
    {
        List<NodeDungeon> roomNodes = BSPGenerator.GetNodesWithRooms();
        if(roomNodes.Count > 0)
        {
            foreach(NodeDungeon room in roomNodes)
            {
                PlaceRoom(room.GetRoomXPos(), room.GetRoomYPos(), room.GetRoomWidth(), room.GetRoomHeight());
            }
        }
    }

    public void RandomiseTileMap()
    {
        if (tiles == null)
        {
            InitialiseEmptyTileMap();
        }
        int count = 0;
        for (int i = 0; i < gridHeight * gridWidth; i++)
        {
            tiles[i].DeleteMe();
            if (Random.Range(0f, 1f) > 0.5f)
            {
                count++;
                Debug.Log("random succeeded");
                tiles[i].SetTile(roomSprites[0], 1);
            }
        }
        Debug.Log("num of successes: " + count);
        FullBitmaskPass();
    }

    public void ClearGrid()
    {
        if (tiles == null)
        {
            InitialiseEmptyTileMap();
        }
        for (int i = 0; i < gridHeight * gridWidth; i++)
        {
            tiles[i].DeleteMe();
        }
    }


    public void InitialiseEmptyTileMap()
    {
        tiles = new List<Tile>();
        for (int i = 0; i < gridHeight; i++)
        {
            for (int j = 0; j < gridWidth; j++)
            {
                Tile toAdd = Instantiate(Resources.Load<GameObject>("Prefabs/BaseTile").GetComponent<Tile>(), transform);
                toAdd.SetTile(gridSprite, 0);
                toAdd.transform.position = transform.position + new Vector3(j, i, 0);
                toAdd.SetGridPosition(j + i * gridHeight);
                tiles.Add(toAdd);
            }
        }
    }

	// Use this for initialization
	void Start () {
        gridSprite = Resources.Load<Sprite>("Sprites/GridCell");
        roomSprites = Resources.LoadAll<Sprite>("Sprites/DragonheartRooms");
        corridorSprites = Resources.LoadAll<Sprite>("Sprites/DragonHeartCorridors");
        InitialiseEmptyTileMap();
        //RandomiseTileMap();
        LMB = TileMode.ROOM;
        RMB = TileMode.DELETE;
        if (GetComponent<BSPDungeon>())
        {
            BSPGenerator = GetComponent<BSPDungeon>();
        }
        BSPGenerator.SetGridSize(gridWidth, gridHeight);
    }

    // Update is called once per frame
    void Update () {
        //Right click
        if(Input.GetMouseButton(1))
        {
            switch(RMB)
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
        if(Input.GetMouseButton(0))
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
                    component.SetTile(roomSprites[GetBitmaskValue(gridPos)], 1);
                    UpdateAdjacentTiles(gridPos, Tile.TileType.ROOM);
                    component.SetTile(roomSprites[GetBitmaskValue(gridPos)], 1);
                }
            }
        }
    }

    public void PlaceRoom(int xPos, int yPos, int width, int height)
    {
        for(int i = 0; i < height; i++)
        {
            for(int j = 0; j < width;j++)
            {
                tiles[GetPosFromCoords(xPos + j, yPos + i)].SetTile(roomSprites[0], 1);
            }
        }
        /*
        Debug.Log("SizeX = " + sizeX + " , SizeY = " + sizeY);
        Debug.Log("Starting pos = " + startX + ", " + startY);
        for (int i = 0; i < sizeX; i++)
        {
            for(int j = 0; j < sizeY;j++)
            {
               Debug.Log("input coords: " + (startX + i) + " , " + (startY - j));
                //Debug.Log(GetPosFromCoords(startX + i, startY - j) - 1);
                //if (GetPosFromCoords(startX + i, startY - j) - 1 >= 0 && GetPosFromCoords(startX + i, startY - j) - 1 < tiles.Count)
                //{
                    tiles[GetPosFromCoords(startX + i, startY - j) - 1].SetTile(roomSprites[0], 1);
                //}
            }
        }
        */
    }

    //Does a pass of the whole grid to update sprites
    public void FullBitmaskPass()
    {
        foreach(Tile tile in tiles)
        {
            if (tile.GetTileType() == Tile.TileType.ROOM)
            {
                tile.SetTile(roomSprites[GetBitmaskValue(tile.GetGridPosition())], 1);
            }
        }
    }

    //tells adjacent tiles to update themselves
    void UpdateAdjacentTiles(int gridPos, Tile.TileType type)
    {
        int north = GetNorth(gridPos);
        int south = GetSouth(gridPos);
        int east = GetEast(gridPos);
        int west = GetWest(gridPos);

        if (north < tiles.Count)
        {
            if (tiles[north].GetTileType() == type)
            {
                tiles[north].SetTile(roomSprites[GetBitmaskValue(north)], 1);
            }
        }

        if (south >= 0)
        {
            if (tiles[south].GetTileType() == type)
            {
                tiles[south].SetTile(roomSprites[GetBitmaskValue(south)], 1);
            }
        }

        if (east % gridWidth != 0)
        {
            if (tiles[east].GetTileType() == type)
            {
                tiles[east].SetTile(roomSprites[GetBitmaskValue(east)], 1);
            }
        }

        if (gridPos % gridWidth != 0)
        {
            if (tiles[west].GetTileType() == type)
            {
                tiles[west].SetTile(roomSprites[GetBitmaskValue(west)], 1);
            }
        }
    }

    //Helper functions for grid positions
    int GetNorth(int origin)
    {
        return origin + gridWidth;
    }

    int GetSouth(int origin)
    {
        return origin - gridWidth;
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
        int ind = x + y * gridWidth;
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
    int GetBitmaskValue(int gridIndex)
    {
        int maskValue = 0;
        //get the north value
        int north = 0, east = 0, south = 0, west = 0;
        if (gridIndex + gridWidth < tiles.Count)
        {
            if (tiles[gridIndex + gridWidth] != null)
            {
                if (tiles[gridIndex + gridWidth].GetTileType() == Tile.TileType.ROOM)
                {
                    north = 1;
                }
            }
        }
        //get the west value
        if (gridIndex % gridWidth != 0)
        {
            if (tiles[gridIndex - 1] != null)
            {
                if (tiles[gridIndex - 1].GetTileType() == Tile.TileType.ROOM)
                {
                    west = 1;
                }
            }
        }
        //get the east value
        if ((gridIndex + 1) % gridWidth != 0)
        {
            if (tiles[gridIndex + 1] != null)
            {
                if (tiles[gridIndex + 1].GetTileType() == Tile.TileType.ROOM)
                {
                    east = 1;
                }
            }
        }
        //get the south value
        if (gridIndex - gridWidth >= 0)
        {
            if (tiles[gridIndex - gridWidth] != null)
            {
                if (tiles[gridIndex - gridWidth].GetTileType() == Tile.TileType.ROOM)
                {
                    south = 1;
                }
            }
        }
        maskValue = north + west * 2 + east * 4 + south * 8;
        return maskValue;
    }

    public Sprite GetGridSprite()
    {
        return gridSprite;
    }
    
}
