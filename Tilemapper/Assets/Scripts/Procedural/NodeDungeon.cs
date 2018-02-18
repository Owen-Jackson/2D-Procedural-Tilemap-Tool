using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NodeDungeon
{

    protected NodeDungeon parent;

    static int minSizeForRoom = 6;

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
    private Room room;

    //stores the corridor positions that link this node's children rooms
    private List<Rect> corridor;

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

    public Room GetRoom()
    {
        if (room != null)
        {
            return room;
        }
        
        else
        {
            Room leftRoom = null;
            Room rightRoom = null;
            if(leftChild != null)
            {
                leftRoom = leftChild.GetRoom();              
            }            
            if(rightChild != null)
            {
                rightRoom = rightChild.GetRoom();
            }

            if(leftRoom == null && rightRoom == null)
            {
                return null;
            }
            else if(rightRoom == null)
            {
                return leftRoom;
            }
            else if(leftRoom == null)
            {
                return rightRoom;
            }
            else if(Random.Range(0, 1) < 0.5)
            {
                return leftRoom;
            }
            else
            {
                return rightRoom;
            }                    
        }
    }

    public List<Rect> GetCorridor()
    {
        return corridor;
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
        if (size < minSizeForRoom)
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
        if (splitHorizontal)
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
        if (leftChild != null || rightChild != null)
        {
            //create rooms in the children
            if (leftChild != null)
            {
                leftChild.CreateRooms();
            }
            if (rightChild != null)
            {
                rightChild.CreateRooms();
            }

            //if this node has two children then make a corridor between them
            if (leftChild != null && rightChild != null)
            {
                //Debug.Log("making corridor between: " + leftChild.GetRoom() + " , " + rightChild.GetRoom());
                CreateCorridor(leftChild.GetRoom(), rightChild.GetRoom());
            }
        }
        else
        {
            //this node is a leaf so make a room for it
            room = new Room(Random.Range(3, width), Random.Range(3, height));
            room.XPos = xPos + Random.Range(1, width - room.Width + 1);
            room.YPos = yPos + Random.Range(1, height - room.Height + 1);
            room.SetEdges();
        }
    }

    //Connects rooms together
    public void CreateCorridor(Room leftRoom, Room rightRoom)
    {
        corridor = new List<Rect>();
        Vector2 point1 = new Vector2(Random.Range(leftRoom.Left + 1, leftRoom.Right - 2), Random.Range(leftRoom.Bottom + 1, leftRoom.Top - 2));
        Vector2 point2 = new Vector2(Random.Range(rightRoom.Left + 1, rightRoom.Right - 2), Random.Range(rightRoom.Bottom + 1, rightRoom.Top - 2));

        int w = (int)(point2.x - point1.x);
        int h = (int)(point2.y - point1.y);

        if (w < 0)
        {
            if (h < 0)
            {
                if (Random.Range(0, 1) < 0.5)
                {
                    corridor.Add(new Rect(point2.x, point1.y, Mathf.Abs(w), 1));
                    corridor.Add(new Rect(point2.x, point2.y, 1, Mathf.Abs(h)));
                }
                else
                {
                    corridor.Add(new Rect(point2.x, point2.y, Mathf.Abs(w), 1));
                    corridor.Add(new Rect(point1.x, point2.y, 1, Mathf.Abs(h)));
                }
            }
            else if (h > 0)
            {
                if (Random.Range(0, 1) < 0.5)
                {
                    corridor.Add(new Rect(point2.x, point1.y, Mathf.Abs(w), 1));
                    corridor.Add(new Rect(point2.x, point1.y, 1, Mathf.Abs(h)));
                }
                else
                {
                    corridor.Add(new Rect(point2.x, point2.y, Mathf.Abs(w), 1));
                    corridor.Add(new Rect(point1.x, point1.y, 1, Mathf.Abs(h)));
                }
            }
            else if (h == 0)
            {
                corridor.Add(new Rect(point2.x, point2.y, Mathf.Abs(w), 1));
            }
        }
        else if (w > 0)
        {
            if (h < 0)
            {
                if (Random.Range(0, 1) < 0.5)
                {
                    corridor.Add(new Rect(point1.x, point2.y, Mathf.Abs(w), 1));
                    corridor.Add(new Rect(point1.x, point2.y, 1, Mathf.Abs(h)));
                }
                else
                {
                    corridor.Add(new Rect(point1.x, point1.y, Mathf.Abs(w), 1));
                    corridor.Add(new Rect(point2.x, point2.y, 1, Mathf.Abs(h)));
                }
            }
            else if (h > 0)
            {
                if (Random.Range(0, 1) < 0.5)
                {
                    corridor.Add(new Rect(point1.x, point1.y, Mathf.Abs(w), 1));
                    corridor.Add(new Rect(point2.x, point1.y, 1, Mathf.Abs(h)));
                }
                else
                {
                    corridor.Add(new Rect(point1.x, point2.y, Mathf.Abs(w), 1));
                    corridor.Add(new Rect(point1.x, point1.y, 1, Mathf.Abs(h)));
                }
            }
            else if (h == 0)
            {
                corridor.Add(new Rect(point1.x, point1.y, Mathf.Abs(w), 1));
            }

        }
        else if (w == 0)
        {
            if (h < 0)
            {
                corridor.Add(new Rect(point2.x, point2.y, 1, Mathf.Abs(h)));
            }
            else if (h > 0)
            {
                corridor.Add(new Rect(point1.x, point1.y, 1, Mathf.Abs(h)));
            }
        }
    }
}
