using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.EventSystems;

public class UITestAlert : MonoBehaviour
{
    public void OnClick()
    {
        int randomNumber = Random.Range(1, 20);
        ActivityLog.inst.NewAlert("Test attacker", "Test target", randomNumber, "obsolete", null);
    }
}
