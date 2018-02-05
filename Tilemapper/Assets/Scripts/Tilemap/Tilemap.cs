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

    public void RandomiseTileMap()
    {
        if (tiles == null)
        {
            InitialiseEmptyTileMap();
        }
        for (int i = 0; i < gridHeight * gridWidth; i++)
        {
            tiles[i].DeleteMe();
            int num = Random.Range(0, roomSprites.Length);
            tiles[i].SetTile(roomSprites[num], 1);
        }
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
                Debug.Log("adding room tile");
                hit.collider.GetComponent<Tile>().SetTile(roomSprites[0], 1);
            }
        }
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

    void AddNeighbourTiles(int gridIndex)
    {

    }

    public Sprite GetGridSprite()
    {
        return gridSprite;
    }
    
}
