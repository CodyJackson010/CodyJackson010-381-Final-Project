using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : TacticsMove
{
    Tile target;
    public Animator anim;

    // Start is called before the first frame update
    void Start()
    {
        Init();
    }

    // Update is called once per frame
    void Update()
    {
        Debug.DrawRay(transform.position, transform.forward);
        HealthCheck();
        CheckFlanking();

        if (!turn)
        {
            return;
        }

        if (!moving)
        {
            DoMovement();
        }
        else
        {
            Move();
            RemoveSelectableTiles();
            UIManager.inst.UpdateHUD();
        }
    }

    void CheckMouse()
    {
        if (Input.GetMouseButtonUp(1)) // 0 = Left-click | 1 - Right-click
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            RaycastHit hit;
            if(Physics.Raycast(ray, out hit))
            {
                if(hit.collider.tag == "Tile")
                {
                    Tile t = hit.collider.GetComponent<Tile>();

                    if (t.selectable)
                    {
                        //MoveToTile(t);
                        target = t;
                        
                    }
                }
            }
        }
    }

    void DoMovement() // Give the player the option to move
    {
        if (!CombatManager.inst.measurementRef.GetComponent<MeasurementTool>().measuring) // Don't allow movement while measuring
        {
            target = null;
            FindSelectableTiles();
            CheckMouse();
            if (target != null)
            {
                AstarSelectable(target);
            }
        }
    }

    public void HealthCheck()
    {
        if (this.GetComponent<Character>().hitPoints <= 0) // DEATH
        {
            TurnManager.RemoveUnit(this.GetComponent<TacticsMove>()); // Remove self from turnmanager
        }
    }

    public void CheckFlanking()
    {
        bool temp = false;
        string desiredtag = "NPC";
        RaycastHit hit;
        GameObject flank1 = null;
        GameObject flank2 = null;

        if (Physics.Raycast(this.transform.position, Vector3.forward, out hit, 1.5f))
        {
            if (hit.collider.tag == desiredtag)
            {
                flank1 = hit.collider.gameObject;
                if (Physics.Raycast(this.transform.position, -Vector3.forward, out hit, 1.5f))
                {
                    if (hit.collider.tag == desiredtag)
                    {
                        temp = true;
                        flank2 = hit.collider.gameObject;
                    }
                }
            }
        }

        if (Physics.Raycast(this.transform.position, Vector3.right, out hit, 1.5f))
        {
            if (hit.collider.tag == desiredtag)
            {
                flank1 = hit.collider.gameObject;
                if (Physics.Raycast(this.transform.position, -Vector3.right, out hit, 1.5f))
                {
                    if (hit.collider.tag == desiredtag)
                    {
                        temp = true;
                        flank2 = hit.collider.gameObject;
                    }
                }
            }
        }

        if (Physics.Raycast(this.transform.position, Vector3.forward + Vector3.right, out hit, 1.5f))
        {
            if (hit.collider.tag == desiredtag)
            {
                flank1 = hit.collider.gameObject;
                if (Physics.Raycast(this.transform.position, -Vector3.forward + -Vector3.right, out hit, 1.5f))
                {
                    if (hit.collider.tag == desiredtag)
                    {
                        temp = true;
                        flank2 = hit.collider.gameObject;
                    }
                }
            }
        }

        if (Physics.Raycast(this.transform.position, -Vector3.forward + Vector3.right, out hit, 1.5f))
        {
            if (hit.collider.tag == desiredtag)
            {
                flank1 = hit.collider.gameObject;
                if (Physics.Raycast(this.transform.position, Vector3.forward + -Vector3.right, out hit, 1.5f))
                {
                    if (hit.collider.tag == desiredtag)
                    {
                        temp = true;
                        flank2 = hit.collider.gameObject;
                    }
                }
            }
        }


        if (temp) // Being flanked
        {
            this.GetComponent<Character>().vantageStatus = 2;
            flank1.GetComponent<Character>().vantageStatus = 1; // Give advantage to flankers
            flank2.GetComponent<Character>().vantageStatus = 1; // Give advantage to flankers
        }
        else // Not being flanked
        {
            this.GetComponent<Character>().vantageStatus = 0;
        }

    }
}
