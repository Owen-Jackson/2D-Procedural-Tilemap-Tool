using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileEditor : MonoBehaviour {

    [SerializeField]
    private static TileEditor instance;

    public static TileEditor Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new TileEditor();
            }
            return instance;
        }
    }
}
