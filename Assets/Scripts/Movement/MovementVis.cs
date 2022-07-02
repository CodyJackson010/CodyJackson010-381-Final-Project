using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementVis : TacticsMoveUtil
{
    /// <summary>
    /// This script is responsible for drawing the pathing line for the player.
    /// This should reset every time a new target tile is found, and only
    /// display on a player's turn.
    /// </summary>


    public LineRenderer line;
    Tile target;
    Tile startTile;
    Tile newTile = null;
    public bool showGuide = false; // Show guidance arrow
    GameObject[] points = new GameObject[35]; // Contains array of gameobjects that linerender uses, shouldn't ever go that high.
    public bool debug = true;
    public bool success = false;

    public GameObject measurementTool;

    void Update()
    {
        if (TurnManager.startCombat == true && debug) // Wait for combat to start
        {
            if (TurnManager.IsPlayer(TurnManager.currentUnit) && showGuide && !measurementTool.GetComponent<MeasurementTool>().showLine)
            { // Is the current unit a player? Should we be showing the guide? Is the measurement tool active?

                RefreshTilesList();
                PrimaryFunction();

                if (TurnManager.currentUnit.moving)
                {
                    line.positionCount = 0;
                    success = false;
                }
            }
            else // Delete the line
            {
                line.positionCount = 0;
                success = false;
            }
        }

        if (Input.GetKeyUp(KeyCode.Escape))
        {
            line.positionCount = 0;
            debug = false;
            showGuide = false;
        }

        if (Input.GetKeyDown(KeyCode.V))
        {
            Flip();
        }
    }

    public void CheckGo()
    {
        showGuide = !showGuide;
    }

    public void Flip()
    {
        if (TurnManager.IsPlayer(TurnManager.currentUnit))
        {
            line.positionCount = 0;
            debug = !debug;
            showGuide = !showGuide;
        }
    }

    public void DrawPath()
    {
        line.positionCount = 0; // Remove previous line

        for(int i = 0; i < points.Length; i++) // Clear points
        {
            points[i] = null;
        }

        points = openPath.ToArray();

        line = GetComponent<LineRenderer>();
        line.useWorldSpace = false;
        //line.alignment = LineAlignment.TransformZ;

        int n = points.Length;

        line.positionCount = n;

        for (int i = 0; i < n - 1; i++)
        {
            line.SetPosition(i, points[i].transform.position);
            line.SetPosition(i + 1, points[i + 1].transform.position);
        }

        success = true;
    }

    void GetTargetFromMouse()
    {
        if (Input.GetMouseButtonUp(1) && TurnManager.doOnce) // 0 = Left-click | 1 = Right-click
        {
            if(TurnManager.currentUnit.move == 0) // No more moves, stop drawing the line.
            {
                TurnManager.doOnce = false;
                move = 5;
            }
            else
            {
                line.positionCount = 0; // Player still has more moves left, just remove the line.
            }
        }
        else if(TurnManager.doOnce){
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.tag == "Tile")
                {
                    Tile t = hit.collider.GetComponent<Tile>();
                    if (t.selectable)
                    {
                        if (target == t){
                            // do nothing
                        }
                        else{
                            target = t;
                            actualTargetTile = t;
                            newTile = target;
                        }
                        
                    }
                }
            }
        }
        else
        {
            //
        }
    }

    void PrimaryFunction()
    {

        startTile = TurnManager.currentUnit.currentTile;
        move = TurnManager.currentUnit.move;

        alternateSelf = true;
        target = null;
        currentTile = startTile;
        
        if (currentTile != null)
        {
            currentTile.current = true;

            //StartCoroutine(FST_Enum());
            //FindSelectableTiles();
            GetTargetFromMouse();
            if (target != null)
            {
                AstarSelectable(target);
                DrawPath();
                //StartCoroutine(FST_Enum());
                //QuickRefresh();
                FindSelectableTiles(); // If this doesn't get called again then the selectable tiles won't be shown while the arrow is moving.
                target.target = true;
                //alternateSelf = false;
            }
        }
        
    }
}