using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Priority_Queue;

public class TacticsMove : MonoBehaviour
{

    List<Tile> selectableTiles = new List<Tile>();
    GameObject[] tiles; // All tiles as gameobjects in an array
    
    public Stack<Tile> path = new Stack<Tile>(); // Starts at the end --> beginning
    public List<GameObject> openPath = new List<GameObject>(); // Used by movement arrow
    public Tile currentTile; // Starting tile (the character is currently on)

    public bool turn = false; // Is it my turn?

    public bool moving = false; // Character currently moving?
    public bool attacking = false; // Character currently attacking? (Animation)
    public int move; // How many tiles can the player move?
    public float jumpHeight = 2; // How high can the player jump / drop down?
    public float moveSpeed = 2; // Visible move speed across tiles
    public bool doneMoving = false; // Am I done moving?

    public bool canAttack; // Used by AI, if they can attack or not

    Vector3 velocity = new Vector3(); // Visible move speed
    Vector3 heading = new Vector3(); // Direction player is going

    float halfHeight = 0; // How tall (half) the tile is. The player sits this high

    public Tile actualTargetTile;
    public Tile AITargetTile;
    public GameObject visParent;
    Tile tempT;

    //------ JUMPING --------//
    bool fallingDown = false;
    bool jumpingUp = false;
    bool movingEdge = false;
    Vector3 jumpTarget;
    public float jumpVelocity = 4.5f;
    //-----------------------//

    //-------------------------------------------------------------- TURN STUFF -----------------------------------------//

    public void BeginTurn()
    {
        turn = true;
        move = this.GetComponent<Character>().movementSpeed;
        doneMoving = false;
    }

    public void EndTurn()
    {
        TurnManager.doOnce = false;
        turn = false;
    }

    public void Destroy()
    {
        Destroy(this.gameObject);
    }

    //--------------------------------------------------------------------------------------------------------------------//

    protected void Init()
    {
        move = this.GetComponent<Character>().movementSpeed;

        //visParent = new GameObject("VisParent");

        halfHeight = GetComponent<Collider>().bounds.extents.y; // If player height/size can change, this must be ran every frame.

        //TurnManager.AddUnit(this); // Load self (remember if a character has this class they will be counted in the turn order) into the turn order.
        
    }

    public void FillTiles()
    {
        tiles = GameObject.FindGameObjectsWithTag("Tile"); // If tiles are added/removed in-game this must be ran every frame.
    }

    public void GetCurrentTile()
    {
        currentTile = GetTargetTile(gameObject);
        currentTile.current = true;
    }

    public Tile GetTargetTile(GameObject target)
    {
        RaycastHit hit;
        Tile tile = null;

        Vector3 adjust = new Vector3(0, -0.7f, 0);

        if(Physics.Raycast(target.transform.position + adjust, -Vector3.up, out hit, 1))
        {
            tile = hit.collider.GetComponent<Tile>();
        }
        return tile;
    }

    public void ComputeAdjacencyLists(float jumpHeight, Tile target)
    {
        // tiles = GameObject.FindGameObjectsWithTag("Tile"); [Run every frame]

        foreach (GameObject tile in tiles) // Find all walkable tiles for specified unit
        {
            Tile t = tile.GetComponent<Tile>();
            t.FindNeighbors(jumpHeight, target);
        }
    }

    public void FindSelectableTiles() // BFS HAPPENS HERE
    {
        ComputeAdjacencyLists(jumpHeight, null);
        GetCurrentTile();

        // Begin BFS

        Queue<Tile> process = new Queue<Tile>();

        process.Enqueue(currentTile);
        currentTile.visited = true;

        while(process.Count > 0)
        {
            Tile t = process.Dequeue();

            if (t.walkable) // Walkable tiles are pretty much every tile. Look for !walkable to find unwalkable tiles
            {
                selectableTiles.Add(t);
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
    
    public void AstarSelectable(Tile target) // Player uses this
    {
        ComputeAdjacencyLists(jumpHeight, null);
        GetCurrentTile();

        SimplePriorityQueue<Tile> process = new SimplePriorityQueue<Tile>();
        // https://github.com/BlueRaja/High-Speed-Priority-Queue-for-C-Sharp/wiki/Using-the-SimplePriorityQueue

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
        
        if(openList.Count == 0)
        {
            FindNextBest(tempT);
            // What do you do if there is no path to the target tile?
            // Find the closest open tile to that area (Shift target tile)
            // Could be done better.
        }
    }

    protected void FindPath(Tile target) // A* HAPPENS HERE (AI uses this)
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
                AITargetTile = target;
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

        move++; // Because "move--" will get called an extra time.

        Tile next = tile;
        while(next != null)
        {
            path.Push(next);
            next = next.parent;
            move--;
        }
    }

    public void Move()
    {
        if(path.Count > 0)
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
                    moveSpeed = 1.8f; // Jumping too fast will overshoot tile and cause weird pathing bugs
                    Jump(target);
                }
                else
                {
                    moveSpeed = 4;
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

                if (!TurnManager.IsPlayer(this)) // AI ONLY
                {
                    this.GetComponent<NPCMove>().anim.Play("Base Layer.Walk");
                }
                else
                {
                    this.GetComponent<PlayerMove>().anim.Play("Base Layer.Walk");
                }
            }
        }
        else
        {
            //if(move <= 0)
            RemoveSelectableTiles();

            moving = false;
            

            GetCurrentTile(); // If this isn't ran, currentTile will = null
            if (!TurnManager.IsPlayer(this)) // AI ONLY
            {
                doneMoving = true;

                int distance = 9999; // Might be a problem later?
                if (AITargetTile != null && currentTile != null)
                {
                    distance = CombatManager.inst.CalculateDistance(currentTile.gameObject, AITargetTile.gameObject);
                }

                if (this.GetComponent<NPCMove>().AIType == 1) // Melee only
                {
                    if (distance <= 1) // Desired tile was reached (For AI)
                    {
                        canAttack = true;
                    }
                    else
                    {
                        canAttack = false;
                    }
                }
            }
        }
    }

    protected void RemoveSelectableTiles()
    {
        if(currentTile != null)
        {
            currentTile.current = false;
            currentTile = null;
        }

        foreach(Tile tile in selectableTiles)
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
        else if(movingEdge){
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

        if(transform.position.y > targetY) // Jumping Up
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

        if(transform.position.y <= target.y)
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

        if(transform.position.y > target.y)
        {
            jumpingUp = false;
            fallingDown = true;
        }
    }
    void MoveToEdge()
    {
        if(Vector3.Distance(transform.position, jumpTarget) >= 0.05f)
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

    protected Tile FindEndTile(Tile t)
    {
        Stack<Tile> tempPath = new Stack<Tile>();

        Tile next = t.parent;

        while(next != null)
        {
            tempPath.Push(next);
            next = next.parent;
        }

        if(tempPath.Count <= move) // Is this path within range of this units move distance?
        {
            return t.parent; // Yes, it is in range
        }

        Tile endTile = null; // Not in range, go as far as possible.
        for(int i = 0; i <= move; i++) // Be careful with this '<='
        {
            endTile = tempPath.Pop();
        }
        return endTile;
    }

    protected Tile FindLowestF(List<Tile> list)
    {
        Tile lowest = list[0];

        foreach(Tile t in list)
        {
            if(t.f < lowest.f)
            {
                lowest = t;
            }
        }

        list.Remove(lowest);

        return lowest;
    }
}
