using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DungeonFloor {
    public int floorNumber;
    public int FloorWidth;
    public int FloorHeight;
    public List<int> Tiles;
    public List<bool> Exits;

    public DungeonFloor(int width, int height)
    {
        FloorWidth = width;
        FloorHeight = height;
        Tiles = new List<int>();
        Exits = new List<bool>();
    }

    public void SetFloorNumber(int newNum)
    {
        floorNumber = newNum;
    }
}
