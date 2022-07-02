using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public bool walkable = true; // Is tile passable? (Deep water, flying units, blocked, etc)
    public bool current = false; // The tile the character currently occupies
    public bool target = false; // The tile the character will head to
    public bool selectable = false; // A tile in range
    public bool debug = false; // Used for debugging

    public float x = 0;
    public float y = 0;

    public List<Tile> adjacencyList = new List<Tile>(); // Neighbors

    //------- BFS -----------------//
    public bool visited = false;
    public Tile parent = null;
    public int distance = 0;
    //-----------------------------//

    //------- A* ------------------//
    public float n = 0; // Natural tile cost
    public float f = 0; // Cost [g + h + n]
    public float g = 0; // Cost [parent --> current tile]
    public float h = 0; // Cost [processed tile --> destination]
    //-----------------------------//

    //-------- Colors -------------//
    Color faintBlue = new Color(0.3f, 0.7f, 0.8f, 0.4f);
    Color debugColor = new Color(0.8f, 0.3f, 0.7f, 0.1f);
    Color faintRed = new Color(0.8f, 0.4f, 0.3f, 0.4f);
    Color whiteColor = new Color(1f, 1f, 1f, 0.1f);
    Color greenColor = new Color(0.1f, 0.8f, 0.1f, 0.4f);
    Color magentaColor = new Color(1f, 0.4f, 0.9f, 0.4f);
    Color blackColor = new Color(0f, 0f, 0f, 0.4f);
    Color orangeColor = new Color(1f, 0.6f, 0.2f, 0.4f);
    Color clearColor = new Color(0f, 0f, 0f, 0f);
    //-----------------------------//


    // Start is called before the first frame update
    void Start()
    {
        x = transform.position.x;
        y = transform.position.y;
    }

    // Update is called once per frame
    void Update()
    {
        if (current)
        {
            GetComponent<Renderer>().material.color = clearColor;
        }
        else if (target)
        {
            GetComponent<Renderer>().material.color = greenColor;
        }
        else if (selectable && n > 0) // Expensive & Selectable
        {
            GetComponent<Renderer>().material.color = faintRed;
        }
        else if (selectable){
            GetComponent<Renderer>().material.color = faintBlue;
        }
        else if(n > 0 && n < 40) // Expensive tile
        {
            GetComponent<Renderer>().material.color = orangeColor;
        }
        else if(!walkable || n >= 50){ // Unwalkable or Blocked tile
            n = 50;
            GetComponent<Renderer>().material.color = blackColor;
        }
        else
        {
            GetComponent<Renderer>().material.color = whiteColor;
        }
    }

    public void Reset()
    {
        adjacencyList.Clear();

        current = false;
        target = false;
        selectable = false;

        visited = false;
        parent = null;
        distance = 0;

        f = 0;
        g = 0;
        h = 0;
    }

    public void SetWeight(float newWeigth)
    {
        this.n = newWeigth;
    }

    public void FindNeighbors(float jumpHeight, Tile target)
    {
        Reset();

        // UP - DOWN
        CheckTile(Vector3.forward, jumpHeight, target);
        CheckTile(-Vector3.forward, jumpHeight, target);

        // RIGHT - LEFT
        CheckTile(Vector3.right, jumpHeight, target);
        CheckTile(-Vector3.right, jumpHeight, target);

        // UP + LEFT/RIGHT (Diagonal)
        CheckTile(Vector3.forward + -Vector3.right, jumpHeight, target);
        CheckTile(Vector3.forward + Vector3.right, jumpHeight, target);

        // DOWN + LEFT/RIGHT (Diagonal)
        CheckTile(-Vector3.forward + -Vector3.right, jumpHeight, target);
        CheckTile(-Vector3.forward + Vector3.right, jumpHeight, target);

    }

    public void FindNeighborsRestricted(float jumpHeight, Tile target, bool ignoreUnwalkable) // Dis-allows Diagonal movement
    {
        Reset();

        if (ignoreUnwalkable)
        {
            // UP - DOWN
            CheckTileUnrestricted(Vector3.forward, jumpHeight, target);
            CheckTileUnrestricted(-Vector3.forward, jumpHeight, target);

            // RIGHT - LEFT
            CheckTileUnrestricted(Vector3.right, jumpHeight, target);
            CheckTileUnrestricted(-Vector3.right, jumpHeight, target);
        }
        else
        {
            // UP - DOWN
            CheckTile(Vector3.forward, jumpHeight, target);
            CheckTile(-Vector3.forward, jumpHeight, target);

            // RIGHT - LEFT
            CheckTile(Vector3.right, jumpHeight, target);
            CheckTile(-Vector3.right, jumpHeight, target);
        }


    }

    public void CheckTile(Vector3 direction, float jumpHeight, Tile target)
    {
        Vector3 halfExtents = new Vector3(0.25f, (1 + jumpHeight) / 2.0f, 0.25f); // Tiles are 1x1x1
        Collider[] colliders = Physics.OverlapBox(transform.position + direction, halfExtents);

        foreach(Collider item in colliders)
        {
            Tile tile = item.GetComponent<Tile>();
            if(tile != null && tile.walkable) // Ignore Non-tiles & Non-walkables
            {
                RaycastHit hit;

                if(!Physics.Raycast(tile.transform.position, Vector3.up, out hit, 1) || (tile == target)) // If there is something there, don't add it, if its open, add it.
                {
                    adjacencyList.Add(tile);
                }
            }
        }
    }

    public void CheckTileUnrestricted(Vector3 direction, float jumpHeight, Tile target) // Include unwalkable tiles
    {
        Vector3 halfExtents = new Vector3(0.25f, (1 + jumpHeight) / 2.0f, 0.25f); // Tiles are 1x1x1
        Collider[] colliders = Physics.OverlapBox(transform.position + direction, halfExtents);

        foreach (Collider item in colliders)
        {
            Tile tile = item.GetComponent<Tile>();
            if (tile != null) // Ignore Non-tiles
            {
                RaycastHit hit;

                if (!Physics.Raycast(tile.transform.position, Vector3.up, out hit, 1) || (tile == target)) // If there is something there, don't add it, if its open, add it.
                {
                    adjacencyList.Add(tile);
                }
            }
        }
    }
}
