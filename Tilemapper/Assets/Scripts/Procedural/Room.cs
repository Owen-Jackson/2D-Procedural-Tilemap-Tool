using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room {

    [SerializeField]
    public int Width { get; set; }
    [SerializeField]
    public int Height { get; set; }
    [SerializeField]
    public int XPos { get; set; }
    [SerializeField]
    public int YPos { get; set; }

    //used for the edges of the 
    public int Top { get; set; }
    public int Bottom { get; set; }
    public int Left { get; set; }
    public int Right { set; get; }

    public Room(int width, int height)
    {
        Width = width;
        Height = height;
    }

    //sets the edge coordinates based on size and position
    public void SetEdges()
    {
        Top = YPos + Height;
        Bottom = YPos;
        Left = XPos;
        Right = XPos + Width;
    }
}
