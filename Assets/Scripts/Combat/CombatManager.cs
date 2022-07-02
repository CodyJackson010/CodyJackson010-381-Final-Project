using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DirectAbilitiesFull;

public class CombatManager : MonoBehaviour
{
    /// <summary>
    /// This class handles combat.
    /// </summary>

    public int attackRoll;
    public int saveRoll;
    public int damageDone;
    public bool outcome;
    public LineRenderer line;
    public LineRenderer farLine;
    public List<GameObject> viableTargets = new List<GameObject>();
    public bool doneAttacking;
    public int attacksDone;
    public bool targetSelection;
    public bool targetReached = false;
    public float timeToWait = 0.01f; // Time to wait before explosion

    public List<GameObject> groupTargets = new List<GameObject>(); // For attacks that hit multiple units

    string floatingString; // used for floating text
    Color passColor; // used for floating text
    string tooltipReadout; // For the tooltip in the activity log
    public bool doOnce = true;

    public GameObject measurementRef;
    public GameObject aoePrefab;
    public GameObject aoeCenter;
    public GameObject tempSoundPref;

    public static CombatManager inst;
    public void Awake()
    {
        inst = this;
        line.positionCount = 0;
    }
    
    /// <summary>
    /// Create a combat event with a single target.
    /// </summary>
    /// <param name="attacker">The person who is attcking.</param>
    /// <param name="defender">The person who is being attacked.</param>
    /// <param name="typeOfAttack">The type of attack.</param>
    /// <param name="attackName">The name of the attack.</param>
    /// <param name="vantage">Does this character have advantage, disadvantage, or none on the attack?</param>
    public void CreateCombatEvent(Character attacker, Character defender, string typeOfAttack, int attackName, int vantage)
    {
        outcome = false;
        damageDone = 0;
        attacksDone++;
        targetReached = false;

        //UIManager.inst.DoFloatingText(attacker.GetComponent<Transform>().gameObject, this.GetComponent<DirectAbilities>().abilityNames[attackName], Color.white);

        switch (typeOfAttack)
        {
            case "melee": // Melee attack
                HandleMelee(attacker, defender, attackName, vantage);
                CheckConcentration(attacker, defender);
                break;

            case "ranged": // Ranged attack
                HandleRanged(attacker, defender, attackName, vantage);
                CheckConcentration(attacker, defender);
                break;

            case "spell": // Spell attack (this is going to get complicated)
                HandleSpell(attacker, defender, attackName, vantage);
                CheckConcentration(attacker, defender);
                break;

            case "effect": // Apply effect to target; healing, buff, boon, etc
                HandleEffect(attacker, defender, attackName, vantage);
                break;

            default:
                Debug.LogError("Invalid attack type.");
                break;
        }
    }

    public void CheckConcentration(Character attacker, Character defender)
    {
        if (defender.concentrating)
        {
            // Check concentration

            int mod = defender.constitutionMod;
            int result;

            if (attacker != defender && attacker.tag != defender.tag) // Someone else and an enemy
            {
                if (outcome == true) // A hit
                {
                    if (damageDone / 2 > 10) // Damage done (halved), is higher, use that
                    {
                        result = RollWithVantage(defender, 0, mod);
                        if (result >= damageDone / 2) // Success! Keep concentration
                        {
                            UIManager.inst.DoFloatingText(defender.GetComponent<Transform>().gameObject, "Concentration success [" + result + "]", Color.green);
                        }
                        else // Failure! Concentration broken.
                        {
                            defender.concentrating = false;

                            foreach (GameObject go in defender.concTargets)
                            {
                                if (go.GetComponent<Character>().debuffs.Contains(defender.concAbilityId))
                                {
                                    go.GetComponent<Character>().debuffs.Remove(defender.concAbilityId);
                                }

                                if (go.GetComponent<Character>().buffs.Contains(defender.concAbilityId))
                                {
                                    go.GetComponent<Character>().buffs.Remove(defender.concAbilityId);
                                }

                                if (defender.concAbilityId == 17)
                                {
                                    go.GetComponent<Character>().armorClass -= 2;
                                }
                            }
                            defender.concAbilityId = 0;

                            UIManager.inst.DoFloatingText(defender.GetComponent<Transform>().gameObject, "Concentration check failed [" + result + "]", Color.yellow);
                        }
                    }
                    else // 10 is higher, use that
                    {
                        result = RollWithVantage(defender, 0, mod);
                        if (result >= 10) // Success! Keep concentration
                        {
                            UIManager.inst.DoFloatingText(defender.GetComponent<Transform>().gameObject, "Concentration success [" + result + "]", Color.green);
                        }
                        else // Failure! Concentration broken.
                        {
                            defender.concentrating = false;

                            foreach (GameObject go in defender.concTargets)
                            {
                                if (go.GetComponent<Character>().debuffs.Contains(defender.concAbilityId))
                                {
                                    go.GetComponent<Character>().debuffs.Remove(defender.concAbilityId);
                                }

                                if (go.GetComponent<Character>().buffs.Contains(defender.concAbilityId))
                                {
                                    go.GetComponent<Character>().buffs.Remove(defender.concAbilityId);
                                }

                                if (defender.concAbilityId == 17)
                                {
                                    go.GetComponent<Character>().armorClass -= 2;
                                }
                            }
                            defender.concAbilityId = 0;

                            UIManager.inst.DoFloatingText(defender.GetComponent<Transform>().gameObject, "Concentration check failed [" + result + "]", Color.yellow);
                        }
                    }
                }
            }
        }
    }

    public void PlayerCheck(Character attacker, Character defender)
    {
        if (TurnManager.IsPlayer(attacker.GetComponent<TacticsMove>()))
        {

        }
        else // Don't change the UI target if the player is attacking someone
        {
            TurnManager.targetPlayer = defender.GetComponent<TacticsMove>();
        }
    }

    void HandleMelee(Character attacker, Character defender, int attackName, int vantage)
    {
        int modifier = 0;
        string dmgType = "";

        switch (attackName)
        {
            case 0: // Debug attack ------------------------------------------------------------------------
                // 1D6 Slashing damage + strength + prof
                modifier = attacker.strengthMod;
                dmgType = "Slashing";

                MeleeFunc(attacker, defender, vantage, modifier, dmgType, 1, 6, 0, "icon_genericsword");

                break;

            case 1: // Scimitar (Goblin) attack ----------------------------------------------------------------
                // 1D6 Slashing damage + dex + prof
                modifier = attacker.dexterityMod;
                dmgType = "Slashing";

                MeleeFunc(attacker, defender, vantage, modifier, dmgType, 1, 6, 0, "icon_scimitar");

                break;

            case 4: // Longsword melee attack ----------------------------------------------------------------
                // 1D8 Slashing damage + strength + prof
                modifier = attacker.strengthMod;
                dmgType = "Slashing";

                MeleeFunc(attacker, defender, vantage, modifier, dmgType, 1, 6, 0, "icon_longsword");

                break;

            case 15: // Club (Goblin) attack ----------------------------------------------------------------
                // 1D4 Bludgening damage + dex + prof
                modifier = attacker.dexterityMod;
                dmgType = "Bludgeoning";

                MeleeFunc(attacker, defender, vantage, modifier, dmgType, 1, 4, 0, "icon_club");

                break;

            default:
                Debug.LogError("Invalid attack name in HandleMelee().");
                break;
        }
    }

    void HandleRanged(Character attacker, Character defender, int attackName, int vantage)
    {
        int modifier = 0;
        string dmgType = "";

        int attRange = CalculateDistance(attacker.gameObject, defender.gameObject);
        if(attRange * 5 > this.GetComponent<DirectAbilities>().requiredRange[attackName])
        {
            vantage = 2; // If you are attacking from greater than the "short" range, you have disadvantage on the attack.
        }
        

        switch (attackName)
        {
            case 2: // Light Crossbow attack (Bandit) ------------------------------------------------------------------------
                // 1D8 Piercing damage + dex + prof
                modifier = attacker.dexterityMod + attacker.proficiency;
                dmgType = "Piercing";

                RangedFunc(attacker, defender, vantage, modifier, dmgType, 1, 8, attacker.dexterityMod, 0, "icon_crossbow");

                break;

            case 14: // Shortbow attack (Goblin) ------------------------------------------------------------------------
                // 1D6 Piercing damage + dex + prof
                modifier = attacker.dexterityMod + attacker.proficiency;
                dmgType = "Piercing";

                RangedFunc(attacker, defender, vantage, modifier, dmgType, 1, 6, attacker.dexterityMod, 0, "icon_shortbow");

                break;

            case 16: // Javelin ------------------------------------------------------------------------
                // 1D6 Piercing damage + dex + prof
                modifier = attacker.dexterityMod + attacker.proficiency;
                dmgType = "Piercing";

                RangedFunc(attacker, defender, vantage, modifier, dmgType, 1, 6, attacker.dexterityMod, 0, "icon_javelin");

                break;


            default:
                Debug.LogError("Invalid attack name in HandleRanged.");
                break;
        }
    }

    void HandleSpell(Character attacker, Character defender, int attackName, int vantage)
    {
        int modifier = 0;
        string dmgType = "";

        switch (attackName)
        {
            case 3: // Scorching Ray (Lv2 multi target) ------------------------------------------------------------------------
                // 2D6 Fire damage * 3 (shots)
                modifier = attacker.spellAttackMod + attacker.proficiency;
                dmgType = "fire";

                StartCoroutine(SpellFunc(attacker, defender, vantage, modifier, dmgType, 2, 6, 0, 1, "red", 0, "icon_scorchingray"));

                break;

            case 5: // Fireball (Lv3 AOE save half) ------------------------------------------------------------------------
                // 8D6 Fire damage
                modifier = attacker.spellAttackMod + attacker.proficiency + 8; // Need to beat this to save
                dmgType = "fire";
                int extraMod = defender.dexterityMod + defender.proficiency;

                StartCoroutine(SpellFunc(attacker, defender, vantage, modifier, dmgType, 8, 6, 0, 3, "red", 1, "icon_fireball", extraMod));

                break;

            case 6: // Cure Wounds (Lv1 healing single melee) ------------------------------------------------------------------------
                // 1D8 + spellMod
                modifier = attacker.spellAttackMod + attacker.proficiency;
                dmgType = "none";

                StartCoroutine(SpellFunc(attacker, defender, vantage, modifier, dmgType, 1, 8, 0, 1, "blue", 0, "icon_curewounds"));

                break;

            case 11: // Firebolt (Lv0 single target) ------------------------------------------------------------------------
                // 1D10 Fire damage
                modifier = attacker.spellAttackMod + attacker.proficiency;
                dmgType = "fire";
                int diceToRoll;

                if (attacker.characterLevel > 16)
                {
                    diceToRoll = 4;
                }
                else if (attacker.characterLevel > 10)
                {
                    diceToRoll = 3;
                }
                else if (attacker.characterLevel > 4)
                {
                    diceToRoll = 2;
                }
                else
                {
                    diceToRoll = 1;
                }

                StartCoroutine(SpellFunc(attacker, defender, vantage, modifier, dmgType, diceToRoll, 10, 0, 1, "red", 0, "icon_firebolt"));

                break;

            case 13: // Sacred flame (Lv0 single target) ------------------------------------------------------------------------
                // 1D8 Radiant damage
                modifier = attacker.spellAttackMod + attacker.proficiency;
                dmgType = "radiant";


                StartCoroutine(SpellFunc(attacker, defender, vantage, modifier, dmgType, 1, 8, 0, 1, "red", 1, "icon_sacredflame"));

                break;

            case 19: // Chaos Bolt (Lv1 single target) ------------------------------------------------------------------------
                // 2D8 + 1d6
                modifier = attacker.spellAttackMod + attacker.proficiency;
                int random = Random.Range(0, 11);
                dmgType = this.GetComponent<DirectAbilities>().attackTypes[random]; // Not how this should work but no one needs to know

                StartCoroutine(SpellFunc(attacker, defender, vantage, modifier, dmgType, 2, 8, 0, 1, "magenta", 0, "icon_chaosbolt", 1, 6));

                break;

            /*
            case x: // Hold person (Lv2 save or suck spell) -----------------------------------------------------------------------
                modifier = defender.wisdomMod + defender.proficiency;
             */

            default:
                Debug.LogError("Invalid attack name in HandleSpell.");
                break;
        }
    }

    void HandleEffect(Character attacker, Character defender, int attackName, int vantage)
    {
        int modifier = 0;
        //int xMod = 0;
        string dmgType = "";
        int effect = 0;

        switch (attackName)
        {
            case 6: // Cure Wounds (Lv1 healing single melee) ------------------------------------------------------------------------
                // 1D8 + spellMod
                modifier = attacker.spellAttackMod + attacker.proficiency; // Used for healing
                dmgType = "none";

                SFXManager.inst.CreateEffectHere("icon_curewonds", defender.gameObject.transform.position);
                StartCoroutine(EffectFunc(attacker, defender, vantage, effect, modifier, dmgType, 1, 8, 0, 1, "blue", 0, 1, "icon_curewounds"));

                break;

            case 7: // Healing Word (Lv1 healing single ranged) ------------------------------------------------------------------------
                // 1D4 + spellMod
                modifier = attacker.spellAttackMod + attacker.proficiency; // Used for healing
                dmgType = "none";

                SFXManager.inst.CreateEffectHere("icon_healingword", defender.gameObject.transform.position);
                StartCoroutine(EffectFunc(attacker, defender, vantage, effect, modifier, dmgType, 1, 4, 0, 1, "blue", 0, 1, "icon_healingword"));

                break;

            case 8: // Bless ?? ------------------------------------------------------------------------
                effect = 8;
                dmgType = "none";

                attacker.concentrating = true;
                attacker.concAbilityId = attackName;
                attacker.concTargets.Add(defender.gameObject);
                StartCoroutine(EffectFunc(attacker, defender, vantage, effect, modifier, dmgType, 0, 0, 0, 1, "blue", 0, 1, "icon_bless"));

                break;

            case 9: // Second Wind (Self Heal) ------------------------------------------------------------------------
                // 1D10 + level
                modifier = attacker.characterLevel; // Used for healing
                dmgType = "none";

                SFXManager.inst.CreateEffectHere("icon_secondwind", defender.gameObject.transform.position);
                StartCoroutine(EffectFunc(attacker, defender, vantage, effect, modifier, dmgType, 1, 10, 0, 1, "blue", 0, 1, "icon_secondwind"));

                break;

            case 10: // Action surge (Self add an action) ------------------------------------------------------------------------
                // Add 1 action

                attacker.normalAction += 1;
                passColor = Color.blue;

                UIManager.inst.UpdateHUD(); // For healthbar
                UIManager.inst.DoFloatingText(attacker.GetComponent<Transform>().gameObject, "Second Wind", passColor);

                break;

            case 12: // Bane ?? ------------------------------------------------------------------------
                effect = 12;
                dmgType = "none";

                attacker.concentrating = true;
                attacker.concAbilityId = attackName;
                attacker.concTargets.Add(defender.gameObject);
                StartCoroutine(EffectFunc(attacker, defender, vantage, effect, modifier, dmgType, 0, 0, 0, 1, "blue", 2, 0, "icon_bane"));

                break;

            case 17: // Shield of Faith ------------------------------------------------------------------------
                effect = 17;
                dmgType = "none";

                attacker.concentrating = true;
                attacker.concAbilityId = attackName;
                attacker.concTargets.Add(defender.gameObject);
                StartCoroutine(EffectFunc(attacker, defender, vantage, effect, modifier, dmgType, 0, 0, 0, 1, "blue", 0, 1, "icon_shieldoffaith"));

                break;

            case 18: // Shield ------------------------------------------------------------------------
                effect = 18;
                dmgType = "none";

                StartCoroutine(EffectFunc(attacker, defender, vantage, effect, modifier, dmgType, 0, 0, 0, 1, "blue", 0, 1, "icon_shield"));

                break;

            default:
                Debug.LogError("Invalid attack name in HandleEffect.");
                break;
        }
    }

    public void DrawMeleeRing(int range) // Draws a ring around the current character to indicate the range of a melee attack
    {
        line.positionCount = 5;

        Vector3 offset1 = new Vector3(-0.5f - range, 0, 0.5f + range); // Top left
        Vector3 offset2 = new Vector3(0.5f + range, 0, 0.5f + range); // Top right
        Vector3 offset3 = new Vector3(0.5f + range, 0, -0.5f - range); // Bottom right
        Vector3 offset4 = new Vector3(-0.5f - range, 0, -0.5f - range); // Bottom left

        line.SetPosition(0, TurnManager.currentUnit.transform.position + offset1);
        line.SetPosition(1, TurnManager.currentUnit.transform.position + offset2);
        line.SetPosition(2, TurnManager.currentUnit.transform.position + offset3);
        line.SetPosition(3, TurnManager.currentUnit.transform.position + offset4);
        line.SetPosition(4, TurnManager.currentUnit.transform.position + offset1);
    }

    public void DrawMaxRing(int range) // Draws a ring around the current character to indicate the max range
    {
        farLine.positionCount = 5;

        Vector3 offset1 = new Vector3(-0.5f - range, 0, 0.5f + range); // Top left
        Vector3 offset2 = new Vector3(0.5f + range, 0, 0.5f + range); // Top right
        Vector3 offset3 = new Vector3(0.5f + range, 0, -0.5f - range); // Bottom right
        Vector3 offset4 = new Vector3(-0.5f - range, 0, -0.5f - range); // Bottom left

        farLine.SetPosition(0, TurnManager.currentUnit.transform.position + offset1);
        farLine.SetPosition(1, TurnManager.currentUnit.transform.position + offset2);
        farLine.SetPosition(2, TurnManager.currentUnit.transform.position + offset3);
        farLine.SetPosition(3, TurnManager.currentUnit.transform.position + offset4);
        farLine.SetPosition(4, TurnManager.currentUnit.transform.position + offset1);
    }

    public void HighlightAllies(int range, bool needLOS = false, GameObject centerPoint = null) // Draw a red outline on potential targets
    {
        float distance;
        viableTargets.Clear();

        foreach (GameObject unit in TurnManager.act)
        {
            if (centerPoint == null)
            {
                distance = CalculateDistance(TurnManager.currentUnit.gameObject, unit.gameObject);
            }
            else
            {
                //distance = CalculateDistance(centerPoint, unit.gameObject);
                distance = Vector3.Distance(centerPoint.transform.position, unit.transform.position);
            }

            if (distance <= range && unit.tag != "NPC") // Yes, it can be attacked!
            {

                bool losCheck = CheckLineOfSight(unit);

                if (needLOS) // Do we need line of sight to do this attack?
                {
                    if (losCheck) // Yes it's in line of sight, add it.
                    {
                        unit.GetComponent<Outline>().enabled = true;
                        viableTargets.Add(unit);
                    }
                    else // No it,s not in line of sight, don't add it.
                    {

                    }
                }
                else // No LOS needed
                {
                    unit.GetComponent<Outline>().enabled = true;
                    viableTargets.Add(unit);
                }
            }
            else // Too far, can't be attacked
            {

            }
        }
    }

    public void HighlightTargets(int range, bool needLOS = false, bool includeAllies = false, GameObject centerPoint = null) // Draw a red outline on potential targets
    {
        float distance;
        viableTargets.Clear();

        foreach (GameObject unit in TurnManager.act)
        {
            if(centerPoint == null)
            {
                distance = CalculateDistance(TurnManager.currentUnit.gameObject, unit.gameObject);
            }
            else
            {
                //distance = CalculateDistance(centerPoint, unit.gameObject);
                distance = Vector3.Distance(centerPoint.transform.position, unit.transform.position);
            }

            if (!includeAllies) // Don't include other players
            {
                if (distance <= range && unit.tag != "Player") // Yes, it can be attacked!
                {

                    bool losCheck = CheckLineOfSight(unit);

                    if (needLOS) // Do we need line of sight to do this attack?
                    {
                        if (losCheck) // Yes it's in line of sight, add it.
                        {
                            unit.GetComponent<Outline>().enabled = true;
                            viableTargets.Add(unit);
                        }
                        else // No it,s not in line of sight, don't add it.
                        {

                        }
                    }
                    else // No LOS needed
                    {
                        unit.GetComponent<Outline>().enabled = true;
                        viableTargets.Add(unit);
                    }
                }
                else // Too far, can't be attacked
                {

                }
            }
            else // We do a little friendly fire
            {
                if (distance <= range) // Yes, it can be attacked!
                {

                    bool losCheck = CheckLineOfSight(unit);

                    if (needLOS) // Do we need line of sight to do this attack?
                    {
                        if (losCheck) // Yes it's in line of sight, add it.
                        {
                            unit.GetComponent<Outline>().enabled = true;
                            viableTargets.Add(unit);
                        }
                        else // No it,s not in line of sight, don't add it.
                        {

                        }
                    }
                    else // No LOS needed
                    {
                        unit.GetComponent<Outline>().enabled = true;
                        viableTargets.Add(unit);
                    }
                }
                else // Too far, can't be attacked
                {

                }
            }
        }
    }

    public void HandleAOE(int aoeSize, int maxRange, int attackNum, string attType, int vantage, string normCost, int spellCost)
    {

        foreach (GameObject go in viableTargets)
        {
            go.GetComponent<Outline>().enabled = false;
        }

        viableTargets.Clear();

        if (aoeCenter != null) // If aoe center exists, destroy it
        {
            Destroy(aoeCenter);
        }

        Vector3 adjust = new Vector3(-0.5f, 0f, -0.5f);
        Vector3 spawnLocation = measurementRef.GetComponent<MeasurementTool>().endTile.gameObject.transform.position + adjust;

        aoeSize = aoeSize * 2 / 5;

        aoeCenter = Instantiate(aoePrefab, spawnLocation, Quaternion.identity);
        
        aoeCenter.transform.localScale = new Vector3(aoeSize, aoeSize, aoeSize);

        //aoeCenter.GetComponentInChildren<SphereCollider>().enabled = true;
        //aoeCenter.GetComponentInChildren<SphereCollider>().enabled = false;

        HighlightTargets(aoeSize / 2, false, true, aoeCenter);

        foreach (GameObject go in viableTargets)
        {
            go.GetComponent<Outline>().enabled = true;
        }

        if (Input.GetMouseButtonUp(1)) // 0 = Left-click | 1 - Right-click
        {
            DoAOEAttack(attackNum, attType, vantage, normCost, spellCost);
        }
    }

    public void ClearTargetsList()
    {
        line.positionCount = 0; // Stop rendering the line
        foreach (GameObject unit in TurnManager.act) // De-highlight everything
        {
            unit.GetComponent<Outline>().enabled = false;
        }
        viableTargets.Clear(); // Clear the list of viable targets

        if(TurnManager.currentUnit.move > 0) // Don't want to refresh player's movement
        {
            TurnManager.doOnce = true; // Draw the movement vis line again
        }

        if (aoeCenter != null) // If aoe center exists, destroy it
        {
            Destroy(aoeCenter);
        }

        TurnManager.currentUnit.attacking = false;
    }

    public void DoAOEAttack(int attackNum, string attType, int vantage, string normCost, int spellCost)
    {
        doOnce = false;

        if (Input.GetMouseButtonUp(1)) // 0 = Left-click | 1 - Right-click
        {
            targetSelection = false;

            TurnManager.currentUnit.attacking = true; // Player is attacking (animate or something)

            if (spellCost == 20) // Don't remove a spellslot
            {

            }
            else
            {
                TurnManager.currentUnit.GetComponent<Character>().spellSlots[spellCost] -= 1; // Remove the spellslot
            }

            switch (normCost)
            {
                case "a":
                    TurnManager.currentUnit.GetComponent<Character>().normalAction -= 1; // Subtract an action from player
                    break;

                case "b":
                    TurnManager.currentUnit.GetComponent<Character>().bonusAction -= 1; // Subtract an bonus action from player
                    break;

                case "r":
                    TurnManager.currentUnit.GetComponent<Character>().reAction -= 1; // Subtract an reaction from player
                    break;

                default:
                    Debug.LogError("Invalid normCost in DoSingleAttack()");
                    break;
            }

            UIManager.inst.UpdateHUD(); // Update the HUD

            foreach(GameObject go in viableTargets) // This is going to hurt!
            {
                CreateCombatEvent(TurnManager.currentUnit.GetComponent<Character>(), go.GetComponent<Character>(), attType, attackNum, vantage);
                doOnce = true;
            }

            doneAttacking = true;
        }
        else
        {
            doneAttacking = false;
            targetSelection = true;
        }
    }

    public void DoSingleAttack(int attackNum, string attType, int vantage, string normCost, int spellCost, int toInclude = 0, int minmaxRange = 0) // Used by player characters
    {
        if (Input.GetMouseButtonUp(0)) // 0 = Left-click | 1 - Right-click
        {

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                switch (toInclude)
                {
                    case 0: // Enemies only
                        if (hit.collider.tag == "NPC") // ATTACK!
                        {
                            targetSelection = false;
                            TacticsMove t = hit.collider.GetComponent<TacticsMove>();

                            float distance = CalculateDistance(TurnManager.currentUnit.gameObject, t.gameObject);

                            if (attType == "ranged" && distance > minmaxRange)
                            {
                                vantage = 2; // Outside normal range so apply disadvantage.
                            }

                            TurnManager.currentUnit.attacking = true; // Player is attacking (animate or something)

                            if(spellCost == 20) // Don't remove a spellslot
                            {

                            }
                            else
                            {
                                TurnManager.currentUnit.GetComponent<Character>().spellSlots[spellCost] -= 1; // Remove the spellslot
                            }

                            switch (normCost)
                            {
                                case "a":
                                    TurnManager.currentUnit.GetComponent<Character>().normalAction -= 1; // Subtract an action from player
                                    break;

                                case "b":
                                    TurnManager.currentUnit.GetComponent<Character>().bonusAction -= 1; // Subtract an bonus action from player
                                    break;  

                                case "r":
                                    TurnManager.currentUnit.GetComponent<Character>().reAction -= 1; // Subtract an reaction from player
                                    break;

                                case "f":
                                    // Free action, no subtract
                                    break;

                                default:
                                    Debug.LogError("Invalid normCost in DoSingleAttack()");
                                    break;
                            }
                            UIManager.inst.UpdateHUD(); // Update the HUD

                            CreateCombatEvent(TurnManager.currentUnit.GetComponent<Character>(), t.GetComponent<Character>(), attType, attackNum, vantage);
                            doneAttacking = true;
                        }
                        break;

                    case 1: // Players only
                        if (hit.collider.tag == "Player") // ATTACK!
                        {
                            targetSelection = false;
                            TacticsMove t = hit.collider.GetComponent<TacticsMove>();

                            float distance = CalculateDistance(TurnManager.currentUnit.gameObject, t.gameObject);

                            if (attType == "ranged" && distance > minmaxRange)
                            {
                                vantage = 2; // Outside normal range so apply disadvantage.
                            }

                            TurnManager.currentUnit.attacking = true; // Player is attacking (animate or something)
                            if (spellCost == 20) // Don't remove a spellslot
                            {

                            }
                            else
                            {
                                TurnManager.currentUnit.GetComponent<Character>().spellSlots[spellCost] -= 1; // Remove the spellslot
                            }

                            switch (normCost)
                            {
                                case "a":
                                    TurnManager.currentUnit.GetComponent<Character>().normalAction -= 1; // Subtract an action from player
                                    break;

                                case "b":
                                    TurnManager.currentUnit.GetComponent<Character>().bonusAction -= 1; // Subtract an bonus action from player
                                    break;

                                case "r":
                                    TurnManager.currentUnit.GetComponent<Character>().reAction -= 1; // Subtract an reaction from player
                                    break;

                                case "f":
                                    // Free action, no subtract
                                    break;

                                default:
                                    Debug.LogError("Invalid normCost in DoSingleAttack()");
                                    break;
                            }
                            UIManager.inst.UpdateHUD(); // Update the HUD

                            CreateCombatEvent(TurnManager.currentUnit.GetComponent<Character>(), t.GetComponent<Character>(), attType, attackNum, vantage);
                            doneAttacking = true;
                        }
                        break;

                    case 2: // Include all
                        if (hit.collider.tag == "NPC" || hit.collider.tag == "Player") // ATTACK!
                        {
                            targetSelection = false;
                            TacticsMove t = hit.collider.GetComponent<TacticsMove>();

                            float distance = CalculateDistance(TurnManager.currentUnit.gameObject, t.gameObject);

                            if (attType == "ranged" && distance > minmaxRange)
                            {
                                vantage = 2; // Outside normal range so apply disadvantage.
                            }

                            TurnManager.currentUnit.attacking = true; // Player is attacking (animate or something)
                            if (spellCost == 20) // Don't remove a spellslot
                            {

                            }
                            else
                            {
                                TurnManager.currentUnit.GetComponent<Character>().spellSlots[spellCost] -= 1; // Remove the spellslot
                            }

                            switch (normCost)
                            {
                                case "a":
                                    TurnManager.currentUnit.GetComponent<Character>().normalAction -= 1; // Subtract an action from player
                                    break;

                                case "b":
                                    TurnManager.currentUnit.GetComponent<Character>().bonusAction -= 1; // Subtract an bonus action from player
                                    break;

                                case "r":
                                    TurnManager.currentUnit.GetComponent<Character>().reAction -= 1; // Subtract an reaction from player
                                    break;

                                case "f":
                                    // Free action, no subtract
                                    break;

                                default:
                                    Debug.LogError("Invalid normCost in DoSingleAttack()");
                                    break;
                            }
                            UIManager.inst.UpdateHUD(); // Update the HUD

                            CreateCombatEvent(TurnManager.currentUnit.GetComponent<Character>(), t.GetComponent<Character>(), attType, attackNum, vantage);
                            doneAttacking = true;
                        }
                        break;

                    default:

                        break;
                }
            }
        }
        else
        {
            doneAttacking = false;
            targetSelection = true;
        }
    }

    bool CheckLineOfSight(GameObject unit) // Check line of sight to target
    {
        RaycastHit hit;
        Vector3 direction = unit.transform.position - TurnManager.currentUnit.transform.position;

        if (Physics.Raycast(TurnManager.currentUnit.transform.position, direction, out hit))
        {
            if (hit.transform == unit.transform)
            {
                // Successs! (Continue)
                Debug.DrawRay(TurnManager.currentUnit.transform.position, direction, Color.red);
                return true;
            }
            else
            {
                return false; // Failed, no line of sight
            }
        }

        return false;
    }

    /// <summary>
    /// Multi-facited function for doing a melee attack.
    /// </summary>
    /// <param name="attacker">Who is doing the attack?</param>
    /// <param name="defender">Who is being attacked?</param>
    /// <param name="vantage">Does the character performing this attack have advantage, disadvantage, or none?</param>
    /// <param name="modifier">Which stat modifier bonus is this attack using? (From character)</param>
    /// <param name="dmgType">What type of damage is this?</param>
    /// <param name="diceAmount">How many primary dice are being used?</param>
    /// <param name="diceSize">What are the size of these dice?</param>
    /// <param name="diceAdd">Is there a piece of bonus damage being added on to this?</param>
    /// <param name="projType">The type of projectile to spawn.</param>
    /// <param name="icon">The icon to be displayed in the activity log.</param>
    /// <param name="xdA">Is there a different type of die being rolled as well? How many?</param>
    /// <param name="xdS">How big is this other die?</param>
    public void MeleeFunc(Character attacker, Character defender, int vantage, int modifier, string dmgType, int diceAmount, int diceSize, int diceAdd, string icon = "obsolete", int xdA = 0, int xdS = 0)
    {
        attackRoll = RollWithVantage(attacker, vantage, modifier);

        PlayerCheck(attacker, defender);

        if (attackRoll >= defender.armorClass) // Hit!
        {
            for(int i = 0; i < diceAmount; i++) // Roll for the dice damage
            {
                switch (diceSize)
                {
                    case 4:
                        damageDone += DiceRoll.inst.RollD4();
                        break;

                    case 6:
                        damageDone += DiceRoll.inst.RollD6();
                        break;

                    case 8:
                        damageDone += DiceRoll.inst.RollD8();
                        break;

                    case 10:
                        damageDone += DiceRoll.inst.RollD10();
                        break;

                    case 12:
                        damageDone += DiceRoll.inst.RollD12();
                        break;

                    case 20:
                        damageDone += DiceRoll.inst.RollD20();
                        break;

                    case 100:
                        damageDone += DiceRoll.inst.RollD100();
                        break;

                    default:
                        damageDone += DiceRoll.inst.RollDX(diceSize);
                        break;

                }
            }

            if(xdA > 0) // Roll for the extra dice size
            {
                for (int i = 0; i < xdA; i++)
                {
                    switch (xdS)
                    {
                        case 4:
                            damageDone += DiceRoll.inst.RollD4();
                            break;

                        case 6:
                            damageDone += DiceRoll.inst.RollD6();
                            break;

                        case 8:
                            damageDone += DiceRoll.inst.RollD8();
                            break;

                        case 10:
                            damageDone += DiceRoll.inst.RollD10();
                            break;

                        case 12:
                            damageDone += DiceRoll.inst.RollD12();
                            break;

                        case 20:
                            damageDone += DiceRoll.inst.RollD20();
                            break;

                        case 100:
                            damageDone += DiceRoll.inst.RollD100();
                            break;

                        default:
                            damageDone += DiceRoll.inst.RollDX(xdS);
                            break;

                    }
                }
            }

            damageDone += modifier + diceAdd;

            if (defender.resistances.Contains(dmgType))
            {
                damageDone = damageDone / 2;
            }
            defender.hitPoints -= damageDone;
            outcome = true;
            passColor = Color.green;
            floatingString = "Hit! [" + attackRoll + "], did [" + damageDone + "] damage.";
            tooltipReadout = attacker.characterName + " hit " + defender.characterName + " rolling a " + attackRoll + " dealing " + damageDone + " " + dmgType + " damage.";
            // Play an animation
            int random = Random.Range(6, 8);
            PlayAnimation(random, attacker.GetComponent<TacticsMove>());
            PlayAnimation(1, defender.GetComponent<TacticsMove>());
            GameObject tempSound = Instantiate(tempSoundPref, attacker.transform.position, Quaternion.identity);
            SoundManager.inst.PlayMeleeSound(true, tempSound.GetComponent<AudioSource>());
            StartCoroutine(DestroyTempSound(tempSound));
        }
        else // Miss.
        {
            outcome = false;
            passColor = Color.red;
            floatingString = "Miss. [" + attackRoll + "] failed to hit";
            tooltipReadout = attacker.characterName + " missed " + defender.characterName + " rolling a " + attackRoll + ".";

            GameObject tempSound = Instantiate(tempSoundPref, attacker.transform.position, Quaternion.identity);
            SoundManager.inst.PlayMeleeSound(false, tempSound.GetComponent<AudioSource>());
            StartCoroutine(DestroyTempSound(tempSound));
            icon = "icon_miss";
            // Play an animation
            int random = Random.Range(6, 8);
            PlayAnimation(random, attacker.GetComponent<TacticsMove>());
        }

        UIManager.inst.UpdateHUD(); // For healthbar
        UIManager.inst.DoFloatingText(attacker.GetComponent<Transform>().gameObject, floatingString, passColor);
        ActivityLog.inst.NewAlert(attacker.characterName, defender.characterName, attackRoll, icon, attacker.gameObject, tooltipReadout);
    }

    public void RangedFunc(Character attacker, Character defender, int vantage, int modifier, string dmgType, int diceAmount, int diceSize, int diceAdd, int projType, string icon = "obsolete", int xdA = 0, int xdS = 0)
    {
        bool missTarget = false; ;

        attackRoll = RollWithVantage(attacker, vantage, modifier);

        PlayerCheck(attacker, defender);

        if (attackRoll >= defender.armorClass) // Hit!
        {
            for (int i = 0; i < diceAmount; i++) // Roll for the dice damage
            {
                switch (diceSize)
                {
                    case 4:
                        damageDone += DiceRoll.inst.RollD4();
                        break;

                    case 6:
                        damageDone += DiceRoll.inst.RollD6();
                        break;

                    case 8:
                        damageDone += DiceRoll.inst.RollD8();
                        break;

                    case 10:
                        damageDone += DiceRoll.inst.RollD10();
                        break;

                    case 12:
                        damageDone += DiceRoll.inst.RollD12();
                        break;

                    case 20:
                        damageDone += DiceRoll.inst.RollD20();
                        break;

                    case 100:
                        damageDone += DiceRoll.inst.RollD100();
                        break;

                    default:
                        damageDone += DiceRoll.inst.RollDX(diceSize);
                        break;

                }
            }

            if (xdA > 0) // Roll for the extra dice size
            {
                for (int i = 0; i < xdA; i++)
                {
                    switch (xdS)
                    {
                        case 4:
                            damageDone += DiceRoll.inst.RollD4();
                            break;

                        case 6:
                            damageDone += DiceRoll.inst.RollD6();
                            break;

                        case 8:
                            damageDone += DiceRoll.inst.RollD8();
                            break;

                        case 10:
                            damageDone += DiceRoll.inst.RollD10();
                            break;

                        case 12:
                            damageDone += DiceRoll.inst.RollD12();
                            break;

                        case 20:
                            damageDone += DiceRoll.inst.RollD20();
                            break;

                        case 100:
                            damageDone += DiceRoll.inst.RollD100();
                            break;

                        default:
                            damageDone += DiceRoll.inst.RollDX(xdS);
                            break;

                    }
                }
            }

            damageDone += modifier + diceAdd;

            if (defender.resistances.Contains(dmgType))
            {
                damageDone = damageDone / 2;
            }
            defender.hitPoints -= damageDone;
            outcome = true;
            passColor = Color.green;
            floatingString = "Hit! [" + attackRoll + "], did [" + damageDone + "] damage.";
            tooltipReadout = attacker.characterName + " hit " + defender.characterName + " rolling a " + attackRoll + " dealing " + damageDone + " " + dmgType + " damage.";
            // Play an animation

            GameObject tempSound = Instantiate(tempSoundPref, defender.transform.position, Quaternion.identity);
            SoundManager.inst.PlayRangedSound(true, icon, tempSound.GetComponent<AudioSource>());
            StartCoroutine(DestroyTempSound(tempSound));
        }
        else // Miss.
        {
            outcome = false;
            passColor = Color.red;
            floatingString = "Miss. [" + attackRoll + "] failed to hit";
            tooltipReadout = attacker.characterName + " missed " + defender.characterName + " rolling a " + attackRoll + ".";

            GameObject tempSound = Instantiate(tempSoundPref, defender.transform.position, Quaternion.identity);
            SoundManager.inst.PlayRangedSound(false, icon, tempSound.GetComponent<AudioSource>());
            StartCoroutine(DestroyTempSound(tempSound));
            icon = "icon_miss";
            missTarget = true;
            // Play an animation
        }

        SFXManager.inst.SetEffectToSpawn(projType);
        SFXManager.inst.startPoint = attacker.gameObject;
        SFXManager.inst.targetPoint = defender.gameObject;
        if(attacker.tag == "NPC")
        {
            SFXManager.inst.SpawnVFX(missTarget, "Player");
        }
        else
        {
            SFXManager.inst.SpawnVFX(missTarget, "NPC");
        }

        UIManager.inst.UpdateHUD(); // For healthbar
        UIManager.inst.DoFloatingText(attacker.GetComponent<Transform>().gameObject, floatingString, passColor);
        ActivityLog.inst.NewAlert(attacker.characterName, defender.characterName, attackRoll, icon, attacker.gameObject, tooltipReadout);
    }

    /// <summary>
    /// Multi-facited function for doing a spell attack.
    /// </summary>
    /// <param name="attacker">Who is doing the attack?</param>
    /// <param name="defender">Who is being attacked?</param>
    /// <param name="vantage">Does the character performing this attack have advantage, disadvantage, or none?</param>
    /// <param name="modifier">What is the attacker's spell attack modifier?</param>
    /// <param name="dmgType">What type of damage is this?</param>
    /// <param name="diceAmount">How many primary dice are being used?</param>
    /// <param name="diceSize">What are the size of these dice?</param>
    /// <param name="diceAdd">Is there a piece of bonus damage being added on to this?</param>
    /// <param name="projType">The type of projectile to spawn.</param>
    /// <param name="projColor">The color of the projectile to spawn.</param>
    /// <param name="saveOrSuck">Is this spell like a normal attack (0), a half save or suck (1, defender does the rolling), or a full save or suck (2, defender does the rolling, success = no effect at all)</param>
    /// <param name="icon">The icon to be displayed in the activity log.</param>
    /// <param name="xdA">Is there a different type of die being rolled as well? How many?</param>
    /// <param name="xdS">How big is this other die?</param>
    public IEnumerator SpellFunc(Character attacker, Character defender, int vantage, int modifier, string dmgType, int diceAmount, int diceSize, int diceAdd, int projType, string projColor, int saveOrSuck, string icon = "obsolete", int xMod = 0, int xdA = 0, int xdS = 0)
    {
        bool missTarget = false;
        bool distVantage = closeQuartersVantage(attacker);
        if (distVantage)
        {
            vantage = 2;
        }

        attackRoll = RollWithVantage(attacker, vantage, modifier);

        PlayerCheck(attacker, defender);

        if(saveOrSuck == 0) // Normal attack
        {
            if (attackRoll >= defender.armorClass) // Hit!
            {
                for (int i = 0; i < diceAmount; i++) // Roll for the dice damage
                {
                    switch (diceSize)
                    {
                        case 4:
                            damageDone += DiceRoll.inst.RollD4();
                            break;

                        case 6:
                            damageDone += DiceRoll.inst.RollD6();
                            break;

                        case 8:
                            damageDone += DiceRoll.inst.RollD8();
                            break;

                        case 10:
                            damageDone += DiceRoll.inst.RollD10();
                            break;

                        case 12:
                            damageDone += DiceRoll.inst.RollD12();
                            break;

                        case 20:
                            damageDone += DiceRoll.inst.RollD20();
                            break;

                        case 100:
                            damageDone += DiceRoll.inst.RollD100();
                            break;

                        default:
                            damageDone += DiceRoll.inst.RollDX(diceSize);
                            break;

                    }
                }

                if (xdA > 0) // Roll for the extra dice size
                {
                    for (int i = 0; i < xdA; i++)
                    {
                        switch (xdS)
                        {
                            case 4:
                                damageDone += DiceRoll.inst.RollD4();
                                break;

                            case 6:
                                damageDone += DiceRoll.inst.RollD6();
                                break;

                            case 8:
                                damageDone += DiceRoll.inst.RollD8();
                                break;

                            case 10:
                                damageDone += DiceRoll.inst.RollD10();
                                break;

                            case 12:
                                damageDone += DiceRoll.inst.RollD12();
                                break;

                            case 20:
                                damageDone += DiceRoll.inst.RollD20();
                                break;

                            case 100:
                                damageDone += DiceRoll.inst.RollD100();
                                break;

                            default:
                                damageDone += DiceRoll.inst.RollDX(xdS);
                                break;

                        }
                    }
                }

                damageDone += modifier + diceAdd;

                if (defender.resistances.Contains(dmgType))
                {
                    damageDone = damageDone / 2;
                }
                defender.hitPoints -= damageDone;
                outcome = true;
                passColor = Color.green;
                floatingString = "Hit! [" + attackRoll + "], did [" + damageDone + "] damage.";
                tooltipReadout = attacker.characterName + " hit " + defender.characterName + " rolling a " + attackRoll + " dealing " + damageDone + " " + dmgType + " damage.";
                // Play an animation
                int random = Random.Range(2, 5);
                PlayAnimation(random, attacker.GetComponent<TacticsMove>());
                PlayAnimation(1, defender.GetComponent<TacticsMove>());

                GameObject tempSound = Instantiate(tempSoundPref, defender.transform.position, Quaternion.identity);
                SoundManager.inst.PlayRangedSound(true, icon, tempSound.GetComponent<AudioSource>());
                StartCoroutine(DestroyTempSound(tempSound));

            }
            else // Miss.
            {
                outcome = false;
                passColor = Color.red;
                floatingString = "Miss. [" + attackRoll + "] failed to hit";
                tooltipReadout = attacker.characterName + " missed " + defender.characterName + " rolling a " + attackRoll + ".";


                GameObject tempSound = Instantiate(tempSoundPref, defender.transform.position, Quaternion.identity);
                SoundManager.inst.PlayRangedSound(false, icon, tempSound.GetComponent<AudioSource>());
                StartCoroutine(DestroyTempSound(tempSound));


                icon = "icon_miss";
                missTarget = true;
                // Play an animation
                int random = Random.Range(2, 5);
                PlayAnimation(random, attacker.GetComponent<TacticsMove>());
            }
        }
        else if(saveOrSuck == 1) // Half damage save or suck
        {
            if (defender.debuffs.Contains(12)) // Bane
            {
                xMod = -1 * DiceRoll.inst.RollD4(); // -1d4
            }

            if (defender.debuffs.Contains(8)) // Bless
            {
                xMod = DiceRoll.inst.RollD4(); // 1d4
            }

            saveRoll = SaveOrSuckRoll(defender, vantage, xMod);

            PlayerCheck(attacker, defender);

            if (saveRoll >= modifier) // Made the save, half damage
            {
                for (int i = 0; i < diceAmount; i++) // Roll for the dice damage
                {
                    switch (diceSize)
                    {
                        case 4:
                            damageDone += DiceRoll.inst.RollD4();
                            break;

                        case 6:
                            damageDone += DiceRoll.inst.RollD6();
                            break;

                        case 8:
                            damageDone += DiceRoll.inst.RollD8();
                            break;

                        case 10:
                            damageDone += DiceRoll.inst.RollD10();
                            break;

                        case 12:
                            damageDone += DiceRoll.inst.RollD12();
                            break;

                        case 20:
                            damageDone += DiceRoll.inst.RollD20();
                            break;

                        case 100:
                            damageDone += DiceRoll.inst.RollD100();
                            break;

                        default:
                            damageDone += DiceRoll.inst.RollDX(diceSize);
                            break;

                    }
                }

                if (xdA > 0) // Roll for the extra dice size
                {
                    for (int i = 0; i < xdA; i++)
                    {
                        switch (xdS)
                        {
                            case 4:
                                damageDone += DiceRoll.inst.RollD4();
                                break;

                            case 6:
                                damageDone += DiceRoll.inst.RollD6();
                                break;

                            case 8:
                                damageDone += DiceRoll.inst.RollD8();
                                break;

                            case 10:
                                damageDone += DiceRoll.inst.RollD10();
                                break;

                            case 12:
                                damageDone += DiceRoll.inst.RollD12();
                                break;

                            case 20:
                                damageDone += DiceRoll.inst.RollD20();
                                break;

                            case 100:
                                damageDone += DiceRoll.inst.RollD100();
                                break;

                            default:
                                damageDone += DiceRoll.inst.RollDX(xdS);
                                break;

                        }
                    }
                }

                damageDone += diceAdd;

                damageDone = damageDone / 2; // Made the save so half damage.

                if (defender.resistances.Contains(dmgType))
                {
                    damageDone = damageDone / 2;
                }

                SFXManager.inst.SetEffectToSpawn(projType);
                SFXManager.inst.startPoint = attacker.gameObject;
                SFXManager.inst.targetPoint = defender.gameObject;
                if (attacker.tag == "NPC")
                {
                    SFXManager.inst.SpawnVFX(missTarget, "Player", icon, projColor);
                }
                else
                {
                    SFXManager.inst.SpawnVFX(missTarget, "NPC", icon, projColor);
                }

                while (!targetReached)
                {
                    yield return null;
                }

                yield return new WaitForSeconds(timeToWait);

                defender.hitPoints -= damageDone;
                outcome = true;
                passColor = Color.green;
                floatingString = "Save! [" + saveRoll + "], took [" + damageDone + "] " + dmgType + " damage.";
                tooltipReadout = defender.characterName + " took half of " + damageDone * 2 + " " + dmgType + " damage (" + damageDone + ") due to making the save.";
                // Play an animation
                int random = Random.Range(2, 5);
                PlayAnimation(random, attacker.GetComponent<TacticsMove>());
                PlayAnimation(1, defender.GetComponent<TacticsMove>());

                GameObject tempSound = Instantiate(tempSoundPref, defender.transform.position, Quaternion.identity);
                SoundManager.inst.PlayMagicSound(true, icon, tempSound.GetComponent<AudioSource>());
                StartCoroutine(DestroyTempSound(tempSound));

            }
            else // Failed the save! Full damage.
            {
                for (int i = 0; i < diceAmount; i++) // Roll for the dice damage
                {
                    switch (diceSize)
                    {
                        case 4:
                            damageDone += DiceRoll.inst.RollD4();
                            break;

                        case 6:
                            damageDone += DiceRoll.inst.RollD6();
                            break;

                        case 8:
                            damageDone += DiceRoll.inst.RollD8();
                            break;

                        case 10:
                            damageDone += DiceRoll.inst.RollD10();
                            break;

                        case 12:
                            damageDone += DiceRoll.inst.RollD12();
                            break;

                        case 20:
                            damageDone += DiceRoll.inst.RollD20();
                            break;

                        case 100:
                            damageDone += DiceRoll.inst.RollD100();
                            break;

                        default:
                            damageDone += DiceRoll.inst.RollDX(diceSize);
                            break;

                    }
                }

                if (xdA > 0) // Roll for the extra dice size
                {
                    for (int i = 0; i < xdA; i++)
                    {
                        switch (xdS)
                        {
                            case 4:
                                damageDone += DiceRoll.inst.RollD4();
                                break;

                            case 6:
                                damageDone += DiceRoll.inst.RollD6();
                                break;

                            case 8:
                                damageDone += DiceRoll.inst.RollD8();
                                break;

                            case 10:
                                damageDone += DiceRoll.inst.RollD10();
                                break;

                            case 12:
                                damageDone += DiceRoll.inst.RollD12();
                                break;

                            case 20:
                                damageDone += DiceRoll.inst.RollD20();
                                break;

                            case 100:
                                damageDone += DiceRoll.inst.RollD100();
                                break;

                            default:
                                damageDone += DiceRoll.inst.RollDX(xdS);
                                break;

                        }
                    }
                }

                damageDone += diceAdd;

                if (defender.resistances.Contains(dmgType))
                {
                    damageDone = damageDone / 2;
                }

                SFXManager.inst.SetEffectToSpawn(projType);
                SFXManager.inst.startPoint = attacker.gameObject;
                SFXManager.inst.targetPoint = defender.gameObject;
                if (attacker.tag == "NPC")
                {
                    SFXManager.inst.SpawnVFX(missTarget, "Player", icon, projColor);
                }
                else
                {
                    SFXManager.inst.SpawnVFX(missTarget, "NPC", icon, projColor);
                }

                while (!targetReached)
                {
                    yield return null;
                }

                yield return new WaitForSeconds(timeToWait);

                defender.hitPoints -= damageDone;

                outcome = true; // ?
                passColor = Color.red;
                floatingString = "Failed! [" + saveRoll + "], took [" + damageDone + "] " + dmgType + " damage.";
                tooltipReadout = defender.characterName + " failed to save, taking " + damageDone + " " + dmgType + " damage.";
                // Play an animation
                int random = Random.Range(2, 5);
                PlayAnimation(random, attacker.GetComponent<TacticsMove>());
                PlayAnimation(1, defender.GetComponent<TacticsMove>());

                GameObject tempSound = Instantiate(tempSoundPref, defender.transform.position, Quaternion.identity);
                SoundManager.inst.PlayMagicSound(true, icon, tempSound.GetComponent<AudioSource>());
                StartCoroutine(DestroyTempSound(tempSound));

            }
        }
        else if(saveOrSuck == 2) // Effect or no effect save or suck
        {

        }
        else
        {
            Debug.LogError("Incorrect 'Save or Suck' input inside SpellFunc");
        }

        UIManager.inst.UpdateHUD(); // For healthbar
        UIManager.inst.DoFloatingText(defender.GetComponent<Transform>().gameObject, floatingString, passColor);
        ActivityLog.inst.NewAlert(attacker.characterName, defender.characterName, attackRoll, icon, attacker.gameObject, tooltipReadout);
    }

    /// <summary>
    /// Multi-facited function for doing an effect.
    /// </summary>
    public IEnumerator EffectFunc(Character attacker, Character defender, int vantage, int effectToApply, int modifier, string dmgType, int diceAmount, int diceSize, int diceAdd, int projType, string projColor, int saveOrSuck, int intent, string icon = "obsolete", int xMod = 0, int xdA = 0, int xdS = 0)
    {
        
        bool missTarget = false;

        if (intent == 0) // Harmful
        {
            if (effectToApply == 0) // No effect, just damage
            {

            }
            else // Effect
            {
                if(saveOrSuck == 0) // Regular
                {

                }
                else if(saveOrSuck == 1) // Half damage
                {
                    saveRoll = SaveOrSuckRoll(defender, vantage, xMod);

                    PlayerCheck(attacker, defender);

                    if (saveRoll >= modifier) // Made the save, half damage
                    {

                        for (int i = 0; i < diceAmount; i++) // Roll for the dice damage
                        {
                            switch (diceSize)
                            {
                                case 4:
                                    damageDone += DiceRoll.inst.RollD4();
                                    break;

                                case 6:
                                    damageDone += DiceRoll.inst.RollD6();
                                    break;

                                case 8:
                                    damageDone += DiceRoll.inst.RollD8();
                                    Debug.Log("Rolled");
                                    break;

                                case 10:
                                    damageDone += DiceRoll.inst.RollD10();
                                    break;

                                case 12:
                                    damageDone += DiceRoll.inst.RollD12();
                                    break;

                                case 20:
                                    damageDone += DiceRoll.inst.RollD20();
                                    break;

                                case 100:
                                    damageDone += DiceRoll.inst.RollD100();
                                    break;

                                default:
                                    damageDone += DiceRoll.inst.RollDX(diceSize);
                                    break;

                            }
                        }

                        if (xdA > 0) // Roll for the extra dice size
                        {
                            for (int i = 0; i < xdA; i++)
                            {
                                switch (xdS)
                                {
                                    case 4:
                                        damageDone += DiceRoll.inst.RollD4();
                                        break;

                                    case 6:
                                        damageDone += DiceRoll.inst.RollD6();
                                        break;

                                    case 8:
                                        damageDone += DiceRoll.inst.RollD8();
                                        break;

                                    case 10:
                                        damageDone += DiceRoll.inst.RollD10();
                                        break;

                                    case 12:
                                        damageDone += DiceRoll.inst.RollD12();
                                        break;

                                    case 20:
                                        damageDone += DiceRoll.inst.RollD20();
                                        break;

                                    case 100:
                                        damageDone += DiceRoll.inst.RollD100();
                                        break;

                                    default:
                                        damageDone += DiceRoll.inst.RollDX(xdS);
                                        break;

                                }
                            }
                        }

                        damageDone += diceAdd;

                        damageDone = damageDone / 2; // Made the save so half damage.

                        if (defender.resistances.Contains(dmgType))
                        {
                            damageDone = damageDone / 2;
                        }

                        SFXManager.inst.SetEffectToSpawn(projType);
                        SFXManager.inst.startPoint = attacker.gameObject;
                        SFXManager.inst.targetPoint = defender.gameObject;
                        if (attacker.tag == "NPC")
                        {
                            SFXManager.inst.SpawnVFX(missTarget, "Player", icon, projColor);
                        }
                        else
                        {
                            SFXManager.inst.SpawnVFX(missTarget, "NPC", icon, projColor);
                        }

                        while (!targetReached)
                        {
                            yield return null;
                        }

                        yield return new WaitForSeconds(timeToWait);

                        defender.hitPoints -= damageDone;
                        outcome = true;
                        passColor = Color.green;
                        floatingString = "Save! [" + saveRoll + "], took [" + damageDone + "] " + dmgType + " damage.";
                        tooltipReadout = defender.characterName + " took half of " + damageDone * 2 + " " + dmgType + " damage (" + damageDone + ") due to making the save.";
                        // Play an animation
                        GameObject tempSound = Instantiate(tempSoundPref, defender.transform.position, Quaternion.identity);
                        SoundManager.inst.PlayMagicSound(true, icon, tempSound.GetComponent<AudioSource>());
                        StartCoroutine(DestroyTempSound(tempSound));

                    }
                }
                else // None or all
                {
                    if (defender.debuffs.Contains(12)) // Bane
                    {
                        xMod = -1 * DiceRoll.inst.RollD4(); // -1d4
                    }

                    if (defender.debuffs.Contains(8)) // Bless
                    {
                        xMod = DiceRoll.inst.RollD4(); // 1d4
                    }

                    saveRoll = SaveOrSuckRoll(defender, vantage, xMod);

                    PlayerCheck(attacker, defender);

                    if (saveRoll >= modifier) // Made the save, no effect
                    {

                        outcome = true;
                        passColor = Color.green;
                        floatingString = "Save! [" + saveRoll + "]";
                        tooltipReadout = defender.characterName + " rolled a " + saveRoll + " making the save and resisting the effect.";
                        // Play an animation
                        int random = Random.Range(2, 5);
                        PlayAnimation(random, attacker.GetComponent<TacticsMove>());

                        if (!doOnce)
                        {
                            GameObject tempSound = Instantiate(tempSoundPref, defender.transform.position, Quaternion.identity);
                            SoundManager.inst.PlayMagicSound(true, icon, tempSound.GetComponent<AudioSource>());
                            StartCoroutine(DestroyTempSound(tempSound));
                        }

                    }
                    else // Failed the save! Apply effect
                    {
                        defender.debuffs.Add(effectToApply);

                        floatingString = "+ " + this.GetComponent<DirectAbilities>().abilityNames[effectToApply] + " +";
                        tooltipReadout = attacker.characterName + " applied the debuff " + this.GetComponent<DirectAbilities>().abilityNames[effectToApply] + " to " + defender.characterName + " .";

                        GameObject tempSound = Instantiate(tempSoundPref, defender.transform.position, Quaternion.identity);
                        SoundManager.inst.PlayMagicSound(true, icon, tempSound.GetComponent<AudioSource>());
                        StartCoroutine(DestroyTempSound(tempSound));

                        UIManager.inst.UpdateHUD(); // For healthbar
                        UIManager.inst.DoFloatingText(defender.GetComponent<Transform>().gameObject, floatingString, passColor);
                        ActivityLog.inst.NewAlert(attacker.characterName, defender.characterName, attackRoll, icon, attacker.gameObject, tooltipReadout);

                        int random = Random.Range(2, 5);
                        PlayAnimation(random, attacker.GetComponent<TacticsMove>());
                        PlayAnimation(1, defender.GetComponent<TacticsMove>());
                    }
                }
            }


        }
        else if(intent == 1) // Helpful
        {

            if(effectToApply == 0) // No effect, just healing
            {
                for (int i = 0; i < diceAmount; i++) // Roll for the dice healing
                {
                    switch (diceSize)
                    {
                        case 4:
                            damageDone += DiceRoll.inst.RollD4();
                            break;

                        case 6:
                            damageDone += DiceRoll.inst.RollD6();
                            break;

                        case 8:
                            damageDone += DiceRoll.inst.RollD8();
                            break;

                        case 10:
                            damageDone += DiceRoll.inst.RollD10();
                            break;

                        case 12:
                            damageDone += DiceRoll.inst.RollD12();
                            break;

                        case 20:
                            damageDone += DiceRoll.inst.RollD20();
                            break;

                        case 100:
                            damageDone += DiceRoll.inst.RollD100();
                            break;

                        default:
                            damageDone += DiceRoll.inst.RollDX(diceSize);
                            break;

                    }
                }

                if (xdA > 0) // Roll for the extra dice size
                {
                    for (int i = 0; i < xdA; i++)
                    {
                        switch (xdS)
                        {
                            case 4:
                                damageDone += DiceRoll.inst.RollD4();
                                break;

                            case 6:
                                damageDone += DiceRoll.inst.RollD6();
                                break;

                            case 8:
                                damageDone += DiceRoll.inst.RollD8();
                                break;

                            case 10:
                                damageDone += DiceRoll.inst.RollD10();
                                break;

                            case 12:
                                damageDone += DiceRoll.inst.RollD12();
                                break;

                            case 20:
                                damageDone += DiceRoll.inst.RollD20();
                                break;

                            case 100:
                                damageDone += DiceRoll.inst.RollD100();
                                break;

                            default:
                                damageDone += DiceRoll.inst.RollDX(xdS);
                                break;

                        }
                    }
                }

                damageDone += modifier + diceAdd;

                defender.hitPoints += damageDone;

                if(defender.hitPoints > defender.maxHealth) // No overheal
                {
                    defender.hitPoints = defender.maxHealth;
                }

                outcome = true;
                passColor = Color.blue;
                floatingString = "+ Healed: " + damageDone + " +";
                tooltipReadout = attacker.characterName + " healed " + defender.characterName + " for " + damageDone + " HP.";
                // Play an animation

                GameObject tempSound = Instantiate(tempSoundPref, defender.transform.position, Quaternion.identity);
                SoundManager.inst.PlayMagicSound(true, icon, tempSound.GetComponent<AudioSource>());
                StartCoroutine(DestroyTempSound(tempSound));

                UIManager.inst.UpdateHUD(); // For healthbar
                UIManager.inst.DoFloatingText(defender.GetComponent<Transform>().gameObject, floatingString, passColor);
                ActivityLog.inst.NewAlert(attacker.characterName, defender.characterName, damageDone, icon, defender.gameObject, tooltipReadout);

            }
            else // Apply an effect
            {
                defender.buffs.Add(effectToApply);
                if(effectToApply == 17)
                {
                    defender.armorClass += 2;
                }
                
                if(effectToApply == 18)
                {
                    defender.armorClass += 5;
                }

                floatingString = "+ " + this.GetComponent<DirectAbilities>().abilityNames[effectToApply] + " +";
                tooltipReadout = attacker.characterName + " applied " + this.GetComponent<DirectAbilities>().abilityNames[effectToApply] + " to " + defender.characterName + " .";

                GameObject tempSound = Instantiate(tempSoundPref, defender.transform.position, Quaternion.identity);
                SoundManager.inst.PlayMagicSound(true, icon, tempSound.GetComponent<AudioSource>());
                StartCoroutine(DestroyTempSound(tempSound));

                UIManager.inst.UpdateHUD(); // For healthbar
                UIManager.inst.DoFloatingText(defender.GetComponent<Transform>().gameObject, floatingString, passColor);
                ActivityLog.inst.NewAlert(attacker.characterName, defender.characterName, attackRoll, icon, attacker.gameObject, tooltipReadout);

                int random = Random.Range(2, 5);
                PlayAnimation(random, attacker.GetComponent<TacticsMove>());
            }


        }
        else
        {
            Debug.LogError("Invalid intent in EffectFunc()");
        }
        
    }


    public bool closeQuartersVantage(Character attacker) // If you are doing a ranged attack next to an enemy you automatically get disadvantage.
    {
        if (TurnManager.IsPlayer(attacker.GetComponent<TacticsMove>())) // For players
        {
            foreach(GameObject unit in TurnManager.allEnemies)
            {
                int distance = CalculateDistance(attacker.gameObject, unit);
                if(distance == 1)
                {
                    return true; // Someone is right next to us? Disadvantage given.
                }
            }
            return false; // No enemies next to us, no disadvantage given.
        }
        else // For enemies
        {
            foreach (GameObject unit in TurnManager.allEnemies)
            {
                int distance = CalculateDistance(attacker.gameObject, unit);
                if (distance == 1)
                {
                    return true; // Someone is right next to us? Disadvantage given.
                }
            }
            return false; // No players next to us, no disadvantage given.
        }
    }

    public int RollWithVantage(Character attacker, int vantageType, int modifier)
    {
        int rollA, rollB;

        switch (vantageType)
        {
            case 0: // No change
                attackRoll = DiceRoll.inst.RollD20() + modifier + attacker.proficiency;
                break;
            case 1: // Advantage (Roll twice, take highest)
                rollA = DiceRoll.inst.RollD20() + modifier + attacker.proficiency;
                rollB = DiceRoll.inst.RollD20() + modifier + attacker.proficiency;
                if (rollA > rollB)
                {
                    attackRoll = rollA;
                }
                else
                {
                    attackRoll = rollB;
                }
                break;
            case 2: // Disadvantage (Roll twice, take lowest)
                rollA = DiceRoll.inst.RollD20() + modifier + attacker.proficiency;
                rollB = DiceRoll.inst.RollD20() + modifier + attacker.proficiency;
                if (rollA > rollB)
                {
                    attackRoll = rollB;
                }
                else
                {
                    attackRoll = rollA;
                }
                break;
            default:
                Debug.LogError("Invalid vantage input for RollWithVantage()");
                break;
        }

        return attackRoll;
    }

    public int SaveOrSuckRoll(Character defender, int vantageType, int modifier)
    {
        int rollA, rollB;

        switch (vantageType)
        {
            case 0: // No change
                saveRoll = DiceRoll.inst.RollD20() + modifier;
                break;
            case 1: // Advantage (Roll twice, take highest)
                rollA = DiceRoll.inst.RollD20() + modifier;
                rollB = DiceRoll.inst.RollD20() + modifier;
                if (rollA > rollB)
                {
                    saveRoll = rollA;
                }
                else
                {
                    saveRoll = rollB;
                }
                break;
            case 2: // Disadvantage (Roll twice, take lowest)
                rollA = DiceRoll.inst.RollD20() + modifier;
                rollB = DiceRoll.inst.RollD20() + modifier;
                if (rollA > rollB)
                {
                    saveRoll = rollB;
                }
                else
                {
                    saveRoll = rollA;
                }
                break;
            default:
                Debug.LogError("Invalid vantage input for RollWithVantage()");
                break;
        }

        return saveRoll;
    }

    /// <summary>
    /// Caculates the distance from one point to another in *non-pythagorean terms*. Returns the largest distance in terms of units (not ft).
    /// </summary>
    /// <param name="start">Start point for measuring.</param>
    /// <param name="end">End point for measuring.</param>
    /// <returns>The largest distance in terms of units (not ft)</returns>
    public int CalculateDistance(GameObject start, GameObject end)
    {
        Transform startDiff;
        int longest;

        startDiff = start.transform;
        Vector3 endRel = startDiff.InverseTransformPoint(end.transform.position);

        int verticalDist = (int)endRel.x;
        int upwardsDist = (int)endRel.y;
        int horizontalDist = (int)endRel.z;

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

        if (verticalDist > horizontalDist && verticalDist > upwardsDist) // Which ever is larger is the one we want
        {
            longest = verticalDist;
        }
        else if (horizontalDist > verticalDist && horizontalDist > upwardsDist)
        {
            longest = horizontalDist;
        }
        else
        {
            longest = upwardsDist;
        }

        return longest;
    }

    IEnumerator DestroyTempSound(GameObject toDestroy)
    {
        yield return new WaitForSeconds(10f);

        Destroy(toDestroy);
    }

    public void PlayAnimation(int type, TacticsMove target)
    {
        if(target == null)
        {
            return;
        }

        switch (type)
        {
            case 1:
                if (!TurnManager.IsPlayer(target)) // AI ONLY
                {
                    target.GetComponent<NPCMove>().anim.Play("Base Layer.GetHit");
                }
                else
                {
                    target.GetComponent<PlayerMove>().anim.Play("Base Layer.GetHit");
                }
                break;

            case 2:
                if (!TurnManager.IsPlayer(target)) // AI ONLY
                {
                    target.GetComponent<NPCMove>().anim.Play("Base Layer.Spell1");
                }
                else
                {
                    target.GetComponent<PlayerMove>().anim.Play("Base Layer.Spell1");
                }
                break;

            case 3:
                if (!TurnManager.IsPlayer(target)) // AI ONLY
                {
                    target.GetComponent<NPCMove>().anim.Play("Base Layer.Spell2");
                }
                else
                {
                    target.GetComponent<PlayerMove>().anim.Play("Base Layer.Spell2");
                }
                break;

            case 4:
                if (!TurnManager.IsPlayer(target)) // AI ONLY
                {
                    target.GetComponent<NPCMove>().anim.Play("Base Layer.Spell3");
                }
                else
                {
                    target.GetComponent<PlayerMove>().anim.Play("Base Layer.Spell3");
                }
                break;

            case 5:
                if (!TurnManager.IsPlayer(target)) // AI ONLY
                {
                    target.GetComponent<NPCMove>().anim.Play("Base Layer.Spell4");
                }
                else
                {
                    target.GetComponent<PlayerMove>().anim.Play("Base Layer.Spell4");
                }
                break;

            case 6:
                if (!TurnManager.IsPlayer(target)) // AI ONLY
                {
                    target.GetComponent<NPCMove>().anim.Play("Base Layer.Attack1");
                }
                else
                {
                    target.GetComponent<PlayerMove>().anim.Play("Base Layer.Attack1");
                }
                break;

            case 7:
                if (!TurnManager.IsPlayer(target)) // AI ONLY
                {
                    target.GetComponent<NPCMove>().anim.Play("Base Layer.Attack2");
                }
                else
                {
                    target.GetComponent<PlayerMove>().anim.Play("Base Layer.Attack2");
                }
                break;

            case 8:
                if (!TurnManager.IsPlayer(target)) // AI ONLY
                {
                    target.GetComponent<NPCMove>().anim.Play("Base Layer.Attack3");
                }
                else
                {
                    target.GetComponent<PlayerMove>().anim.Play("Base Layer.Attack3");
                }
                break;
        }
    }
}
