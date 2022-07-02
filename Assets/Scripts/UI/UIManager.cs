using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DirectAbilitiesFull;

public class UIManager : MonoBehaviour
{
    /// <summary>
    /// This script will handle all of the UI
    /// </summary>

    public Button endTurnButton;
    public TMP_Text ccMovement; // Current (player) character's remaining movement
    public TMP_Text ccName; // Current (player) character's name
    public TMP_Text ccActions; // Current (player) character's remaining actions
    public TMP_Text ccBActions; // Current (player) character's remaining bonus actions
    public TMP_Text ccRActions; // Current (player) character's remaining reactions
    public Image ccPicture; // Current (player) character's portrait
    public TMP_Text ccClass_lvl; // Current (player) character's class and level

    public GameObject abilityPrefab; // UI Prefab for an ability
    public GameObject mainCamera;
    public Canvas canvas;
    public GameObject camLoc;
    public GameObject mainstartRef;

    public GameObject initiativePrefab;

    public int escapeLevel = 0;

    // Health Bar

    public Image healthBar;
    public float currentHealth;
    public float maxHP;
    public TMP_Text healthText;

    // Spell Slots

    public GameObject ssBarRef;
    public TMP_Text s1;
    public TMP_Text s2;
    public TMP_Text s3;
    public TMP_Text s4;
    public TMP_Text s5;
    public TMP_Text s6;
    public TMP_Text s7;
    public TMP_Text s8;
    public TMP_Text s9;

    public static UIManager inst;
    public void Awake()
    {
        inst = this;
    }

    // -------------------------------------------------------- UI ----------------------------------------------- //

    void Start()
    {
        Button btn = endTurnButton.GetComponent<Button>();
        btn.onClick.AddListener(EndPlayerTurn);
        mainstartRef.SetActive(true);
    }

    void TaskOnClick()
    {

    }

    public void UpdateHUD()
    {
        string class_lvl = "";

        if(TurnManager.targetPlayer != null)
        {
            ccMovement.SetText(TurnManager.targetPlayer.move.ToString()); // Movement
            ccName.SetText(TurnManager.targetPlayer.GetComponent<Character>().characterName); // Name
            ccActions.SetText(TurnManager.targetPlayer.GetComponent<Character>().normalAction.ToString()); // Actions
            ccBActions.SetText(TurnManager.targetPlayer.GetComponent<Character>().bonusAction.ToString()); // Bonus Actions
            ccRActions.SetText(TurnManager.targetPlayer.GetComponent<Character>().reAction.ToString()); // Reactions
            ccPicture.sprite = TurnManager.targetPlayer.GetComponent<Character>().characterPfp; // PFP
            class_lvl = TurnManager.targetPlayer.GetComponent<Character>().classString + " (" + TurnManager.targetPlayer.GetComponent<Character>().characterLevel + ")";
            ccClass_lvl.SetText(class_lvl);

            // HP
            currentHealth = TurnManager.targetPlayer.GetComponent<Character>().hitPoints;
            maxHP = TurnManager.targetPlayer.GetComponent<Character>().maxHealth;
            healthBar.fillAmount = currentHealth / maxHP;
            string newHPText = currentHealth.ToString() + "/" + maxHP.ToString();
            healthText.SetText(newHPText);
        }
        else
        {
            ccMovement.SetText(TurnManager.currentUnit.move.ToString()); // Movement
            ccName.SetText(TurnManager.currentUnit.GetComponent<Character>().characterName); // Name
            ccActions.SetText(TurnManager.currentUnit.GetComponent<Character>().normalAction.ToString()); // Actions
            ccBActions.SetText(TurnManager.currentUnit.GetComponent<Character>().bonusAction.ToString()); // Bonus Actions
            ccRActions.SetText(TurnManager.currentUnit.GetComponent<Character>().reAction.ToString()); // Reactions
            ccPicture.sprite = TurnManager.currentUnit.GetComponent<Character>().characterPfp; // PFP
            class_lvl = TurnManager.currentUnit.GetComponent<Character>().classString + " (" + TurnManager.currentUnit.GetComponent<Character>().characterLevel + ")";
            ccClass_lvl.SetText(class_lvl);

            // HP
            currentHealth = TurnManager.currentUnit.GetComponent<Character>().hitPoints;
            maxHP = TurnManager.currentUnit.GetComponent<Character>().maxHealth;
            healthBar.fillAmount = currentHealth / maxHP;
            string newHPText = currentHealth.ToString() + "/" + maxHP.ToString();
            healthText.SetText(newHPText);
        }


    }

    public void EnableEndTurnButton() // Enable the player to end their turn
    {
        endTurnButton.enabled = true;
    }

    public GameObject movementVisRef;

    public void EndPlayerTurn() // Used by "End Turn" button
    {
        if(!TurnManager.currentUnit.moving && !TurnManager.currentUnit.attacking)
        {
            endTurnButton.enabled = false;
            TurnManager.EndTurn();
            movementVisRef.GetComponent<MovementVis>().showGuide = false;
        }
    }


    // ----------------------------------------------------------------------------------------------------------- //

    // Floating text

    public GameObject floatingTextPrefab;
    public Vector3 up = new Vector3(0, 1, 0);

    public void DoFloatingText(GameObject targetGameObject, string textToDisplay, Color desiredColor)
    {
        GameObject damageTextInstance = Instantiate(floatingTextPrefab, targetGameObject.transform.position + up, Quaternion.identity);
        damageTextInstance.transform.GetChild(0).GetComponent<TextMeshPro>().SetText(textToDisplay);
        damageTextInstance.transform.GetChild(0).GetComponent<TextMeshPro>().color = desiredColor;
    }

    public GameObject specialPrefab;

    public void SpecialAction()
    {
        GameObject newThing = Instantiate(specialPrefab, camLoc.transform.position + new Vector3(0, 2, 0), Quaternion.identity);
    }

    // UI for character abilities

    public GameObject[] abilityList;
    public GameObject abilityBase;
    string tooltip;
    
    public void GenerateAbilityUI()
    {
        int count = TurnManager.currentUnit.GetComponent<Character>().abilities.Count;

        if (count > 0)
        {
            foreach(GameObject ability in abilityList)
            {
                ability.GetComponent<AbilityUI>().DestroySelf();
            }
        }

        abilityList = new GameObject[count];

        for(int i = 0; i < count; i++)
        {
            GameObject newAbility = (GameObject)Instantiate(abilityPrefab); // Create a generic ability from the prefab
            newAbility.transform.SetParent(abilityBase.transform, false); // Set its parent to the ability bar (gets arranged automatically)

            int currAbilityNum = TurnManager.currentUnit.GetComponent<Character>().abilities[i];

            // Get important data about the ability

            newAbility.GetComponent<AbilityUI>().abilityNumInt = currAbilityNum;
            newAbility.GetComponent<AbilityUI>().minRange = CombatManager.inst.GetComponent<DirectAbilities>().requiredRange[currAbilityNum];
            newAbility.GetComponent<AbilityUI>().maxRange = CombatManager.inst.GetComponent<DirectAbilities>().maxRange[currAbilityNum];
            newAbility.GetComponent<AbilityUI>().targetType = CombatManager.inst.GetComponent<DirectAbilities>().targetType[currAbilityNum];
            newAbility.GetComponent<AbilityUI>().targetRadius = CombatManager.inst.GetComponent<DirectAbilities>().targetRadius[currAbilityNum];
            newAbility.GetComponent<AbilityUI>().intent = CombatManager.inst.GetComponent<DirectAbilities>().abilityIntent[currAbilityNum];
            newAbility.GetComponent<AbilityUI>().cost = CombatManager.inst.GetComponent<DirectAbilities>().costOfUse[currAbilityNum];

            // Set the design of the ability

            int numToDisplay = i + 1;

            newAbility.GetComponent<AbilityUI>().abilityNum.text = numToDisplay.ToString();
            tooltip = CombatManager.inst.GetComponent<DirectAbilities>().abilityNames[currAbilityNum] + " - " + CombatManager.inst.GetComponent<DirectAbilities>().abilityDescriptions[currAbilityNum];
            newAbility.GetComponent<TooltipGo>().tipToShow = tooltip;
            newAbility.GetComponent<AbilityUI>().abilityImage.sprite = CombatManager.inst.GetComponent<DirectAbilities>().abilityIcon[currAbilityNum];

            abilityList[i] = newAbility;
        }
    }

    // UI for Initative

    public GameObject[] initCharsList;
    public GameObject initiativeBase;
    List<Character> allCharacters = new List<Character>();

    public void CreateInitiatveUI()
    {

        foreach(GameObject obj in TurnManager.act)
        {
            allCharacters.Add(obj.GetComponent<Character>());
        }

        initCharsList = new GameObject[allCharacters.Count];
        int i = 0;

        foreach (Character cha in allCharacters) // Create the iniative prefabs
        {
            GameObject newInit = (GameObject)Instantiate(initiativePrefab);
            newInit.transform.SetParent(initiativeBase.transform, false); 

            newInit.GetComponent<InitiativeIcon>().me = cha;

            if (TurnManager.IsPlayer(cha.GetComponent<TacticsMove>()))
            {
                newInit.GetComponent<InitiativeIcon>().isPlayer = true;
            }
            else
            {
                newInit.GetComponent<InitiativeIcon>().isPlayer = false;
            }

            initCharsList[i] = newInit;
            i++;
        }

        initCharsList[0].GetComponent<InitiativeIcon>().isActive = true;


    }

    public void NextCharacter() // "Moves" the indicator arrow for which character is going
    {
        for (int i = 0; i < initCharsList.Length; i++) // Disable them all
        {
            if (initCharsList[i].GetComponent<InitiativeIcon>().isActive == true)
            {
                if(i + 1 == initCharsList.Length) // Could be wrong
                {
                    initCharsList[0].GetComponent<InitiativeIcon>().isActive = true;
                    initCharsList[i].GetComponent<InitiativeIcon>().isActive = false;
                    return;
                }
                else
                {
                    initCharsList[i + 1].GetComponent<InitiativeIcon>().isActive = true;
                    initCharsList[i].GetComponent<InitiativeIcon>().isActive = false;
                    return;
                }
            }
            
        }
    }

    public void RemoveCharacterFromInitiative(Character character)
    {
        List<GameObject> temp = new List<GameObject>();

        foreach (GameObject c in initCharsList)
        {
            temp.Add(c);
        }

        foreach(GameObject x in temp)
        {
            if (x.GetComponent<InitiativeIcon>().me.Equals(character))
            {
                temp.Remove(x.gameObject);
                allCharacters.Remove(character);
                Destroy(x);
                initCharsList = temp.ToArray();
                return;
            }
        }

        //initCharsList = temp.ToArray();

        /*
        for(int i = 0; i < initCharsList.Length; i++)
        {
            initCharsList.Exis
            if(initCharsList[i].GetComponent<InitiativeIcon>().me == character)
            {
                Debug.Log("Removed: " + initCharsList[i].GetComponent<InitiativeIcon>().me);
                allCharacters.Remove(character);
                initCharsList[i].GetComponent<InitiativeIcon>().Destroy();
                initCharsList[i] = null;
                return;
            }
        }
        */
    }

    public void UpdateSpellSlots(Character targetChar)
    {
        if(targetChar != null)
        {
            if (targetChar.classString.ToString() != "Barbarian" && targetChar.classString.ToString() != "Fighter") // Has spellslots
            {
                ssBarRef.SetActive(true);

                s1.SetText(targetChar.spellSlots[1].ToString());
                s2.SetText(targetChar.spellSlots[2].ToString());
                s3.SetText(targetChar.spellSlots[3].ToString());
                s4.SetText(targetChar.spellSlots[4].ToString());
                s5.SetText(targetChar.spellSlots[5].ToString());
                s6.SetText(targetChar.spellSlots[6].ToString());
                s7.SetText(targetChar.spellSlots[7].ToString());
                s8.SetText(targetChar.spellSlots[8].ToString());
                s9.SetText(targetChar.spellSlots[9].ToString());

                if (targetChar.spellSlots[1] == 0)
                {
                    s1.color = Color.red;
                }
                if (targetChar.spellSlots[2] == 0)
                {
                    s2.color = Color.red;
                }
                if (targetChar.spellSlots[3] == 0)
                {
                    s3.color = Color.red;
                }
                if (targetChar.spellSlots[4] == 0)
                {
                    s4.color = Color.red;
                }
                if (targetChar.spellSlots[5] == 0)
                {
                    s5.color = Color.red;
                }
                if (targetChar.spellSlots[6] == 0)
                {
                    s6.color = Color.red;
                }
                if (targetChar.spellSlots[7] == 0)
                {
                    s7.color = Color.red;
                }
                if (targetChar.spellSlots[8] == 0)
                {
                    s8.color = Color.red;
                }
                if (targetChar.spellSlots[9] == 0)
                {
                    s9.color = Color.red;
                }
            }
            else
            {
                ssBarRef.SetActive(false);
            }
        }
    }
    
}
