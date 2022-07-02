using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ActivityPopup : MonoBehaviour
{
    /// <summary>
    /// Used by any individual pop-up in the activity log.
    /// </summary>

    public GameObject zoomOnClick;

    // Text components
    public TMP_Text attackerTxt;
    public TMP_Text targetTxt;
    public TMP_Text rollTxt;
    public Image attackType;
    public Image numResultBase;

    public void OnClick()
    {
        if (zoomOnClick != null)
        {
            // Move camera to zoom to the zoomOnClick gameobject
            CameraController.inst.followTarget = zoomOnClick.transform;
            StartCoroutine(UnsetCamTarget());
        }
        else
        {
            // Clicked on but nothing to zoom to
        }
    }

    public void Close() // Destroy this event (unsure if will be used)
    {
        Destroy(gameObject);
    }

    IEnumerator UnsetCamTarget()
    {
        yield return new WaitForSeconds(0.1f);

        CameraController.inst.followTarget = null;
    }
}
