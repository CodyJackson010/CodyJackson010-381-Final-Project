using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TacticsMoveUtil : MonoBehaviour
{
    /// <summary>
    /// This class is identical to TacticsMove except it is to be used for utility purposes.
    /// The reason for this is currently TurnManager will load anything with 'TacticsMove'
    /// into the turn order. So this class has been created to utilize the functions
    /// without disrupting the turn order.
    /// </summary>


    List<Tile> selectableTiles = new List<Tile>();
    List<Tile> selectableTiles2 = new List<Tile>();
    GameObject[] tiles; // All tiles as gameobjects in an array

    Stack<Tile> path = new Stack<Tile>(); // Starts at the end --> beginning
    public List<GameObject> openPath = new List<GameObject>(); // Used by movement arrow
    public List<GameObject> measurementPath = new List<GameObject>(); // Used by measurement tool
    public Tile currentTile; // Starting tile (the character is currently on)

    public bool turn = false; // Is it my turn?

    public bool moving = false; // Character currently moving?
    public int move; // How many tiles can the player move?
    public int mtMove; // For measurement tool
    public float jumpHeight = 2; // How high can the player jump / drop down?
    public float moveSpeed = 2; // Visible move speed across tiles

    Vector3 velocity = new Vector3(); // Visible move speed
    Vector3 heading = new Vector3(); // Direction player is going

    float halfHeight = 0; // How tall (half) the tile is. The player sits this high

    public Tile actualTargetTile;
    GameObject visParent;
    GameObject measurementParent;
    Tile tempT;
    Tile measurementTarget;
    public bool alternateSelf = false; // Alternate method of finding self in GetCurrentTile
    public bool measuring = false; // For measurement tool

    //------ JUMPING --------//
    bool fallingDown = false;
    bool jumpingUp = false;
    bool movingEdge = false;
    Vector3 jumpTarget;
    public float jumpVelocity = 4.5f;
    //-----------------------//

    /*
     * IMPORTANT!
     * Movement vis & PlayerMove utilize the same function inside different scripts to visualize selectable (blue) tiles.
     * They base the size of off their own "move" varaibles. 
     * THEY MUST BE THE SAME TO CORRECTLY VISUALIZE WHERE THE PLAYER CAN GO!
     */ 
    

    protected void Init()
    {
        tiles = GameObject.FindGameObjectsWithTag("Tile"); // If tiles are added/removed in-game this must be ran every frame.

        halfHeight = GetComponent<Collider>().bounds.extents.y; // If player height/size can change, this must be ran every frame.

    }

    protected void Awake()
    {
        visParent = new GameObject("VisParent");
        measurementParent = new GameObject("MeasurementParent");

        visParent.transform.parent = this.transform;
        measurementParent.transform.parent = this.transform;

    }

    public void FillPoints()
    {
        Tile[] nTiles = path.ToArray();

        openPath.Clear();

        for (int i = 0; i < path.Count; i++)
        {
            GameObject newPoint = new GameObject("VisPath");
            newPoint.transform.position = new Vector3(nTiles[i].transform.position.x, nTiles[i].transform.position.y + 0.5f, nTiles[i].transform.position.z);
            //newPoint.transform.Rotate(0, 0, 90); // Due to linerender using Z axis
            newPoint.transform.parent = visParent.transform;
            //newPoint.transform.SetParent(visParent.transform);

            openPath.Add(newPoint);
        }
    }

    public void ClearPoints()
    {
        foreach (GameObject go in openPath)
        {
            Destroy(go);
        }
    }

    public void GetCurrentTile()
    {
        if (alternateSelf || measuring)
        {

        }
        else
        {
            currentTile = GetTargetTile(gameObject);
            currentTile.current = true;
        }
    }

    public Tile GetTargetTile(GameObject target)
    {
        RaycastHit hit;
        Tile tile = null;

        if (Physics.Raycast(target.transform.position, -Vector3.up, out hit, 1))
        {
            tile = hit.collider.GetComponent<Tile>();
        }
        return tile;
    }

    public void ComputeAdjacencyLists(float jumpHeight, Tile target)
    {

        foreach (GameObject tile in tiles) // Find all walkable tiles for specified unit
        {
            Tile t = tile.GetComponent<Tile>();
            t.FindNeighbors(jumpHeight, target);
        }
    }

    public void RefreshTilesList()
    {
        tiles = GameObject.FindGameObjectsWithTag("Tile"); //[Run every frame]
    }

    public void FindSelectableTiles() // BFS HAPPENS HERE
    {
        ClearPoints();
        ComputeAdjacencyLists(jumpHeight, null);
        GetCurrentTile();

        // Begin BFS

        Queue<Tile> process = new Queue<Tile>();

        process.Enqueue(currentTile);
        currentTile.visited = true;
        //currentTile.parent = ?? leave as null

        while (process.Count > 0)
        {
            Tile t = process.Dequeue();

            if (t.walkable)
            {
                selectableTiles.Add(t);
                selectableTiles2.Add(t);
                t.selectable = true;

                if (t.distance < move) // Limit movement based on personal move distiance
                {
                    foreach (Tile tile in t.adjacencyList)
                    {
                        if (!tile.visited)
                        {
                            tile.parent = t;
                            tile.visited = true;
                            tile.distance = 1 + t.distance;
                            process.Enqueue(tile);
                        }
                    }
                }
            }
        }
    }

    public IEnumerator FST_Enum() // FindSelectableTiles but limited to running at the end of every frame. Decreases FPS drop
    {

        yield return new WaitForEndOfFrame();
        //yield return new WaitForSeconds(0.5f);

        ClearPoints();
        ComputeAdjacencyLists(jumpHeight, null);
        GetCurrentTile();

        // Begin BFS

        Queue<Tile> process = new Queue<Tile>();

        process.Enqueue(currentTile);
        currentTile.visited = true;
        //currentTile.parent = ?? leave as null

        while (process.Count > 0)
        {
            Tile t = process.Dequeue();

            if (t.walkable)
            {
                selectableTiles.Add(t);
                //selectableTiles2.Add(t);
                t.selectable = true;

                if (t.distance < move) // Limit movement based on personal move distiance
                {
                    foreach (Tile tile in t.adjacencyList)
                    {
                        if (!tile.visited)
                        {
                            tile.parent = t;
                            tile.visited = true;
                            tile.distance = 1 + t.distance;
                            process.Enqueue(tile);
                        }
                    }
                }
            }
        }
    }

    public void QuickRefresh()
    {
        /*
        Queue<Tile> process = new Queue<Tile>();

        process.Enqueue(currentTile);
        currentTile.visited = true;
        //currentTile.parent = ?? leave as null

        while (process.Count > 0)
        {
            Tile t = process.Dequeue();

            if (t.walkable)
            {
                if (t.distance < move) // Limit movement based on personal move distiance
                {
                    foreach (Tile tile in t.adjacencyList)
                    {
                        t.selectable = true;
                    }
                }
            }
        }
        */

        //Debug.Log(selectableTiles2.Count);

        foreach (Tile t in selectableTiles2)
        {
            if (t.walkable)
            {
                if (t.distance < move) // Limit movement based on personal move distiance
                {
                    foreach (Tile tile in t.adjacencyList)
                    {
                        t.selectable = true;
                    }
                }
            }
        }
    }

    public void AstarSelectable(Tile target)
    {
        ComputeAdjacencyLists(jumpHeight, null);
        GetCurrentTile();

        List<Tile> openList = new List<Tile>(); // Any tile not processed
        List<Tile> closedList = new List<Tile>(); // Every tile that has been processed
        // Finishes when target tile is in closed list

        openList.Add(currentTile);
        //currentTile.parent = ?? leave as null
        currentTile.h = Vector3.Distance(currentTile.transform.position, target.transform.position); // Use SqrMagnitude here for efficiency
        currentTile.f = currentTile.h + currentTile.n;

        while (openList.Count > 0) // If 0 is hit before a target path is found, there is no path
        {
            Tile t = FindLowestF(openList); // Not necessary if using priority queue
            tempT = t;
            closedList.Add(t); // Best path found, don't process again

            if (t == target) // True = Success
            {
                MoveToTile(target);
                return;
            }

            foreach (Tile tile in t.adjacencyList)
            {
                if (closedList.Contains(tile))
                {
                    // Do nothing, already processed
                }
                else if (openList.Contains(tile)) // Two paths found to tile, but which one is better?
                {
                    float tempG = t.g + Vector3.Distance(tile.transform.position, t.transform.position);

                    if (tempG < tile.g)
                    {
                        tile.parent = t;

                        tile.g = tempG;
                        tile.f = tile.g + tile.h + tile.n;
                    }
                }
                else // First time node is seen
                {
                    tile.parent = t;

                    tile.g = t.g + Vector3.Distance(tile.transform.position, t.transform.position);
                    tile.h = Vector3.Distance(tile.transform.position, target.transform.position);
                    tile.f = tile.g + tile.h + tile.n;

                    openList.Add(tile); // Proccessed
                }
            }
        }

        if (openList.Count == 0)
        {
            FindNextBest(tempT);
            // What do you do if there is no path to the target tile?
            // Find the closest open tile to that area (Shift target tile)
            // Could be done better.
        }
    }


    protected void FindNextBest(Tile t)
    {
        actualTargetTile = FindEndTile(t);
        MoveToTile(actualTargetTile);
    }


    public void MoveToTile(Tile tile)
    {
        path.Clear();
        tile.target = true;
        moving = true;

        Tile next = tile;
        while (next != null)
        {
            path.Push(next);
            next = next.parent;
        }

        FillPoints();
    }

    public void Move()
    {
        if (path.Count > 0)
        {
            Tile t = path.Peek();
            Vector3 target = t.transform.position;
            //halfHeight = GetComponent<Collider>().bounds.extents.y; [Run every frame?]
            target.y += halfHeight + t.GetComponent<Collider>().bounds.extents.y; // Calculate the unit's position on top of the target tile

            if (Vector3.Distance(transform.position, target) >= 0.05f)
            {
                bool jump = transform.position.y != target.y; // Do we need to jump?

                if (jump)
                {
                    Jump(target);
                }
                else
                {
                    CalculateHeading(target);
                    SetHorizontalVelocity();
                }

                // Locomotion
                transform.forward = heading;
                transform.position += velocity * Time.deltaTime;
            }
            else
            {
                // Tile center reached
                transform.position = target;
                path.Pop();
            }
        }
        else
        {
            RemoveSelectableTiles();
            moving = false;
        }
    }

    protected void RemoveSelectableTiles()
    {
        if (currentTile != null)
        {
            currentTile.current = false;
            currentTile = null;
        }

        foreach (Tile tile in selectableTiles)
        {
            tile.Reset();
        }

        selectableTiles.Clear();
    }

    void CalculateHeading(Vector3 target)
    {
        heading = target - transform.position;
        heading.Normalize();
    }

    void SetHorizontalVelocity()
    {
        velocity = heading * moveSpeed;
    }

    void Jump(Vector3 target)
    {
        if (fallingDown)
        {
            FallDownward(target);
        }
        else if (jumpingUp)
        {
            JumpUpward(target);
        }
        else if (movingEdge)
        {
            MoveToEdge();
        }
        else
        {
            PrepareJump(target);
        }
    }

    void PrepareJump(Vector3 target)
    {
        float targetY = target.y;

        target.y = transform.position.y;

        CalculateHeading(target);

        if (transform.position.y > targetY) // Jumping Up
        {
            fallingDown = false;
            jumpingUp = false;
            movingEdge = true;

            jumpTarget = transform.position + (target - transform.position) / 2.0f; // Find halfway point
        }
        else // Jumping Down
        {
            fallingDown = false;
            jumpingUp = true;
            movingEdge = false;

            velocity = heading * moveSpeed / 3.0f; // Change the float value if needed

            float difference = targetY - transform.position.y;

            velocity.y = jumpVelocity * (0.5f + difference / 2.0f);
        }
    }
    void FallDownward(Vector3 target)
    {
        velocity += Physics.gravity * Time.deltaTime;

        if (transform.position.y <= target.y)
        {
            fallingDown = false;
            jumpingUp = false;
            movingEdge = false;

            Vector3 p = transform.position;
            p.y = target.y;
            transform.position = p;

            velocity = new Vector3();
        }
    }
    void JumpUpward(Vector3 target)
    {
        velocity += Physics.gravity * Time.deltaTime;

        if (transform.position.y > target.y)
        {
            jumpingUp = false;
            fallingDown = true;
        }
    }
    void MoveToEdge()
    {
        if (Vector3.Distance(transform.position, jumpTarget) >= 0.05f)
        {
            SetHorizontalVelocity();
        }
        else
        {
            movingEdge = false;
            fallingDown = true;

            velocity /= 5.0f; // Falling Speed (larger # = slower)
            velocity.y = 1.5f;
        }
    }

    public void BeginTurn()
    {
        DebugCleanTiles();
        turn = true;
    }

    public void EndTurn()
    {
        turn = false;
    }

    protected Tile FindEndTile(Tile t)
    {
        Stack<Tile> tempPath = new Stack<Tile>();

        Tile next = t.parent;

        while (next != null)
        {
            tempPath.Push(next);
            next = next.parent;
        }

        if (tempPath.Count <= move) // Is this path within range of this units move distance?
        {
            return t.parent; // Yes, it is in range
        }

        Tile endTile = null; // Not in range, go as far as possible.
        for (int i = 0; i <= move; i++) // Be careful with this '<='
        {
            endTile = tempPath.Pop();
        }
        return endTile;
    }

    protected void FindPath(Tile target) // A* HAPPENS HERE
    {
        ComputeAdjacencyLists(jumpHeight, target);
        GetCurrentTile();

        List<Tile> openList = new List<Tile>(); // Any tile not processed
        List<Tile> closedList = new List<Tile>(); // Every tile that has been processed
        // Finishes when target tile is in closed list

        openList.Add(currentTile);
        //currentTile.parent = ??
        currentTile.h = Vector3.Distance(currentTile.transform.position, target.transform.position); // Use SqrMagnitude here for efficiency
        currentTile.f = currentTile.h + currentTile.n;

        while (openList.Count > 0) // If 0 is hit before a target path is found, there is no path
        {
            Tile t = FindLowestF(openList); // Not necessary if using priority queue
            tempT = t;
            closedList.Add(t); // Best path found, don't process again

            if (t == target) // True = Success | Goal is to end up ADJACENT to the target, not on it
            {
                actualTargetTile = FindEndTile(t);
                MoveToTile(actualTargetTile);
                return;
            }

            foreach (Tile tile in t.adjacencyList)
            {
                if (closedList.Contains(tile))
                {
                    // Do nothing, already processed
                }
                else if (openList.Contains(tile)) // Two paths found to tile, but which one is better?
                {
                    float tempG = t.g + Vector3.Distance(tile.transform.position, t.transform.position);

                    if (tempG < tile.g)
                    {
                        tile.parent = t;

                        tile.g = tempG;
                        tile.f = tile.g + tile.h + tile.n;
                    }
                }
                else // First time node is seen
                {
                    tile.parent = t;

                    tile.g = t.g + Vector3.Distance(tile.transform.position, t.transform.position);
                    tile.h = Vector3.Distance(tile.transform.position, target.transform.position);
                    tile.f = tile.g + tile.h + tile.n;

                    openList.Add(tile); // Proccessed
                }
            }
        }

        if (openList.Count == 0)
        {
            FindNextBest(tempT);
            // What do you do if there is no path to the target tile?
            // Find the closest open tile to that area (Shift target tile)
            // Could be done better.
        }
    }

    protected Tile FindLowestF(List<Tile> list)
    {
        Tile lowest = list[0];

        foreach (Tile t in list)
        {
            if (t.f < lowest.f)
            {
                lowest = t;
            }
        }

        list.Remove(lowest);

        return lowest;
    }

    public void DebugCleanTiles() // Removes debug texture from all available tiles in list.
    {
        /*
        foreach(Tile t in selectableTiles)
        {
            t.debug = false;
        }
        */

        foreach (GameObject go in tiles)
        {
            go.GetComponent<Tile>().debug = false;
        }
    }

    // ----------------------------- RESTRICTED PATHFINDING (NO DIAGONALS) ------------------------------ //

    public void CALRestricted(float jumpHeight, Tile target)
    {
        tiles = GameObject.FindGameObjectsWithTag("Tile"); //[Run every frame]

        foreach (GameObject tile in tiles) // Find all walkable tiles for specified unit
        {
            Tile t = tile.GetComponent<Tile>();
            t.FindNeighborsRestricted(jumpHeight, target, true);
        }
    }

    public IEnumerable FST_Enum_Restricted() // FindSelectableTiles but limited to running at the end of every frame. Decreases FPS drop
    {
        yield return new WaitForEndOfFrame();

        ClearMeasurementPoints();
        CALRestricted(jumpHeight, null);
        GetCurrentTile();
        
        // Begin BFS

        Queue<Tile> process = new Queue<Tile>();

        process.Enqueue(currentTile);
        currentTile.visited = true;
        //currentTile.parent = ?? leave as null

        while (process.Count > 0)
        {
            Tile t = process.Dequeue();

            //if (t.walkable)
            //{
                selectableTiles.Add(t);
                t.selectable = true;

                if (t.distance < mtMove) // Limit movement based on personal move distiance
                {
                    foreach (Tile tile in t.adjacencyList)
                    {
                        if (!tile.visited)
                        {
                            tile.parent = t;
                            tile.visited = true;
                            tile.distance = 1 + t.distance;
                            process.Enqueue(tile);
                        }
                    }
                }
            //}
        }
    }

    public void AstarSelectableRestricted(Tile target, Tile cTile) // Used by measurement tool
    {
        CALRestricted(jumpHeight, null);
        currentTile = cTile;

        List<Tile> openList = new List<Tile>(); // Any tile not processed
        List<Tile> closedList = new List<Tile>(); // Every tile that has been processed
        // Finishes when target tile is in closed list

        openList.Add(currentTile);
        //currentTile.parent = ?? leave as null
        if(currentTile == null)
        {
            Debug.Log("its null");
        }
        currentTile.h = Vector3.Distance(currentTile.transform.position, target.transform.position); // Use SqrMagnitude here for efficiency
        currentTile.f = currentTile.h + currentTile.n;

        while (openList.Count > 0) // If 0 is hit before a target path is found, there is no path
        {
            Tile t = FindLowestF(openList); // Not necessary if using priority queue
            tempT = t;
            closedList.Add(t); // Best path found, don't process again

            if (t == target) // True = Success
            {
                measurementTarget = target;
                MoveToTile_ALT(target);
                return;
            }

            foreach (Tile tile in t.adjacencyList)
            {
                if (closedList.Contains(tile))
                {
                    // Do nothing, already processed
                }
                else if (openList.Contains(tile)) // Two paths found to tile, but which one is better?
                {
                    float tempG = t.g + Vector3.Distance(tile.transform.position, t.transform.position);

                    if (tempG < tile.g)
                    {
                        tile.parent = t;

                        tile.g = tempG;
                        tile.f = tile.g + tile.h + tile.n;
                    }
                }
                else // First time node is seen
                {
                    tile.parent = t;

                    tile.g = t.g + Vector3.Distance(tile.transform.position, t.transform.position);
                    tile.h = Vector3.Distance(tile.transform.position, target.transform.position);
                    tile.f = tile.g + tile.h + tile.n;

                    openList.Add(tile); // Proccessed
                }
            }
        }

        if (openList.Count == 0)
        {
            FindNextBest(tempT);
            // What do you do if there is no path to the target tile?
            // Find the closest open tile to that area (Shift target tile)
            // Could be done better.
        }
    }

    public void MeasurementPoints()
    {
        GameObject startPoint = new GameObject("StartPoint");
        startPoint.transform.position = new Vector3(currentTile.transform.position.x, currentTile.transform.position.y + 0.5f, currentTile.transform.position.z);

        startPoint.transform.parent = this.transform;

        measurementPath.Add(startPoint);


        GameObject endPoint = new GameObject("EndPoint");
        endPoint.transform.position = new Vector3(measurementTarget.transform.position.x, measurementTarget.transform.position.y + 0.5f, measurementTarget.transform.position.z);

        endPoint.transform.parent = this.transform;

        measurementPath.Add(endPoint);
    }

    public void ClearMeasurementPoints()
    {
        foreach (GameObject go in measurementPath)
        {
            Destroy(go);
        }

        measurementPath.Clear();
    }

    public void MoveToTile_ALT(Tile tile)
    {
        path.Clear();
        tile.target = true;
        moving = true;

        Tile next = tile;
        while (next != null)
        {
            path.Push(next);
            next = next.parent;
        }

        MeasurementPoints();
    }

    // ----------------------------------------------------------------------------------------------------------------------- //

}

