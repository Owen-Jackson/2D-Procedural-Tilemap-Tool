using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NodeDungeon {

    protected NodeDungeon parent;

    static int minSizeForRoom = 5;

    //public NodeDungeon[] children;
    private NodeDungeon leftChild = null;
    private NodeDungeon rightChild = null;

    [SerializeField]
    protected int treeDepth;  //which layer of the tree this node is in

    int lineSplit;           //The position of the split along the axis it is splitting
    bool splitHorizontal;    //says whether the split is along the x or y axis

    Tilemap tilemap;

    //positions of the corners of the grid to create the bounds
    int xPos;
    int yPos;
    [SerializeField]
    int width;
    [SerializeField]
    int height;

    //private int[] topLeft;
    //private int[] bottomRight;

    //stores the values for the room in this node
    private bool hasARoom = false;
    private int roomWidth;
    private int roomHeight;
    [SerializeField]
    private int roomXPos;
    [SerializeField]
    private int roomYPos;

    public NodeDungeon(int x, int y, int _width, int _height) //x and y position starts in the bottom left
    {
        xPos = x;
        yPos = y;
        width = _width;
        height = _height;    
    }

    public NodeDungeon GetLeftChild()
    {
        return leftChild;
    }    

    public NodeDungeon GetRightChild()
    {
        return rightChild;
    }

    public int GetSplitPos()
    {
        return lineSplit;
    }

    public bool IsSplitHorizontal()
    {
        return splitHorizontal;
    }

    public int GetXPos()
    {
        return xPos;
    }

    public int GetRoomXPos()
    {
        return roomXPos;
    }

    public int GetRoomYPos()
    {
        return roomYPos;
    }

    public int GetRoomWidth()
    {
        return roomWidth;
    }

    public int GetRoomHeight()
    {
        return roomHeight;
    }

    public int GetYPos()
    {
        return yPos;
    }

    public int GetWidth()
    {
        return width;
    }

    public int GetHeight()
    {
        return height;
    }

    public static void SetMinSizeForRoom(int min)
    {
        minSizeForRoom = min;
    }

    public bool SplitNode()
    {
        //get direction of split
           //if height is too long, split along the y axis
        if (height > width && height / width >= 1.5f)
        {
            splitHorizontal = true;
        }
        //if width is too long, split vertically
        else if (width > height && width / height >= 1.5f)
        {
            splitHorizontal = false;
        }
        //otherwise randomly select
        else
        {
            splitHorizontal = Random.Range(0f, 1f) > 0.5;
        }

        //check this node has enough room to split
        int size = CalculateMaxRoomSize();
        //check that there is enough room for the split
        if(size < minSizeForRoom)
        {
            //try the other split axis before giving up
            //splitHorizontal = !splitHorizontal;
            //size = CalculateMaxRoomSize();
            //if (size <= minSizeForRoom)
            //{
                //not enough room left
                return false;
            //}
        }

        //get which coordinate to split along
        lineSplit = Random.Range(minSizeForRoom, size);

        //create left and right children
        if(splitHorizontal)
        {
            leftChild = new NodeDungeon(xPos, yPos, width, lineSplit);
            rightChild = new NodeDungeon(xPos, yPos + lineSplit, width, height - lineSplit);
        }
        else
        {
            leftChild = new NodeDungeon(xPos, yPos, lineSplit, height);
            rightChild = new NodeDungeon(xPos + lineSplit, yPos, width - lineSplit, height);
        }

        return true;
    }

    int CalculateMaxRoomSize()
    {
        int size;
        if (splitHorizontal)
        {
            size = height - minSizeForRoom;
        }
        else
        {
            size = width - minSizeForRoom;
        }
        return size;
    }

    //Recursively generates the rooms
    public void CreateRooms()
    {
        //if this node has children, it will not create a room
        if(leftChild != null || rightChild != null)
        {
            //create rooms in the children
            if(leftChild != null)
            {
                leftChild.CreateRooms();
            }
            if(rightChild != null)
            {
                rightChild.CreateRooms();
            }
        }
        else
        {
            //this node is a leaf so make a room for it
            hasARoom = true;
            roomWidth = Random.Range(3, width);
            roomHeight = Random.Range(3, height);
            roomXPos = xPos + Random.Range(1, width - roomWidth + 1);
            roomYPos = yPos + Random.Range(1, height - roomHeight + 1);
        }
    }

    public bool HasARoom()
    {
        return hasARoom;
    }
    /*
    public NodeDungeon(NodeDungeon _parent, int _depth, int[] _topLeft, int[] _bottomRight, Tilemap _tilemap)
    {
        parent = _parent;
        treeDepth = _depth;
        tilemap = _tilemap;
        //Debug.Log("bottom right = " + _bottomRight[0] + ", " + _bottomRight[1]);
        topLeft = new int[2] { _topLeft[0] + 1, _topLeft[1] - 1 };
        bottomRight = new int[2] { _bottomRight[0] - 1, _bottomRight[1] + 1 };
        gridArea = new int[2] { _bottomRight[0] - _topLeft[0], _topLeft[1] - _bottomRight[1]};
        Debug.Log("initial grid area = " + gridArea[0] + ", " + gridArea[1]);
    }

    public int GetSplit()
    {
        return lineSplit;
    }

    public int GetDepth()
    {
        return treeDepth;
    }

    public List<NodeDungeon> SplitBranch(List<NodeDungeon> nodeList)
    {
        //if this node's bounds are larger than the minimum size, split tree
        if (gridArea[0] > 4 && gridArea[1] > 4)
        {
            children = new NodeDungeon[2] { null, null };
            int[] childGridATL = topLeft; //top left
            int[] childGridABR; //bottom right
            int[] childGridBTL; //top left
            int[] childGridBBR = bottomRight; //bottom right

            //split randomly along the x or y axis
            int axis = Random.Range(0, 1);  //0 = split along x-axis, 1 = split along y-axis
            int split;
            if (axis == 0)
            {
                split = Random.Range(1, gridArea[1]);
            }
            else
            {
                split = Random.Range(1, gridArea[0]);
            }
            lineSplit = split;
            //split--;

            //create two smaller grids from the split
            if (split == 0)
            {
                //Debug.Log("splitting along the x axis at " + split);
                childGridABR = new int[2] { topLeft[0] + split, bottomRight[1] };

                childGridBTL = new int[2] { topLeft[0] + split, topLeft[1] };
            }
            else
            {
                //Debug.Log("splitting along the y axis at " + split);
                //Debug.Log("ABR = " + bottomRight[0] + ", " + (topLeft[1] - split));
                childGridABR = new int[2] { bottomRight[0], topLeft[1] - split };
                //Debug.Log("BTL = " + topLeft[0] + ", " + (topLeft[1] - split));
                childGridBTL = new int[2] { topLeft[0], topLeft[1] - split };
            }

            //branch off the children with the new grids
            //Debug.Log("child A topleft at: " + childGridATL[0] + ", " + childGridATL[1]);
            //Debug.Log("child B topleft at: " + childGridBTL[0] + ", " + childGridBTL[1]);
            children[0] = new NodeDungeon(this, treeDepth + 1, childGridATL, childGridABR, tilemap);
            children[1] = new NodeDungeon(this, treeDepth + 1, childGridBTL, childGridBBR, tilemap);

            //Repeat this function in the children it applies to
            if (childGridABR[0] - childGridATL[0] > 4 && childGridATL[1] - childGridABR[1] > 4)
            {
                children[0].SplitBranch(nodeList);
                //Debug.Log("splitting left side");
            }
            if (childGridBBR[0] - childGridBTL[0] > 4 && childGridBTL[1] - childGridBBR[1] > 4)
            {
                children[1].SplitBranch(nodeList);
                //Debug.Log("splitting right side");
            }
            nodeList.Add(children[0]);
            nodeList.Add(children[1]);
        }

        return nodeList;
    }

    public void CreateRooms()
    {
        if(children == null)
        {
            int[] roomStartPos = new int[2] { Random.Range(topLeft[0], bottomRight[0]), Random.Range(topLeft[1], bottomRight[1]) };
            int sizeX = Random.Range(3, gridArea[0]);
            int sizeY = Random.Range(3, gridArea[1]);
            //Debug.Log("gridArea = " + gridArea[0] + ", " + gridArea[1]);
            //Debug.Log("placing room. start pos = " + roomStartPos[0] + ", " + roomStartPos[1]);
            //Debug.Log("sizeX = " + sizeX + " sizeY = " + sizeY);
            tilemap.PlaceRoom(sizeX, sizeY, roomStartPos[0], roomStartPos[1]);
        }
        else
        {
            children[0].CreateRooms();
            children[1].CreateRooms();
        }
    }
    */
}
