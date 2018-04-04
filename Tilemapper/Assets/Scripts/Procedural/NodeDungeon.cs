using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class NodeDungeon
{
    public class AStarNode
    {
        public int FScore { get; set; } 
        public int GScore { get; set; } 
        public int HScore { get; set; } 
        public Vector2 Position { get; set; }
        public AStarNode Parent { get; set; }
        public Tile NodeTile { get; set; }  //stores the tile at this node
    }

    //a list of all of the tiles for the A* algorithm
    List<AStarNode> aStarNodes = new List<AStarNode>();

    protected NodeDungeon parent;

    static int minSizeForRoom = 6;

    //public NodeDungeon[] children;
    private NodeDungeon leftChild = null;
    private NodeDungeon rightChild = null;

    int lineSplit;           //The position of the split along the axis it is splitting
    bool splitHorizontal;    //says whether the split is along the x or y axis

    //positions of the corners of the grid to create the bounds
    int xPos;
    int yPos;
    int width;
    int height;

    //stores the values for the room in this node
    private Room room;

    //stores the corridor positions that link this node's children rooms
    private List<Tile> corridor;

    private List<int> exitPositions;

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

    public List<Tile> GetCorridor()
    {
        return corridor;
    }

    public List<int> GetExits()
    {
        return exitPositions;
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
            return false;
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

            /*
            //if this node has two children then make a corridor between them
            if (leftChild != null && rightChild != null)
            {
                //Debug.Log("making corridor between: " + leftChild.GetRoom() + " , " + rightChild.GetRoom());
                CreateCorridor(leftChild.GetRoom(), rightChild.GetRoom());
            }
            */
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

    public void CreateCorridorRecursive()
    {
        //if this node has children, it will be able to create a corridor
        if (leftChild != null || rightChild != null)
        {
            //create rooms in the children
            if (leftChild != null)
            {
                leftChild.CreateCorridorRecursive();
            }
            if (rightChild != null)
            {
                rightChild.CreateCorridorRecursive();
            }

            //if this node has two children then make a corridor between them
            if (leftChild != null && rightChild != null)
            {
                //Debug.Log("making corridor between: " + leftChild.GetRoom() + " , " + rightChild.GetRoom());
                CreateCorridor(leftChild.GetRoom(), rightChild.GetRoom());
            }
        }
    }

    //Connects rooms together
    public void CreateCorridor(Room leftRoom, Room rightRoom)
    {
        corridor = new List<Tile>();
        //Vector2 point1 = new Vector2(Random.Range(leftRoom.Left, leftRoom.Right), Random.Range(leftRoom.Bottom, leftRoom.Top));
        //Vector2 point2 = new Vector2(Random.Range(rightRoom.Left, rightRoom.Right), Random.Range(rightRoom.Bottom, rightRoom.Top));

        //get where to exit each room
        Vector3 point1 = GetExitEdge(leftRoom, rightRoom);
        Vector3 point2 = GetExitEdge(rightRoom, leftRoom);
        Vector2 exit1 = GetExitPos(point1, (int)point1.z);
        Vector2 exit2 = GetExitPos(point2, (int)point2.z);

        corridor.AddRange(AStar(point1, point2, exit1, exit2));        
    }

    //Randomly selects where to create an exit out of this room from
    private Vector3 GetExitEdge(Room room1, Room room2)
    {
        Vector3 point = new Vector3();
        bool validPoint = false;
        List<KeyValuePair<int, int>> edgeDistances = new List<KeyValuePair<int, int>>();
        edgeDistances.Add(new KeyValuePair<int, int>(0, Mathf.Abs(room2.Left - room1.Left)));
        edgeDistances.Add(new KeyValuePair<int, int>(1, Mathf.Abs(room2.Right - room1.Right)));
        edgeDistances.Add(new KeyValuePair<int, int>(2, Mathf.Abs(room2.Top - room1.Top)));
        edgeDistances.Add(new KeyValuePair<int, int>(3, Mathf.Abs(room2.Bottom - room1.Bottom)));

        edgeDistances = edgeDistances.OrderBy(x => x.Value).ToList();

        //randomly pick a side to start tunneling from
        int bestInd = 0;
        while (!validPoint)
        {            
            switch (edgeDistances[bestInd].Key)
            {
                //exit out of the left side
                case 0:
                    if (room1.Left > 0)
                    {
                        point = new Vector3(room1.Left - 1, Random.Range(room1.Bottom, room1.Top), 0);
                        //Debug.Log("selecting from left" + point);
                        validPoint = true;
                    }
                    break;
                //exit out of the right side
                case 1:
                    if (room1.Right < TilemapData.Instance.GridWidth - 1)
                    {
                        point = new Vector3(room1.Right, Random.Range(room1.Bottom, room1.Top), 1);
                        //Debug.Log("selecting from right" + point);
                        validPoint = true;
                    }
                    break;
                //exit out of the top
                case 2:
                    if (room1.Top < TilemapData.Instance.GridHeight - 1)
                    {
                        point = new Vector3(Random.Range(room1.Left, room1.Right), room1.Top, 2);
                        //Debug.Log("selecting from top" + point);
                        validPoint = true;
                    }
                    break;
                //exit out of the bottom
                case 3:
                    if (room1.Bottom > 0)
                    {
                        point = new Vector3(Random.Range(room1.Left, room1.Right), room1.Bottom - 1, 3);
                        //Debug.Log("selecting from bottom" + point);
                        validPoint = true;
                    }
                    break;
                default:
                    validPoint = true;
                    break;
            }
            if(bestInd >= 3)
            {
                break;
            }
            if(!validPoint)
            {
                bestInd++;
            }
        }
        return point;
    }

    private Vector2 GetExitPos(Vector2 origin, int side)
    {
        switch(side)
        {
            //exit from left side
            case 0:
                return new Vector2(origin.x + 1, origin.y);
            //exit from right side
            case 1:
                return new Vector2(origin.x - 1, origin.y);
            //exit from top side
            case 2:
                return new Vector2(origin.x, origin.y - 1);
            //exit from bottom side
            case 3:
                return new Vector2(origin.x, origin.y + 1);
            default:
                return origin;

        }
    }

    public List<Tile> AStar(Vector2 start, Vector2 end, Vector2 exit1, Vector2 exit2)
    {
        //create full list of nodes from the grid
        aStarNodes = new List<AStarNode>();
        for (int i = 0; i < TilemapData.Instance.GridHeight; i++)
        {
            for (int j = 0; j < TilemapData.Instance.GridWidth; j++)
            {
                AStarNode toAdd = new AStarNode();
                toAdd.NodeTile = TilemapData.Instance.TilesList[TilemapData.Instance.GetPosFromCoords(j , i)];
                toAdd.Position = new Vector2(j, i);
                toAdd.HScore = (int)(Mathf.Abs(end.x - toAdd.Position.x) + Mathf.Abs(end.y - toAdd.Position.y));
                toAdd.Parent = null;
                aStarNodes.Add(toAdd);
            }
        }

        //create open and closed lists
        List<AStarNode> openList = new List<AStarNode>();
        List<AStarNode> closedList = new List<AStarNode>();

        //set the starting node
        AStarNode startFrom = aStarNodes.FirstOrDefault(x => x.Position == exit1);
        AStarNode endAt = aStarNodes.FirstOrDefault(x => x.Position == exit2);
        exitPositions = new List<int>() { startFrom.NodeTile.GetGridPosition(), endAt.NodeTile.GetGridPosition() };
        startFrom.GScore = 0;

        //add starting point to the open list
        openList.Add(startFrom);
        //check that the scores are the same for debug
        //Debug.Log("hValue: " + hValue + " , startfrom hScore: " + startFrom.HScore);

        bool foundPath = false;
        do
        {
            //find the tile with the lowest (read: best) score
            int lowestScoreInd = 0;
            for (int i = 0; i < openList.Count; i++)
            {
                if (openList[i].FScore < openList[lowestScoreInd].FScore)
                {
                    lowestScoreInd = i;
                }
            }
            AStarNode currentNode = openList[lowestScoreInd];

            //add it to the closed list and remove it from the open list
            closedList.Add(currentNode);
            openList.Remove(currentNode);

            //check if the destination tile is in the closed list
            for (int i = 0; i < closedList.Count; i++)
            {
                if (closedList[i].Position == end)
                {
                    //the destination was just added so we can go back down the path
                    foundPath = true;
                    //create a list of nodes that make up the shortest path
                    List<Tile> shortestPath = new List<Tile>() { currentNode.NodeTile };
                    AStarNode temp = currentNode;
                    while (temp.Parent != null)
                    {
                        temp = temp.Parent;
                        shortestPath.Add(temp.NodeTile);
                    }
                    return shortestPath;
                }
            }
            if (foundPath)
            {
                break;
            }

            List<AStarNode> neighbours = GetNeighbours(currentNode);
            foreach (AStarNode neighbour in neighbours)
            {
                //if this neighbour is in the closed list then skip over it
                if (closedList.Contains(neighbour))
                {
                    continue;
                }
                //if the open list does not have this neighbour then add it
                if (!openList.Contains(neighbour))
                {
                    neighbour.GScore = currentNode.GScore;
                    neighbour.Parent = currentNode;
                    openList.Add(neighbour);
                }
                else
                {
                    //if the GScore for this neighbour is lower than the current one then a better path has been found
                    //update the neighbour node's GScore
                    int tempScore = currentNode.GScore + 1;
                    //if the neighbour's score is greater than the current one then it is not a better path, skip it
                    if(tempScore >= currentNode.GScore)
                    {
                        continue;
                    }
                    //this node is a better option, record it
                    //cameFrom = currentNode;
                    neighbour.GScore = tempScore;
                    neighbour.FScore = neighbour.GScore + neighbour.HScore;
                }
            }

        } while (openList.Count > 0);

        //pathfinding failed
        Debug.Log("failed to find a path");
        return null;

    }

    public List<AStarNode> GetNeighbours(AStarNode node)
    {
        List<AStarNode> neighbours = new List<AStarNode>();
        if(node.Position.x > 0)
        {
            //add left neighbour
            AStarNode temp = aStarNodes[aStarNodes.IndexOf(node) - 1];
            //Debug.Log(temp.NodeTile.GetTileType());
            if (temp.NodeTile.GetTileType() != Tile.TileType.ROOM)
            {
                //Debug.Log("added left tile");
                neighbours.Add(temp);
            }
        }
        if(node.Position.x < TilemapData.Instance.GridWidth - 1)
        {
            //add right neighbour
            AStarNode temp = aStarNodes[aStarNodes.IndexOf(node) + 1];
            //Debug.Log(temp.NodeTile.GetTileType());
            if (temp.NodeTile.GetTileType() != Tile.TileType.ROOM)
            {
                //Debug.Log("added right tile");
                neighbours.Add(temp);
            }
        }
        if(node.Position.y < TilemapData.Instance.GridHeight - 1)
        {
            //add top neighbour
            AStarNode temp = aStarNodes[aStarNodes.IndexOf(node) + TilemapData.Instance.GridWidth];
            //Debug.Log(temp.NodeTile.GetTileType());
            if (temp.NodeTile.GetTileType() != Tile.TileType.ROOM)
            {
                //Debug.Log("added top tile");
                neighbours.Add(temp);
            }
        }
        if(node.Position.y > 0)
        {
            //add bottom neighbour
            AStarNode temp = aStarNodes[aStarNodes.IndexOf(node) - TilemapData.Instance.GridWidth];
            //Debug.Log(temp.NodeTile.GetTileType());
            if (temp.NodeTile.GetTileType() != Tile.TileType.ROOM)
            {
                //Debug.Log("added top tile");
                neighbours.Add(temp);
            }
        }

        return neighbours;
    }
}
