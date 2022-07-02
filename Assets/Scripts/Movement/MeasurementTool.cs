using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MeasurementTool : TacticsMoveUtil
{
    public bool showLine = false;
    public LineRenderer lr;
    public Tile endTile;
    public Tile startTile;

    public int horizontalDist;
    public int verticalDist;
    public int upwardsDist;
    public int longest;
    public GameObject[] pointsPath;
    public string displayDist;
    public TMP_Text displayText;
    public bool alternateUse = false; // For ranged/spell attacks

    public GameObject movementVis;
    public GameObject distanceTextWindow;

    public Transform startDiff;
    public Vector3 endRel;

    public Vector3 shiftD = new Vector3(0, 0, 0);


    // Update is called once per frame
    void Update()
    {
        if (showLine && !movementVis.GetComponent<MovementVis>().showGuide) // Don't want to show player path and measurement tool at the same time
        {
            UIManager.inst.escapeLevel = 1;
            measuring = true;
            if (!alternateUse)
            {
                distanceTextWindow.SetActive(true);
            }

            DrawVisLine(shiftD);
        }
        else
        {
            UIManager.inst.escapeLevel = 0;
            measuring = false;
            distanceTextWindow.SetActive(false);
            lr.positionCount = 0;
        }

        if (Input.GetKeyDown(KeyCode.Escape) && !alternateUse)
        {
            showLine = false;
            UIManager.inst.escapeLevel = 0;
        }

        if (Input.GetKeyDown(KeyCode.M) && !alternateUse)
        {
            Flip();
            UIManager.inst.escapeLevel = 0;
        }
    }

    public void Flip()
    {
        showLine = !showLine;
    }

    public void DrawVisLine(Vector3 shift, Tile start = null, Tile end = null)
    {
        UIManager.inst.escapeLevel = 1;
        if (start == null)
        {
            start = TurnManager.currentUnit.currentTile;
        }

        if(end == null)
        {
            GetTargetFromMouse();
            end = endTile;
        }

        if (end == null)
        {
            return; // Try again
        }

        lr.positionCount = 0; // Remove previous line

        Vector3 adjust = new Vector3(0, 0.5f, 0);

        lr.positionCount = 2;
        lr.SetPosition(0, start.transform.position + adjust);
        lr.SetPosition(1, end.transform.position + adjust + shift);

        // Calculate which distance is longer (because pythagorean math doesn't exist in DnD)

        startDiff = start.transform;
        Vector3 endRel = startDiff.InverseTransformPoint(end.transform.position);

        verticalDist = (int)endRel.x;
        upwardsDist = (int)endRel.y;
        horizontalDist = (int)endRel.z;

        if (verticalDist < 0) // If any of them are negative, convert it
        {
            verticalDist *= -1;
        }
        if (horizontalDist < 0)
        {
            horizontalDist *= -1;
        }
        if (upwardsDist < 0)
        {
            upwardsDist *= -1;
        }

        // Which ever is larger is the one we want

        if(upwardsDist != 0)
        {
            upwardsDist = upwardsDist / 20; // Why is upwardsDist multiplied by 20? I don't know, but this should fix it.
        }

        longest = Mathf.Max(verticalDist, horizontalDist, upwardsDist);

        if (alternateUse)
        {

        }
        else
        {
            displayDist = longest * 5 + "ft (" + longest + ")";
            displayText.SetText(displayDist); // Set the text
        }

        //Debug.Log("x[" + verticalDist + "] y[" + upwardsDist + "] z[" + horizontalDist + "]");
    }

    public void GetTargetFromMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.tag == "Tile" || hit.collider.tag == "Player" || hit.collider.tag == "NPC")
            {
                if (hit.collider.tag == "Player" || hit.collider.tag == "NPC")
                {

                    if (alternateUse)
                    {
                        lr.material.color = Color.red;
                    }
                    else
                    {
                        lr.material.color = Color.blue;
                    }
                    Tile t = hit.collider.GetComponent<TacticsMove>().GetTargetTile(hit.collider.gameObject);

                    endTile = t;
                }
                else
                {
                    Tile t = hit.collider.GetComponent<Tile>();

                    endTile = t;
                }

            }
        }
    }

    public void Clear()
    {
        UIManager.inst.escapeLevel = 0;
        measuring = false;
        distanceTextWindow.SetActive(false);
        lr.positionCount = 0;
    }
}
