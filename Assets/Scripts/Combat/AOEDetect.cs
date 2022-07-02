using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AOEDetect : MonoBehaviour
{

    private void OnTriggerEnter(Collider col)
    {
        Debug.Log("CollisionA!");
        if (col.gameObject.tag == "Player" || col.gameObject.tag == "NPC")
        {
            Debug.Log("CollisionA!");
            CombatManager.inst.viableTargets.Add(col.gameObject);
        }
    }

    public void OnCollisionEnter(Collision col)
    {
        if(col.gameObject.tag == "Player" || col.gameObject.tag == "NPC")
        {
            Debug.Log("Collision!");
            CombatManager.inst.viableTargets.Add(col.gameObject);
        }
    }
}
