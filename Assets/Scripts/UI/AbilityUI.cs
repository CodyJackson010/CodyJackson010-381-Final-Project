using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DirectAbilitiesFull;

public class AbilityUI : MonoBehaviour
{
    public Image abilityImage; // Image of the ability
    public TMP_Text abilityNum; // Number displayed next to image
    public Button abilityButton; // Button assigned to the ability
    public Image abilityBacking; // Backing image of the ability
    public TMP_Text abilityTextMultiuse; // Number for how many times this ability can be used
    public int limitedUseNum = 1; // Above but in int form

    public GameObject disabledRef; // Cover for when you can't use this ability anymore
    public GameObject xRef; // Multi-attack
    public Image multiBacking; // Multi-attack
    public TMP_Text multiText; // Multi-attack

    public int abilityNumInt; // The unique identifier for this ability
    public int minRange; // Minimum range needed for attack
    public int maxRange; // Maximum range needed for attack
    public string targetType; // Type of attack and how many targets it should include
    public int targetRadius; // Radius of AOE attack
    public int intent; // Intent of the attack. (0 = Harm, 1 = Aid)

    public int vantage = 0;
    public bool multiUp = false; // Multi-target number being displayed

    int intType; // Identify which cleanup needs to be ran
    bool ranOnce = false;
    public bool debug = false;
    public int debugN;

    public bool active = false; // Is this button currently active?

    public string cost; // What do you need to be able to cast this? (Action/BA/R - SS)
    public int spellCost; // ^
    public string actionCost; //  ^
    public bool requiresSpellSlot = false;

    SpriteRenderer rend;

    [Range(0f, 1f)]
    public float fadeToAmount = 0f;
    public float fadeSpeed = 0.05f;

    // Start is called before the first frame update
    void Start()
    {
        ParseCost();
    }

    // Update is called once per frame
    void Update()
    {

        if (TurnManager.currentUnit != null && TurnManager.IsPlayer(TurnManager.currentUnit))
        {
            CheckAvailable();
            SetAltNumber();

            if (requiresSpellSlot)
            {
                switch (actionCost)
                {
                    case "a":
                        if (active &&
                        (!TurnManager.currentUnit.moving && !TurnManager.currentUnit.attacking) &&
                        TurnManager.currentUnit.GetComponent<Character>().normalAction > 0 &&
                        TurnManager.currentUnit.turn && TurnManager.currentUnit.GetComponent<Character>().spellSlots[spellCost] > 0)
                        {

                            // If the button is active, the current unit isn't moving, isn't attacking, has an **action** to use, 
                            // has the required spellslot, and it's their turn...

                            HandleType();
                        }
                        else
                        {
                            DoCleanUp();
                        }
                        break;

                    case "b":
                        if (active &&
                        (!TurnManager.currentUnit.moving && !TurnManager.currentUnit.attacking) &&
                        TurnManager.currentUnit.GetComponent<Character>().bonusAction > 0 &&
                        TurnManager.currentUnit.turn && TurnManager.currentUnit.GetComponent<Character>().spellSlots[spellCost] > 0)
                        {

                            // If the button is active, the current unit isn't moving, isn't attacking, has an **action** to use, 
                            // has the required spellslot, and it's their turn...

                            HandleType();
                        }
                        else
                        {
                            DoCleanUp();
                        }
                        break;

                    case "r":
                        if (active &&
                        (!TurnManager.currentUnit.moving && !TurnManager.currentUnit.attacking) &&
                        TurnManager.currentUnit.GetComponent<Character>().reAction > 0 &&
                        TurnManager.currentUnit.turn && TurnManager.currentUnit.GetComponent<Character>().spellSlots[spellCost] > 0)
                        {

                            // If the button is active, the current unit isn't moving, isn't attacking, has an **action** to use, 
                            // has the required spellslot, and it's their turn...

                            HandleType();
                        }
                        else
                        {
                            DoCleanUp();
                        }
                        break;

                    case "f": // Pretty sure this case will never happen
                        if (active &&
                        (!TurnManager.currentUnit.moving && !TurnManager.currentUnit.attacking) &&
                        TurnManager.currentUnit.turn && TurnManager.currentUnit.GetComponent<Character>().spellSlots[spellCost] > 0)
                        {

                            // If the button is active, the current unit isn't moving, isn't attacking, 
                            // has the required spellslot, and it's their turn...

                            HandleType();
                        }
                        else
                        {
                            DoCleanUp();
                        }
                        break;
                }
            }
            else
            {
                switch (actionCost)
                {
                    case "a":
                        if (active &&
                        (!TurnManager.currentUnit.moving && !TurnManager.currentUnit.attacking) &&
                        TurnManager.currentUnit.GetComponent<Character>().normalAction > 0 &&
                        TurnManager.currentUnit.turn)
                        {

                            // If the button is active, the current unit isn't moving, isn't attacking, has an **action** to use, and it's their turn...

                            HandleType();
                        }
                        else
                        {
                            DoCleanUp();
                        }
                        break;

                    case "b":
                        if (active &&
                        (!TurnManager.currentUnit.moving && !TurnManager.currentUnit.attacking) &&
                        TurnManager.currentUnit.GetComponent<Character>().bonusAction > 0 &&
                        TurnManager.currentUnit.turn)
                        {

                            // If the button is active, the current unit isn't moving, isn't attacking, has an **bonus action** to use, and it's their turn...

                            HandleType();
                        }
                        else
                        {
                            DoCleanUp();
                        }
                        break;

                    case "r":
                        if (active &&
                        (!TurnManager.currentUnit.moving && !TurnManager.currentUnit.attacking) &&
                        TurnManager.currentUnit.GetComponent<Character>().reAction > 0 &&
                        TurnManager.currentUnit.turn)
                        {

                            // If the button is active, the current unit isn't moving, isn't attacking, has an **reaction** to use, and it's their turn...

                            HandleType();
                        }
                        else
                        {
                            DoCleanUp();
                        }
                        break;

                    case "f":
                        if (active &&
                        (!TurnManager.currentUnit.moving && !TurnManager.currentUnit.attacking) &&
                        TurnManager.currentUnit.turn)
                        {

                            // If the button is active, the current unit isn't moving, isn't attacking, and it's their turn...

                            HandleType();
                        }
                        else
                        {
                            DoCleanUp();
                        }
                        break;
                }
            }
        }
    }

    public void SwitchActive()
    {
        active = !active;
    }

    public void DestroySelf()
    {
        Destroy(gameObject);
    }

    // --------------------------------------------------------------------------------------------------------------- //

    public void HandleMelee()
    {
        if (Input.GetKeyUp(KeyCode.Escape) || !active) // Stop doing attack if user presses escape.
        {
            CleanA();
            UIManager.inst.escapeLevel = 0;
        }
        else
        {
            TurnManager.showGuide = false; // Stop showing the line
            TurnManager.doOnce = false; // Stop drawing the movement vis line
            CombatManager.inst.DrawMeleeRing(1);
            CombatManager.inst.HighlightTargets(1);
            CombatManager.inst.DoSingleAttack(abilityNumInt, "melee", vantage, actionCost, 20, 0);

            if (CombatManager.inst.doneAttacking)
            {
                CleanA();
            }
        }
    }

    public void CleanA()
    {
        CombatManager.inst.ClearTargetsList(); // We are done, lets clean up
        CombatManager.inst.line.positionCount = 0;
        TurnManager.showGuide = true;
        active = false;
    }

    public void HandleMeleeMulti(int targets)
    {

    }

    public void CleanB()
    {

    }

    public void HandleRanged()
    {
        if (Input.GetKeyUp(KeyCode.Escape) || !active) // Stop doing attack if user presses escape.
        {
            CleanC();
            UIManager.inst.escapeLevel = 0;
        }
        else
        {
            TurnManager.showGuide = false; // Stop showing the line
            TurnManager.doOnce = false; // Stop drawing the movement vis line
            CombatManager.inst.DrawMeleeRing(minRange / 5);
            CombatManager.inst.DrawMaxRing(maxRange / 5);
            CombatManager.inst.HighlightTargets(maxRange / 5, true);
            CombatManager.inst.DoSingleAttack(abilityNumInt, "ranged", vantage, actionCost, 20, 0, minRange / 5);

            if (CombatManager.inst.doneAttacking)
            {
                CleanC();
            }
        }
    }

    public void CleanC()
    {
        CombatManager.inst.ClearTargetsList(); // We are done, lets clean up
        CombatManager.inst.line.positionCount = 0;
        CombatManager.inst.farLine.positionCount = 0;
        TurnManager.showGuide = true;
        active = false;
    }

    public void HandleRangedMulti(int targets)
    {

    }

    public void CleanD()
    {

    }

    public void HandleSpellSingle()
    {
        if (Input.GetKeyUp(KeyCode.Escape) || !active) // Stop doing attack if user presses escape.
        {
            CleanE();
            UIManager.inst.escapeLevel = 0;
        }
        else
        {
            TurnManager.showGuide = false; // Stop showing the line
            TurnManager.doOnce = false; // Stop drawing the movement vis line
            CombatManager.inst.DrawMeleeRing(minRange / 5);
            CombatManager.inst.HighlightTargets(minRange / 5, true);
            CombatManager.inst.DoSingleAttack(abilityNumInt, "spell", vantage, actionCost, spellCost, 0, minRange / 5);

            if (CombatManager.inst.doneAttacking)
            {
                CleanE();
            }
        }
    }

    public void CleanE()
    {
        CombatManager.inst.ClearTargetsList(); // We are done, lets clean up
        CombatManager.inst.line.positionCount = 0;
        TurnManager.showGuide = true;
        //CombatManager.inst.measurementRef.GetComponent<MeasurementTool>().Clear();
        active = false;
    }

    public IEnumerator HandleSpellMulti(int targets)
    {
        
        if (Input.GetKeyUp(KeyCode.Escape) || !active && !multiUp) // Stop doing attack if user presses escape.
        {
            CleanF();
            UIManager.inst.escapeLevel = 0;
        }
        else
        {
            //CombatManager.inst.measurementRef.GetComponent<MeasurementTool>().DrawVisLine();

            //yield return new WaitForSeconds(0.75f);

            TurnManager.showGuide = false; // Stop showing the line
            TurnManager.doOnce = false; // Stop drawing the movement vis line
            CombatManager.inst.DrawMeleeRing(minRange / 5);
            CombatManager.inst.HighlightTargets(minRange / 5, true);

            if(xRef != null)
            {
                xRef.SetActive(true);
            }

            FlipColor();


            multiText.SetText((targets - CombatManager.inst.attacksDone).ToString());
            CombatManager.inst.DoSingleAttack(abilityNumInt, "spell", vantage, actionCost, spellCost, 0, minRange / 5);

            while (!CombatManager.inst.doneAttacking)
            {
                yield return null;
            }

            if (CombatManager.inst.doneAttacking)
            {
                if(CombatManager.inst.attacksDone == targets)
                {
                    CombatManager.inst.attacksDone = 0;

                    if (xRef != null)
                    {
                        xRef.SetActive(false);
                    }
                    CleanF();
                }
                else
                {
                    StartCoroutine(HandleSpellMulti(targets)); // Go again
                }
            }
        }
    }

    public void CleanF()
    {
        CombatManager.inst.ClearTargetsList(); // We are done, lets clean up
        CombatManager.inst.line.positionCount = 0;
        TurnManager.showGuide = true;
        //CombatManager.inst.measurementRef.GetComponent<MeasurementTool>().Clear();
        active = false;
    }

    public IEnumerator HandleSpellAOE()
    {

        if (Input.GetKeyUp(KeyCode.Escape) || !active) // Stop doing attack if user presses escape.
        {
            CleanG();
            UIManager.inst.escapeLevel = 0;
        }
        else
        {
            Vector3 shift = new Vector3(-0.5f, 0, -0.5f);
            CombatManager.inst.measurementRef.GetComponent<MeasurementTool>().DrawVisLine(shift);
            TurnManager.showGuide = false; // Stop showing the line
            TurnManager.doOnce = false; // Stop drawing the movement vis line
            CombatManager.inst.DrawMeleeRing(minRange / 5);

            CombatManager.inst.HandleAOE(targetRadius, minRange / 5, abilityNumInt, "spell", vantage, actionCost, spellCost);

            while (!CombatManager.inst.doneAttacking)
            {
                yield return null;
            }
            
            if (CombatManager.inst.doneAttacking)
            {
                CleanG();
            }
        }
    }

    public void CleanG()
    {
        CombatManager.inst.measurementRef.GetComponent<MeasurementTool>().Clear();
        CombatManager.inst.ClearTargetsList(); // We are done, lets clean up
        CombatManager.inst.line.positionCount = 0;
        CombatManager.inst.farLine.positionCount = 0;
        TurnManager.showGuide = true;
        active = false;
    }

    public void HandleEffectMulti(int targets)
    {

    }

    public void CleanH()
    {

    }

    public void HandleEffectSingle()
    {
        if (Input.GetKeyUp(KeyCode.Escape) || !active) // Stop doing attack if user presses escape.
        {
            CleanI();
            UIManager.inst.escapeLevel = 0;
        }
        else
        {

            if(minRange / 5 == 1 || minRange < 0)
            {
                // Melee range, don't both with line
            }
            else
            {
                Vector3 shift = new Vector3(0, 0, 0);
                CombatManager.inst.measurementRef.GetComponent<MeasurementTool>().DrawVisLine(shift);
            }
            TurnManager.showGuide = false; // Stop showing the line
            TurnManager.doOnce = false; // Stop drawing the movement vis line


            CombatManager.inst.DrawMeleeRing(minRange / 5);
            
            switch (intent)
            {
                case 0: // Harmful attack
                    CombatManager.inst.HighlightTargets(minRange / 5);
                    CombatManager.inst.DoSingleAttack(abilityNumInt, "effect", vantage, actionCost, spellCost, 0);
                    break;

                case 1: // Aid attack
                    CombatManager.inst.HighlightAllies(minRange / 5);
                    CombatManager.inst.DoSingleAttack(abilityNumInt, "effect", vantage, actionCost, spellCost, 1);
                    break;

                case 2: // Self
                    UIManager.inst.UpdateHUD(); // Update the HUD

                    CombatManager.inst.CreateCombatEvent(TurnManager.currentUnit.GetComponent<Character>(), TurnManager.currentUnit.GetComponent<Character>(), "effect", abilityNumInt, vantage);
                    CombatManager.inst.doneAttacking = true;

                    TurnManager.currentUnit.GetComponent<Character>().usedAbilities.Add(abilityNumInt);
                    break;

                default:
                    Debug.Log("Error, invalid intent in AbilityUI (HandleSpellSingle()");
                    break;
            }

            if (CombatManager.inst.doneAttacking)
            {
                CleanI();
            }
        }
    }

    public void CleanI()
    {
        CombatManager.inst.measurementRef.GetComponent<MeasurementTool>().Clear();
        CombatManager.inst.ClearTargetsList(); // We are done, lets clean up
        CombatManager.inst.line.positionCount = 0;
        CombatManager.inst.farLine.positionCount = 0;
        TurnManager.showGuide = true;
        active = false;
    }

    public IEnumerator HandleEffectAOE()
    {


        while (!CombatManager.inst.doneAttacking)
        {
            yield return null;
        }
    }

    public void CleanJ()
    {

    }

    // --------------------------------------------------------------------------------------------------------------------------- //

    IEnumerator ColorFlip()
    {
        for(float i = 1f; i >= fadeToAmount; i -= 0.05f)
        {
            Color c = rend.material.color;

            c.g = i;
            c.b = i;

            rend.material.color = c;

            yield return new WaitForSeconds(fadeSpeed);
        }
    }

    public void ColorFlipSetup()
    {
        rend = multiBacking.GetComponent<SpriteRenderer>();
        Color c = rend.material.color;
        c.g = 1f;
        c.b = 1f;
        rend.material.color = c;
    }

    public void FlipColor()
    {
        multiBacking.color = Color.magenta;
    }

    public void ParseCost()
    {
        if (cost.Length > 1) // We need to decode this, it requires a spellslot
        {
            string[] minmax = cost.Split('-'); // Splits the string
            actionCost = minmax[0];
            spellCost = int.Parse(minmax[1]);
            requiresSpellSlot = true;
        }
        else // No spellslot needed
        {
            actionCost = cost;
            requiresSpellSlot = false;
        }
    }

    public void HandleType()
    {
        // First, what kind of ability is this?

        ranOnce = true;
        intType = 0;
        UIManager.inst.escapeLevel = 1;
        CombatManager.inst.attacksDone = 0;
        CombatManager.inst.doneAttacking = false;
        CombatManager.inst.doOnce = false;

        vantage = TurnManager.currentUnit.GetComponent<Character>().vantageStatus;


        if (targetType.Length > 2) // We need to decode this
        {

            string[] minmax = targetType.Split('-'); // Splits the string to the type and how many targets
            string type = minmax[0];
            int targets = int.Parse(minmax[1]);

            switch (type)
            {
                case "m": // Multi target melee
                    intType = 5;
                    CombatManager.inst.attacksDone = 0;
                    HandleMeleeMulti(targets);
                    break;

                case "rm": // Multi target ranged
                    intType = 6;
                    CombatManager.inst.attacksDone = 0;
                    HandleRangedMulti(targets);
                    break;

                case "s": // Multi target spell
                    intType = 7;
                    StartCoroutine(HandleSpellMulti(targets));
                    break;

                case "e": // Multi target effect
                    intType = 8;
                    HandleEffectMulti(targets);
                    break;

                default:
                    Debug.LogError("Invalid type of multi-target-attack in AbilityUI.");
                    break;
            }
        }


        else // This is a single target attack (or AOE)
        {
            CombatManager.inst.doOnce = false;
            switch (targetType)
            {
                case "ms": // Single target melee
                    intType = 1;
                    HandleMelee();
                    break;

                case "rs": // Single target ranged
                    intType = 2;
                    HandleRanged();
                    break;

                case "ss": // Single target spell
                    intType = 3;
                    HandleSpellSingle();
                    break;

                case "sa": // AOE Spell *
                    intType = 4;
                    StartCoroutine(HandleSpellAOE());

                    break;

                case "es": // Effect single
                    intType = 9;
                    HandleEffectSingle();
                    break;

                case "ea": // Effect AOE
                    intType = 10;
                    StartCoroutine(HandleEffectAOE());
                    break;

                default:
                    Debug.LogError("Invalid type of attack in AbilityUI.");
                    break;
            }
        }
    }

    public void DoCleanUp()
    {
        SetAltNumber();

        if (ranOnce)
        {
            switch (intType)
            {
                case 0:
                    Debug.LogError("Something wrong with intType in AbilityUI.");
                    break;

                case 1:
                    CleanA(); // Single Melee
                    break;

                case 2:
                    CleanC(); // Single Ranged
                    break;

                case 3:
                    CleanE(); // Single Spell
                    break;

                case 4:
                    CleanG(); // AOE Spell
                    break;

                case 5:
                    CleanB(); // Multi-Melee
                    break;

                case 6:
                    CleanD(); // Multi target ranged
                    break;

                case 7:
                    CleanF(); // Multi target spell
                    break;

                case 8:
                    CleanH(); // Multi Target Effect
                    break;

                case 9:
                    CleanI(); // Single target effect
                    break;

                case 10:
                    CleanJ(); // Effect AOE
                    break;
            }

            ranOnce = false;

            if (requiresSpellSlot)
            {
                UIManager.inst.UpdateSpellSlots(TurnManager.currentUnit.GetComponent<Character>()); // Update spell slots UI
            }
        }
    }

    public void CheckAvailable()
    {
        if(disabledRef == null)
        {
            return;
        }

        if(disabledRef.activeSelf == false) // If its off
        {
            if (TurnManager.currentUnit.GetComponent<Character>().usedAbilities.Contains(abilityNumInt)) // If this ability has been used, disable this abilities use.
            {
                limitedUseNum = 0;
                disabledRef.SetActive(true);
                return;
            }

            if (requiresSpellSlot) // This needs a spell slot
            {
                if (TurnManager.currentUnit.GetComponent<Character>().spellSlots[CombatManager.inst.GetComponent<DirectAbilities>().neededSpellSlot[abilityNumInt]] == 0)
                {
                    // Out of spell slots needed to cast this spell

                    disabledRef.SetActive(true);
                    return;
                }
            }
        }
    }

    public void SetAltNumber()
    {
        if (abilityTextMultiuse == null)
        {
            return;
        }

        if (CombatManager.inst.GetComponent<DirectAbilities>().limitedUse[abilityNumInt] == -1 || CombatManager.inst.GetComponent<DirectAbilities>().neededSpellSlot[abilityNumInt] == 0) // No limited use, turn off the alt text
        {
            abilityTextMultiuse.enabled = false;
        }
        else if (CombatManager.inst.GetComponent<DirectAbilities>().limitedUse[abilityNumInt] == -5) // Limited use (spell slot based)
        {
            int needed;

            needed = CombatManager.inst.GetComponent<DirectAbilities>().neededSpellSlot[abilityNumInt];
            limitedUseNum = TurnManager.currentUnit.GetComponent<Character>().spellSlots[needed];

            abilityTextMultiuse.SetText(limitedUseNum.ToString());
        }
        else // Limited use (conventional)
        {
            abilityTextMultiuse.SetText(limitedUseNum.ToString());
        }
    }
}
