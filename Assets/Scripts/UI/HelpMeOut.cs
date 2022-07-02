using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelpMeOut : MonoBehaviour
{
    public bool active = false;

    public GameObject main;

    public static HelpMeOut inst;
    public void Awake()
    {
        inst = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyUp(KeyCode.Escape))
        {
            active = false;
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            Flip();
        }

        if (active)
        {
            main.SetActive(true);
        }
        else
        {
            main.SetActive(false);
        }
    }

    public void Flip()
    {
        if (TurnManager.IsPlayer(TurnManager.currentUnit))
        {
            active = !active;
        }
    }
}
