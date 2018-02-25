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

    public enum TileType
    {
        NONE = 0,
        ROOM,
        CORRIDOR,
        EXIT
    };

    [SerializeField]
    TileType type;

    public void SetTileType(int ID)
    {
        type = (TileType)ID;
    }

    public void SetTile(Sprite _sprite, int typeID)
    {
        spriteRenderer.sprite = _sprite;
        type = (TileType)typeID;
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

	// Update is called once per frame
	void Update () {
		
	}

    public void DeleteMe()
    {
        spriteRenderer.sprite = transform.parent.GetComponent<Tilemap>().GetGridSprite();
        type = TileType.NONE;
    }

    public TileType GetTileType()
    {
        return type;
    }

    public void SetBitmaskValue(int mask)
    {

    }
}
