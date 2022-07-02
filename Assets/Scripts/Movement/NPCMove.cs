using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DirectAbilitiesFull;

public class NPCMove : TacticsMove
{

    public GameObject target;
    public int AIType; // Which type of AI will this be? [1:Melee, 2:Ranged, Caster, etc]
    public int distanceFromTarget;

    public GameObject newFiringPos;
    public int tooCloseDist = 4; // The point at which a Ranged AI will want to pull back
    int rangedStatus;
    public GameObject realTarget;
    public GameObject debug;
    public GameObject healTarget;

    bool doHeal = false;
    bool doOnce = false;

    List<GameObject> targetList = new List<GameObject>();
    List<GameObject> tileList = new List<GameObject>();

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

        switch (AIType)
        {
            case 1:
                MeleeAI();
                break;
            case 2:
                RangedAI();
                break;

            case 3:
                CasterAI();
                break;

            default:
                Debug.LogError("Error in AI type inside NPCMove. Choose a valid AIType!");
                break;
        }
    }

    void MeleeAI()
    {
        CheckFlanking();

        if (!turn)
        {
            return;
        }
        else
        {
            // Debug things here
        }


        if (!moving && !doneMoving) // If i'm not moving but can still move...
        {
            FindNearestTarget(); // Get the target
            CalculatePath(); // A* inside
            FindSelectableTiles(); // BFS
            actualTargetTile.target = true;

            distanceFromTarget = CombatManager.inst.CalculateDistance(this.gameObject, target);
            distanceFromTarget = distanceFromTarget * 5; // Convert to ft
        }
        else if (!moving && canAttack) // If i'm not moving and am in (melee) range of my target...
        {
            // Attack
            if(this.GetComponent<Character>().normalAction != 0) // I can take an action
            {
                DoMeleeAttack();
                //CombatManager.inst.CreateCombatEvent(this.GetComponent<Character>(), target.GetComponent<Character>(), "melee", 0, this.GetComponent<Character>().vantageStatus);
                //this.GetComponent<Character>().normalAction -= 1;
            }

            TurnManager.EndTurn();
        }
        else if (!moving && doneMoving) // If i'm not moving but have gone as far as I can go...
        {
            if (target == null)
            {
                FindNearestTarget(); // Get a new target
            }

            rangedStatus = CanDoRangedAttack(distanceFromTarget);

            if(rangedStatus == 0)
            {
                // End turn
            }
            else if(rangedStatus == 1)
            {
                // Do it!
                if (this.GetComponent<Character>().normalAction != 0) // I can take an action
                {
                    DoRangedAttack();
                    //this.GetComponent<Character>().normalAction -= 1;
                    //CombatManager.inst.CreateCombatEvent(this.GetComponent<Character>(), target.GetComponent<Character>(), "ranged", 2, this.GetComponent<Character>().vantageStatus);
                    
                }
            }
            else
            {
                // Should we bother?
                int maxER = CombatManager.inst.GetComponent<DirectAbilities>().maxRange[2];
                if(distanceFromTarget >= maxER + 15)
                {
                    // We are about to get into effective fire range, hold off for now.
                }
                else
                {
                    // We are very far away, go ahead.
                    if (this.GetComponent<Character>().normalAction != 0) // I can take an action
                    {
                        DoRangedAttack();
                        //CombatManager.inst.CreateCombatEvent(this.GetComponent<Character>(), target.GetComponent<Character>(), "ranged", 2, 2);
                        //this.GetComponent<Character>().normalAction -= 1;
                    }
                }
            }

            TurnManager.EndTurn();
        }
        else
        {
            Move();
        }
    }

    void RangedAI()
    {
        CheckFlanking();

        if (!turn)
        {
            return;
        }
        else
        {
            // Debug things here
        }

        // 1 - Am I in range of a target?
        // 2 - Do I have line of sight of a target?

        FindNearestTarget(); // Who is closest to me?

        distanceFromTarget = CombatManager.inst.CalculateDistance(this.gameObject, target);

        if (!moving && !doneMoving) // If i'm not moving but can still move...
        {
            if (distanceFromTarget <= 1) // They are in melee range!
            {
                if (this.GetComponent<Character>().normalAction != 0)
                {
                    DoMeleeAttack(); // Fight back!
                }
                PullBack(); // Retreat
                FindSelectableTiles(); // BFS
                actualTargetTile.target = true;
            }
            else if (distanceFromTarget < tooCloseDist) // They are too close, pull back
            {
                PullBack();
                FindSelectableTiles(); // BFS
                actualTargetTile.target = true;
            }
            else // We are fine, continue
            {

            }

            while(target == null) // Target has died during someone else's turn, find a new one.
            {
                FindNearestTarget();
            }

            target = CheckLOS(); // Can we hit the target from where we started this turn?
            if (target == null) // No
            {
                FindFiringPosition(); // We can't hit anything! Let's try to move somewhere where we can
                target = newFiringPos;
                //Debug.Log("target: " + target);

                GameObject ft = new GameObject();
                Vector3 adjust = new Vector3(0, 1, 0);
                ft.transform.position = target.transform.position + adjust; // Move it up
                target = ft;

                CalculatePathDirect(); // A* inside
                FindSelectableTiles(); // BFS
                //Move();
                //actualTargetTile.target = true;

                Destroy(ft);
            }

            doneMoving = true;
            canAttack = true;
            target = CheckLOS();

        }
        else if (!moving && canAttack) // If i'm not moving and am in (melee) range of my target...
        {
            // Attack
            if (this.GetComponent<Character>().normalAction != 0) // I can take an action
            {
                DoRangedAttack();
                //this.GetComponent<Character>().normalAction -= 1;
                //CombatManager.inst.CreateCombatEvent(this.GetComponent<Character>(), target.GetComponent<Character>(), "ranged", 2, this.GetComponent<Character>().vantageStatus);
            }

            TurnManager.EndTurn();
        }
        else if (!moving && doneMoving) // If i'm not moving but have gone as far as I can go...
        {
            TurnManager.EndTurn();
        }
        else
        {
            Move();
        }
    }

    void CasterAI() // AI for spellcaster
    {
        CheckFlanking();

        if (!turn)
        {
            return;
        }
        else
        {
            // Debug things here
        }

        FindNearestTarget(); // Who is closest to me?

        distanceFromTarget = CombatManager.inst.CalculateDistance(this.gameObject, target);

        if (!moving && !doneMoving) // If i'm not moving but can still move...
        {
            if (distanceFromTarget <= 1) // They are in melee range!
            {
                if (this.GetComponent<Character>().normalAction != 0)
                {
                    DoMeleeAttack(); // Fight back!
                }
                PullBack(); // Retreat
                FindSelectableTiles(); // BFS
                actualTargetTile.target = true;
            }
            else if (distanceFromTarget < tooCloseDist) // They are too close, pull back
            {
                PullBack();
                FindSelectableTiles(); // BFS
                actualTargetTile.target = true;
            }
            else // We are fine, continue
            {

            }

            // Should we heal anyone?
            healTarget = CheckAllyHealth();

            if(healTarget == null) // Don't bother
            {
                doHeal = false;

                while (target == null) // Target has died during someone else's turn, find a new one.
                {
                    FindNearestTarget();
                }

                target = CheckLOS(); // Can we hit the target from where we started this turn?
                if (target == null) // No
                {
                    FindFiringPosition(); // We can't hit anything! Let's try to move somewhere where we can
                    target = newFiringPos;

                    GameObject ft = new GameObject();
                    Vector3 adjust = new Vector3(0, 1, 0);
                    ft.transform.position = target.transform.position + adjust; // Move it up
                    target = ft;

                    CalculatePathDirect(); // A* inside
                    FindSelectableTiles(); // BFS
                                           //Move();
                                           //actualTargetTile.target = true;

                    Destroy(ft);
                }

                doneMoving = true;
                canAttack = true;
                target = CheckLOS();
            }
            else // We need to heal someone
            {
                if(healTarget == this) // We need to heal
                {
                    foreach (int ability in this.GetComponent<Character>().abilities)
                    {
                        if (CombatManager.inst.GetComponent<DirectAbilities>().targetType[ability] == "es" && CombatManager.inst.GetComponent<DirectAbilities>().abilityIntent[ability] == 1)
                        {
                            this.GetComponent<Character>().spellSlots[CombatManager.inst.GetComponent<DirectAbilities>().neededSpellSlot[ability]] -= 1; // Remove the spellslot
                            CombatManager.inst.CreateCombatEvent(this.GetComponent<Character>(), this.GetComponent<Character>(), "effect", ability, this.GetComponent<Character>().vantageStatus);
                            return;
                        }
                    }
                    return;
                }
                else  
                {
                    doHeal = true;

                    while (target == null) // Target has died during someone else's turn, find a new one.
                    {
                        healTarget = CheckAllyHealth();
                    }

                    if (healTarget == null)
                    {
                        return;
                    }
                    else
                    {
                        target = healTarget;
                    }

                    target = CheckLOS(); // Can we hit the target from where we started this turn?
                    if (target == null) // No
                    {
                        FindFiringPosition(); // We can't hit anything! Let's try to move somewhere where we can
                        target = newFiringPos;

                        GameObject ft = new GameObject();
                        Vector3 adjust = new Vector3(0, 1, 0);
                        ft.transform.position = target.transform.position + adjust; // Move it up
                        target = ft;

                        CalculatePathDirect(); // A* inside
                        FindSelectableTiles(); // BFS
                                               //Move();
                                               //actualTargetTile.target = true;

                        Destroy(ft);
                    }

                    doneMoving = true;
                    canAttack = true;
                    target = CheckLOS();
                }
            }

        }
        else if (!moving && canAttack) // If i'm not moving and am in range of my target...
        {
            int coinflip = Random.Range(0, 1);

            if (doHeal) // Use a heal
            {
                if(coinflip == 1 && this.GetComponent<Character>().spellSlots[1] >= 1 && !this.GetComponent<Character>().concentrating) // Buff them instead
                {
                    if (this.GetComponent<Character>().normalAction != 0) // I can take an action
                    {
                        DoRangedAttack(false, true);
                    }
                }
                else // Just heal them
                {
                    if (this.GetComponent<Character>().normalAction != 0) // I can take an action
                    {
                        DoRangedAttack(true);
                    }
                }
            }
            else
            {
                if(coinflip == 1 && this.GetComponent<Character>().spellSlots[1] >= 1 && !this.GetComponent<Character>().concentrating) // Debuff them
                {
                    if (this.GetComponent<Character>().normalAction != 0) // I can take an action
                    {
                        DoRangedAttack(false, false, true);
                    }
                }
                else // Normal attack (with a spell)
                {
                    if (this.GetComponent<Character>().normalAction != 0) // I can take an action
                    {
                        DoRangedAttack();
                    }
                }
            }

            TurnManager.EndTurn();
        }
        else if (!moving && doneMoving) // If i'm not moving but have gone as far as I can go...
        {
            TurnManager.EndTurn();
        }
        else
        {
            Move();
        }
    }

    public void DoMeleeAttack()
    {
        this.GetComponent<Character>().normalAction -= 1;

        switch (this.GetComponent<Character>().classString.ToString())
        {
            case "Striker":
                foreach (int ability in this.GetComponent<Character>().abilities)
                {
                    if (CombatManager.inst.GetComponent<DirectAbilities>().targetType[ability] == "ms")
                    {
                        CombatManager.inst.CreateCombatEvent(this.GetComponent<Character>(), target.GetComponent<Character>(), "melee", ability, this.GetComponent<Character>().vantageStatus);
                        return;
                    }
                }
                break;

            case "Defender":
                foreach (int ability in this.GetComponent<Character>().abilities)
                {
                    if (CombatManager.inst.GetComponent<DirectAbilities>().targetType[ability] == "ms")
                    {
                        CombatManager.inst.CreateCombatEvent(this.GetComponent<Character>(), target.GetComponent<Character>(), "melee", ability, this.GetComponent<Character>().vantageStatus);
                        return;
                    }
                }
                break;

            case "Controller":
                foreach (int ability in this.GetComponent<Character>().abilities)
                {
                    if (CombatManager.inst.GetComponent<DirectAbilities>().targetType[ability] == "ms")
                    {
                        CombatManager.inst.CreateCombatEvent(this.GetComponent<Character>(), target.GetComponent<Character>(), "melee", ability, this.GetComponent<Character>().vantageStatus);
                        return;
                    }
                }
                break;

            case "Leader":
                foreach (int ability in this.GetComponent<Character>().abilities)
                {
                    if (CombatManager.inst.GetComponent<DirectAbilities>().targetType[ability] == "ms")
                    {
                        CombatManager.inst.CreateCombatEvent(this.GetComponent<Character>(), target.GetComponent<Character>(), "melee", ability, this.GetComponent<Character>().vantageStatus);
                        return;
                    }
                }
                break;

            default:
                Debug.LogError("NPC has invalid class.");
                break;
        }
    }

    public void DoRangedAttack(bool doHealing = false, bool doBuff = false, bool doDebuff = false)
    {
        this.GetComponent<Character>().normalAction -= 1;
        
        switch (this.GetComponent<Character>().classString.ToString())
        {
            case "Striker":
                foreach (int ability in this.GetComponent<Character>().abilities)
                {
                    if (CombatManager.inst.GetComponent<DirectAbilities>().targetType[ability] == "rs")
                    {
                        CombatManager.inst.CreateCombatEvent(this.GetComponent<Character>(), target.GetComponent<Character>(), "ranged", ability, this.GetComponent<Character>().vantageStatus);
                        return;
                    }
                }
                break;

            case "Defender":
                foreach (int ability in this.GetComponent<Character>().abilities)
                {
                    if (CombatManager.inst.GetComponent<DirectAbilities>().targetType[ability] == "rs")
                    {
                        CombatManager.inst.CreateCombatEvent(this.GetComponent<Character>(), target.GetComponent<Character>(), "ranged", ability, this.GetComponent<Character>().vantageStatus);
                        return;
                    }
                }
                break;

            case "Controller":

                if (TurnManager.IsPlayer(target.GetComponent<TacticsMove>()))
                {
                    doHealing = false;
                    doBuff = false;
                    doDebuff = false;
                }

                if (doHealing) // Heal
                {
                    foreach (int ability in this.GetComponent<Character>().abilities)
                    {
                        if (CombatManager.inst.GetComponent<DirectAbilities>().targetType[ability] == "es" && CombatManager.inst.GetComponent<DirectAbilities>().abilityIntent[ability] == 1)
                        {
                            this.GetComponent<Character>().spellSlots[CombatManager.inst.GetComponent<DirectAbilities>().neededSpellSlot[ability]] -= 1; // Remove the spellslot
                            CombatManager.inst.CreateCombatEvent(this.GetComponent<Character>(), target.GetComponent<Character>(), "effect", ability, this.GetComponent<Character>().vantageStatus);
                            return;
                        }
                    }
                }
                else if (doBuff) // Buff
                {
                    foreach (int ability in this.GetComponent<Character>().abilities)
                    {
                        if (CombatManager.inst.GetComponent<DirectAbilities>().targetType[ability] == "e-3" && CombatManager.inst.GetComponent<DirectAbilities>().abilityIntent[ability] == 1)
                        {
                            this.GetComponent<Character>().spellSlots[CombatManager.inst.GetComponent<DirectAbilities>().neededSpellSlot[ability]] -= 1; // Remove the spellslot
                            CombatManager.inst.CreateCombatEvent(this.GetComponent<Character>(), target.GetComponent<Character>(), "effect", ability, this.GetComponent<Character>().vantageStatus);

                            List<Character> temp = new List<Character>();
                            temp.Clear();
                            temp.Add(target.GetComponent<Character>());
                            bool resultLos;
                            int amountDone = 0;

                            foreach(GameObject go in TurnManager.allEnemies)
                            {
                                if(amountDone != 2)
                                {
                                    resultLos = CheckLOS(go);

                                    if (resultLos && !temp.Contains(go.GetComponent<Character>()))
                                    {
                                        CombatManager.inst.CreateCombatEvent(this.GetComponent<Character>(), go.GetComponent<Character>(), "effect", ability, this.GetComponent<Character>().vantageStatus);
                                        temp.Add(go.GetComponent<Character>());
                                        amountDone++;
                                    }
                                }
                                else
                                {
                                    return;
                                }
                            }
                        }
                    }
                    return;
                }
                else if(doDebuff) // Debuff
                {
                    foreach (int ability in this.GetComponent<Character>().abilities)
                    {
                        if (CombatManager.inst.GetComponent<DirectAbilities>().targetType[ability] == "e-3" && CombatManager.inst.GetComponent<DirectAbilities>().abilityIntent[ability] == 0)
                        {
                            this.GetComponent<Character>().spellSlots[CombatManager.inst.GetComponent<DirectAbilities>().neededSpellSlot[ability]] -= 1; // Remove the spellslot
                            CombatManager.inst.CreateCombatEvent(this.GetComponent<Character>(), target.GetComponent<Character>(), "effect", ability, this.GetComponent<Character>().vantageStatus);

                            List<Character> temp = new List<Character>();
                            temp.Clear();
                            temp.Add(target.GetComponent<Character>());
                            bool resultLos;
                            int amountDone = 0;

                            foreach (GameObject go in TurnManager.allEnemies)
                            {
                                if (amountDone != 2)
                                {
                                    resultLos = CheckLOS(go);

                                    if (resultLos && !temp.Contains(go.GetComponent<Character>()))
                                    {
                                        CombatManager.inst.CreateCombatEvent(this.GetComponent<Character>(), go.GetComponent<Character>(), "effect", ability, this.GetComponent<Character>().vantageStatus);
                                        temp.Add(go.GetComponent<Character>());
                                        amountDone++;
                                    }
                                }
                                else
                                {
                                    return;
                                }
                            }
                        }
                    }
                    return;
                }
                else // Regular spell attack
                {
                    foreach (int ability in this.GetComponent<Character>().abilities)
                    {
                        if (CombatManager.inst.GetComponent<DirectAbilities>().targetType[ability] == "ss")
                        {
                            this.GetComponent<Character>().spellSlots[CombatManager.inst.GetComponent<DirectAbilities>().neededSpellSlot[ability]] -= 1; // Remove the spellslot
                            CombatManager.inst.CreateCombatEvent(this.GetComponent<Character>(), target.GetComponent<Character>(), "spell", ability, this.GetComponent<Character>().vantageStatus);
                            return;
                        }
                    }
                }

                break;

            case "Leader":
                foreach (int ability in this.GetComponent<Character>().abilities)
                {
                    if (CombatManager.inst.GetComponent<DirectAbilities>().targetType[ability] == "rs")
                    {
                        CombatManager.inst.CreateCombatEvent(this.GetComponent<Character>(), target.GetComponent<Character>(), "ranged", ability, this.GetComponent<Character>().vantageStatus);
                        return;
                    }
                }
                break;

            default:
                Debug.LogError("NPC has invalid class.");
                break;
        }
        
    }

    public void HealthCheck()
    {
        if(this.GetComponent<Character>().hitPoints <= 0 && !doOnce) // DEATH
        {
            doOnce = true;
            TurnManager.RemoveUnit(this.GetComponent<TacticsMove>()); // Remove self from turnmanager
        }
    }

    GameObject CheckAllyHealth()
    {
        healTarget = null;
        float max;
        float current;
        float result;

        foreach(GameObject ally in TurnManager.allEnemies)
        {
            max = ally.GetComponent<Character>().maxHealth;
            current = ally.GetComponent<Character>().hitPoints;

            result = 1 - ((max - current) / max);

            if (result < 0.60) // Below 60% HP
            {
                return ally; // Heal this person
            }
        }

        return null; // No one needs healing
    }

    public void CheckFlanking()
    {
        bool temp = false;
        string desiredtag = "Player";
        RaycastHit hit;
        GameObject flank1 = null;
        GameObject flank2 = null;

        if(Physics.Raycast(this.transform.position, Vector3.forward, out hit, 1.5f))
        {
            if (hit.collider.tag == desiredtag)
            {
                flank1 = hit.collider.gameObject;
                if(Physics.Raycast(this.transform.position, -Vector3.forward, out hit, 1.5f))
                {
                    if (hit.collider.tag == desiredtag)
                    {
                        temp = true;
                        flank2 = hit.collider.gameObject;
                    }
                }
            }
        }

        if(Physics.Raycast(this.transform.position, Vector3.right, out hit, 1.5f))
        {
            if (hit.collider.tag == desiredtag)
            {
                flank1 = hit.collider.gameObject;
                if(Physics.Raycast(this.transform.position, -Vector3.right, out hit, 1.5f))
                {
                    if (hit.collider.tag == desiredtag)
                    {
                        temp = true;
                        flank2 = hit.collider.gameObject;
                    }
                }
            }
        }

        if(Physics.Raycast(this.transform.position, Vector3.forward + Vector3.right, out hit, 1.5f)){
            if (hit.collider.tag == desiredtag)
            {
                flank1 = hit.collider.gameObject;
                if(Physics.Raycast(this.transform.position, -Vector3.forward + -Vector3.right, out hit, 1.5f))
                {
                    if (hit.collider.tag == desiredtag)
                    {
                        temp = true;
                        flank2 = hit.collider.gameObject;
                    }
                }
            }
        }

        if(Physics.Raycast(this.transform.position, -Vector3.forward + Vector3.right, out hit, 1.5f))
        {
            if (hit.collider.tag == desiredtag)
            {
                flank1 = hit.collider.gameObject;
                if(Physics.Raycast(this.transform.position, Vector3.forward + -Vector3.right, out hit, 1.5f))
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
    
    int CanDoRangedAttack(int currDist) // Is this unit in range to do a ranged attack?
    {
        int maxEffectiveRange; // Goes up the the maximum distance of the ranged attack (Grants disadvantage)
        int effectiveRange; // The normal range of a ranged attack (No disadvantage)

        List<int> abilityList = new List<int>(this.GetComponent<Character>().abilities);
        
        /*
        if (abilityList.Contains(2))
        {
            // This character can do a ranged attack? Continue.
        }
        else
        {
            return 0;
        }
        */

        // Check line of sight to target
        RaycastHit hit;
        Vector3 direction = target.transform.position - transform.position;

        if (Physics.Raycast(transform.position, direction, out hit))
        {
            if (hit.transform == target.transform)
            {
                // Successs! (Continue)
                Debug.DrawRay(transform.position, direction, Color.red);
            }
            else
            {
                return 0; // Failed, no line of sight
            }
        }
        

        effectiveRange = CombatManager.inst.GetComponent<DirectAbilities>().requiredRange[2];
        maxEffectiveRange = CombatManager.inst.GetComponent<DirectAbilities>().maxRange[2];

        if(currDist <= effectiveRange)
        {
            return 1; // The target is within effective range. Lets do it!
        }
        else if(currDist <= maxEffectiveRange)
        {
            return 2; // The target is within max range, should we still do it?
        }
        else
        {
            return 0; // The target is out of range, don't bother.
        }
    }

    public void PullBack()
    {

        // Find a random reachable tile which is X distance away
        Vector3 dirToTarget = transform.position - target.transform.position;
        Vector3 newLoc = transform.position + dirToTarget;

        GameObject retreatPoint = new GameObject("fallback point");
        retreatPoint.transform.SetParent(transform, false);

        retreatPoint.transform.position = newLoc;

        GameObject targetGO = FindNearestTile(retreatPoint);
        Tile targetTile = targetGO.GetComponent<Tile>();
        FindPath(targetTile); // (A*) Find a path to that target

        Destroy(retreatPoint);
    }

    void CalculatePath() // Where is the NPC going to move to?
    {
        Tile targetTile = GetTargetTile(target); // Get the tile the target is on
        FindPath(targetTile); // (A*) Find a path to that target
    }

    void CalculatePathDirect() // Where is the NPC going to move to? (Direct, not adjacent)
    {
        Tile targetTile = GetTargetTile(target); // Get the tile the target is on
        AstarSelectable(targetTile); // (A*) Find a path to that target
    }

    void FindNearestTarget() // Put AI targeting decisions here
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag("Player"); // Only target players

        GameObject nearest = null;
        float distance = Mathf.Infinity;

        foreach (GameObject obj in targets) // Find closest target based on distance (change this later to be based on different factors)
        {
            float d = Vector3.Distance(transform.position, obj.transform.position);
            //Vector3.SqrMagnitude this is more efficient

            if (d < distance)
            {
                distance = d;
                nearest = obj;
            }
        }

        target = nearest;
    }

    // Bug: Next viable tile search operates as a spiral beginning around the current tile. This can result in the AI taking a very
    // long path (ignoring movement restrictions) due to different heights.

    GameObject FindNearestTile(GameObject targetPoint)
    {
        GameObject[] allTiles = GameObject.FindGameObjectsWithTag("Tile");

        tileList.Clear();

        foreach (GameObject obj in allTiles)
        {
            tileList.Add(obj);
        }
        
        tileList.Sort(delegate (GameObject a, GameObject b) // Sort list by min to max distance
        {
            return Vector3.Distance(targetPoint.transform.position, a.transform.position)
            .CompareTo(
              Vector3.Distance(targetPoint.transform.position, b.transform.position));
        });

        return tileList[0];
    }

    void FindFiringPosition()
    {
        GetCurrentTile();

        GameObject potentialTarget;
        GameObject holder = FindNearestTile(currentTile.gameObject);
        realTarget = null;

        Queue<GameObject> tileQ = new Queue<GameObject>();

        tileQ.Clear();

        if (tileList.Contains(currentTile.gameObject)) // Don't want to check from our curren't pos cause we already did.
        {
            tileList.Remove(currentTile.gameObject);
        }

        foreach (GameObject obj in tileList)
        {
            tileQ.Enqueue(obj);
        }

        while (realTarget == null)
        {

            potentialTarget = tileQ.Peek();

            realTarget = CheckLOS(potentialTarget);

            if (realTarget != null)
            {
                // Success!
                newFiringPos = potentialTarget;
                debug = potentialTarget;
                return;

            }
            else
            {
                // Try again
                tileQ.Dequeue();
            }
        }

        Debug.LogError("Ranged AI failed to find a firing position");
    }

    GameObject CheckLOS(GameObject location = null)
    {
        GameObject xLoc;

        if(location == null)
        {
            location = this.gameObject;
        }
        else
        {
            xLoc = new GameObject();
            Vector3 adjust = new Vector3(0, 1, 0);
            xLoc.transform.position = location.transform.position + adjust; // Move it up
            location = xLoc;
            Destroy(xLoc);
        }

        GameObject[] targets = GameObject.FindGameObjectsWithTag("Player"); // Only target players

        foreach(GameObject obj in targets)
        {
            if(obj != null)
            {
                targetList.Add(obj);
            }
            
        }

        foreach (GameObject obj in targets)
        {
            if (obj == null)
            {
                targetList.Remove(obj);
            }

        }

        targetList.Sort(delegate (GameObject b, GameObject a) // Sort list by min to max distance
        {
            return Vector3.Distance(location.transform.position, a.transform.position)
            .CompareTo(
              Vector3.Distance(location.transform.position, b.transform.position));
        });

        foreach (GameObject obj in targetList) // Find closest target based on distance (change this later to be based on different factors)
        {
            if(obj != null)
            {
                // Check line of sight to target
                RaycastHit hit;
                Vector3 direction = obj.transform.position - location.transform.position;

                if (Physics.Raycast(location.transform.position, direction, out hit))
                {
                    if (hit.transform == obj.transform)
                    {
                        // Successs! This is our new target
                        Debug.DrawRay(location.transform.position, direction, Color.red);
                        return obj;
                    }
                    else
                    {
                        // Failed, no line of sight
                    }
                }
            }
            else
            {
                targetList.Remove(obj);
            }
        }

        return null; // Failed to get LOS with anything
    }

}
