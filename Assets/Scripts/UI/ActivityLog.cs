using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActivityLog : MonoBehaviour
{
    /// <summary>
    /// Handles events in the activity log.
    /// </summary>

    public GameObject alertPref;
    public GameObject activityLogBase; // Which thing is the activity log (where there VerticalLayoutGroup is)

    // Colors
    Color c_criticalFail = new Color(0.7f, 0f, 0f); // Deep red
    Color c_criticalSuccess = new Color(0.1f, 1f, 0f); // Bright green
    Color c_success = new Color(0.1f, 0.8f, 0f); // Green
    Color c_failure = new Color(0.8f, 0.1f, 0.1f); // Red
    Color c_debug = new Color(1f, 0.5f, 0.9f); // Debug color (pink)

    public static ActivityLog inst;
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
        
    }

    /// <summary>
    /// This function creates a new alert given certain parameters. The parameters have the option of being blank. Thus displaying blank.
    /// </summary>
    /// <param name="attackerText"> The name of the attacker. </param>
    /// <param name="targetText"> The target of the attack. </param>
    /// <param name="rollText"> The resulting roll made by the attacker. </param>
    /// <param name="attackIcon"> The Icon displayed between attacker name and target name (depends on type of attack). </param>
    /// <param name="target"> The GameObject of the target so the camera can focus on it if need be. </param>
    public void NewAlert(string attackerText = "", string targetText = "", int rollText = 0, string attackIcon = "", GameObject target = null, string tooltip = "")
    {
        // Create the alert
        GameObject alertGo = (GameObject)Instantiate(alertPref);

        alertGo.transform.SetParent(activityLogBase.transform, true); // 'false' is for worldPositionStays

        // Set the text
        alertGo.GetComponent<ActivityPopup>().attackerTxt.text = attackerText;
        alertGo.GetComponent<ActivityPopup>().targetTxt.text = targetText;
        alertGo.GetComponent<ActivityPopup>().rollTxt.text = rollText.ToString();

        // Set the Dice roll # backing color

        // Get the dice roll result from combat manager or whatever. Implement this and remove the debug color code later.
        if(CombatManager.inst.outcome == true){ // Success (bool)
            alertGo.GetComponent<ActivityPopup>().numResultBase.GetComponent<Image>().color = c_success;
        }
        else if(CombatManager.inst.outcome == false){ // Failure (bool)
            alertGo.GetComponent<ActivityPopup>().numResultBase.GetComponent<Image>().color = c_failure;
        }
        else if(CombatManager.inst.attackRoll == 20 && CombatManager.inst.outcome == true) // Critical success
        {
            alertGo.GetComponent<ActivityPopup>().numResultBase.GetComponent<Image>().color = c_criticalSuccess;
        }
        else if (CombatManager.inst.attackRoll == 20 && CombatManager.inst.outcome == false) // Critical success but a miss still (god help you)
        {
            alertGo.GetComponent<ActivityPopup>().numResultBase.GetComponent<Image>().color = c_failure;
        }
        else if(CombatManager.inst.attackRoll == 1){ // Critical failure
            alertGo.GetComponent<ActivityPopup>().numResultBase.GetComponent<Image>().color = c_criticalFail;
        }
        else{ // Debug???
            Debug.LogError("Failure in setting dice outcome color in ActivityLog.cs");
            alertGo.GetComponent<ActivityPopup>().numResultBase.GetComponent<Image>().color = c_debug;
        }
        //alertGo.GetComponent<ActivityPopup>().numResultBase.GetComponent<Image>().color = c_success;

        // Set the attack type Icon
        Sprite iconTexture = Resources.Load<Sprite>("UI/Icons/" + attackIcon);
        if (iconTexture == null) // Try the stylized icons
        {
            Debug.LogError("Attack Type icon in ActivityLog.cs is null");
        }
        else
        {
            alertGo.GetComponent<ActivityPopup>().attackType.GetComponent<Image>().sprite = iconTexture;
        }

        // Set the tooltip (if there is one)
        if(tooltip.Length > 0)
        {
            alertGo.GetComponent<TooltipGo>().tipToShow = tooltip;
        }

        // Zoom to the target (if given)

        alertGo.GetComponent<ActivityPopup>().zoomOnClick = target;
    }
}
