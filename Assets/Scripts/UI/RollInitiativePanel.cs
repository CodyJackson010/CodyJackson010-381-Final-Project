using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RollInitiativePanel : MonoBehaviour
{
    public GameObject self;
    public bool startOperation = false;

    public void Update()
    {
        if (startOperation)
        {
            TurnManager.startCombat = true;
            self.SetActive(false);
            //StartCoroutine(Begin());
        }
    }

    public void Flip()
    {
        startOperation = !startOperation;
    }

    IEnumerator Begin()
    {
        yield return new WaitForSeconds(1f);

        TurnManager.startCombat = true;
    }
}
