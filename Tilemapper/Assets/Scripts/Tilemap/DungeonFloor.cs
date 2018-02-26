﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DungeonFloor {
    public int FloorWidth;
    public int FloorHeight;
    public List<int> Tiles;

    public DungeonFloor(int width, int height)
    {
        FloorWidth = width;
        FloorHeight = height;
    }
}
