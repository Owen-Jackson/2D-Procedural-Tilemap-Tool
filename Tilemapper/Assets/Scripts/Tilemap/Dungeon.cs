using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Dungeon {

    public List<DungeonFloor> Floors;

    public Dungeon()
    {
        Floors = new List<DungeonFloor>();
    }
}
