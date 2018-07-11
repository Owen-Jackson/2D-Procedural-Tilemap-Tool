using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour {
    [SerializeField]
    private SpriteRenderer spriteRenderer = null;
    [SerializeField]
    private int gridPosition;
    [SerializeField]
    private bool isSolid;
    [SerializeField]
    private int ID;
    [SerializeField]
    private int bitmaskValue = 0;
    [SerializeField]
    private bool isExit = false;
    [SerializeField]
    private GameObject overlay;

    public enum TileType
    {
        NONE = 0,
        ROOM,
        CORRIDOR,
    };

    [SerializeField]
    TileType type;

    Tile northNeighbour = null, eastNeighbour = null, westNeighbour = null, southNeighbour = null;

    public void SetTileType(int ID)
    {
        type = (TileType)ID;
    }

    public void SetTile(int typeID)
    {
        SetTileType(typeID);
        UpdateSprite();
    }

    public void SetGridPosition(int pos)
    {
        gridPosition = pos;
    }

    //Used to calculate bitmask value
    public int GetGridPosition()
    {
        return gridPosition;
    }

    // Use this for initialization
    void Awake () {
        spriteRenderer = GetComponent<SpriteRenderer>();
	}    	

    public void DeleteMe()
    {
        spriteRenderer.sprite = transform.parent.GetComponent<Tilemap>().GetGridSprite();
        type = TileType.NONE;
        SetIsExit(false);
    }

    public TileType GetTileType()
    {
        return type;
    }

    public bool CheckIfIsAnExit()
    {
        return isExit;
    }

    public void SetIsExit(bool isThisAnExit)
    {
        isExit = isThisAnExit;
        //toggle the overlay for displaying the exit
        if(isExit)
        {
            overlay.SetActive(true);
        }
        else if(overlay.activeSelf)
        {
            overlay.SetActive(false);
        }
        UpdateSprite();
    }

    //calculates a bitmask value based on adjacent tiles
    virtual public int GetBitmaskValue(Tile northTile = null, Tile westTile = null, Tile eastTile = null, Tile southTile = null)    //pass in the surrounding tiles to check against
    {
        //ignore if the tile is a blank space
        if(type == TileType.NONE)
        {
            return 0;
        }

        int maskValue = 0;
        int north = 0, west = 0, east = 0, south = 0;

        //check north
        if (northTile != null)
        {
            if (northTile.GetTileType() == type)
            {
                north = 1;
            }
            else if(isExit && northTile.isExit)
            {
                north = 1;
            }
        }

        //check west
        if (westTile != null)
        {
            if (westTile.GetTileType() == type)
            {
                west = 1;
            }
            else if(isExit && westTile.isExit)
            {
                west = 1;
            }
        }

        //check east
        if (eastTile != null)
        {
            if (eastTile.GetTileType() == type)
            {
                east = 1;
            }
            else if(isExit && eastTile.isExit)
            {
                east = 1;
            }
        }

        //check south
        if (southTile != null)
        {
            if (southTile.GetTileType() == type)
            {
                south = 1;
            }
            else if(isExit && southTile.isExit)
            {
                south = 1;
            }
        }

        //calculate the bitmask value with the results of each check
        maskValue += north + west * 2 + east * 4 + south * 8;
        return maskValue;
    }

    public void UpdateNeighbours(Tile northTile = null, Tile westTile = null, Tile eastTile = null, Tile southTile = null)
    {
        northNeighbour = northTile;
        westNeighbour = westTile;
        eastNeighbour = eastTile;
        southNeighbour = southTile;
    }

    public void UpdateSprite()
    {
        bitmaskValue = GetBitmaskValue(northNeighbour, westNeighbour, eastNeighbour, southNeighbour);
        spriteRenderer.sprite = TilemapData.Instance.spriteAtlas[type][bitmaskValue];    //load the sprite with the associated index
    }
}
