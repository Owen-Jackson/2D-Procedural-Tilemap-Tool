using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BSPDungeon : MonoBehaviour {

    //The starting node of the tree
    public NodeDungeon root;
    //stores the size of the grid
    public int GridWidth { get; set; }
    public int GridHeight { get; set; }
    List<LineRenderer> splitLines;
    public List<NodeDungeon> nodes;
    [SerializeField]
    List<Room> rooms;

    public void Start()
    {
        //root = new NodeDungeon(null, 0, new int[2] { 0, gridArea[1] }, new int[2] { gridArea[0], 0 }, tilemap);
        splitLines = new List<LineRenderer>();
    }

    public void SetGridSize(int width, int height)
    {
        //Debug.Log(width + " , " + height);
        GridWidth = width;
        GridHeight = height;
    }

    public void BuildTree()
    {
        //reset the root
        //root = new NodeDungeon(null, 0, new int[2] { 0, gridArea[1] }, new int[2] { gridArea[0], 0 }, tilemap);
        nodes = new List<NodeDungeon>();
        if (splitLines.Count > 0)
        {
            for(int i = 0; i < splitLines.Count; i++)
            {
                Destroy(splitLines[i].gameObject);
            }
            splitLines.Clear();
        }
        splitLines = new List<LineRenderer>();
        root = new NodeDungeon(-1, -1, GridWidth + 1, GridHeight + 1);
        nodes.Add(root);
        //branch the tree as far as it will go
        bool hasSplit;
        do
        {
            hasSplit = false;
            List<NodeDungeon> tempList = new List<NodeDungeon>();
            foreach (NodeDungeon node in nodes)
            {
                if (node.GetLeftChild() == null && node.GetRightChild() == null)
                {
                    if(node.SplitNode())
                    {
                        LineRenderer line = (Instantiate(Resources.Load("Prefabs/LineRendererDebug")) as GameObject).GetComponent<LineRenderer>();
                        if (node.IsSplitHorizontal())
                        {
                            line.SetPositions(new Vector3[2] { new Vector3(node.GetXPos() + 0.5f, node.GetYPos() + node.GetSplitPos(), 0), new Vector3(node.GetXPos() + node.GetWidth() - 0.5f, node.GetYPos() + node.GetSplitPos(), 0) });
                        }
                        else
                        {
                            line.SetPositions(new Vector3[2] { new Vector3(node.GetXPos() + node.GetSplitPos(), node.GetYPos() + 0.5f, 0), new Vector3(node.GetXPos() + node.GetSplitPos(), node.GetYPos() + node.GetHeight() - 0.5f, 0) });
                        }
                        splitLines.Add(line);
                        tempList.Add(node.GetLeftChild());
                        tempList.Add(node.GetRightChild());
                        hasSplit = true;
                    }
                }
            }
            nodes.AddRange(tempList);
        }
        while (hasSplit);
        
        //make the rooms
        root.CreateRooms();

    }

    public List<Room> GetNodesWithRooms()
    {
        rooms = new List<Room>();
        for(int i = 0; i < nodes.Count;i++)
        {
            Room temp = nodes[i].GetRoom();
            if (temp != null)
            {
                rooms.Add(temp);
            }
        }
        return rooms;
    }

    public void CreateCorridors()
    {
        root.CreateCorridorRecursive();
    }

    public List<int> GetExits()
    {
        List<int> exitPositions = new List<int>();
        for(int i = 0; i < nodes.Count;i++)
        {
            if (nodes[i].GetExits() != null)
            {
                exitPositions.AddRange(nodes[i].GetExits());
            }
        }
        return exitPositions;
    }

    public List<Tile> GetCorridors()
    {
        List<Tile> corridorsList = new List<Tile>();
        for(int i = 0; i < nodes.Count; i++)
        {
            if(nodes[i].GetCorridor() != null)
            {
                corridorsList.AddRange(nodes[i].GetCorridor());
            }
        } 
        return corridorsList;
    }
}
