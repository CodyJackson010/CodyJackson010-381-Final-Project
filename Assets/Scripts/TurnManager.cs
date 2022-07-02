using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DirectAbilitiesFull;
using System.Linq;

public class TurnManager : MonoBehaviour
{
    static Dictionary<string, List<TacticsMove>> units = new Dictionary<string, List<TacticsMove>>();
    static Queue<string> turnKey = new Queue<string>(); // Who's turn is it?
    static Queue<TacticsMove> turnTeam = new Queue<TacticsMove>(); // Queue for current turn's team
    public static TacticsMove currentUnit;
    public static TacticsMove targetPlayer; // The player being attacked
    public static List<TacticsMove> allUnits = new List<TacticsMove>();

    public static int characterCount;
    public static List<GameObject> act = new List<GameObject>(); // All characters, friend or foe
    public static List<GameObject> allPlayers = new List<GameObject>();
    public static List<GameObject> allEnemies = new List<GameObject>();
    public static Queue<TacticsMove> initiativeOrder = new Queue<TacticsMove>();

    //static int characterCount;
    //static Dictionary<string, List<GameObject>> characters = new Dictionary<string, List<GameObject>>();

    public static bool startCombat = false; // If true, register turns
    public static bool doOnce = false;
    public static bool showGuide; // For vis arrow
    public static bool playOnce = true;

    public static bool musicStart = false;

    public static TurnManager inst;
    public void Awake()
    {
        inst = this;
    }

    void Start()
    {
        
    }

    public void UtilStartCombat() // Used by Debug Panel
    {
        startCombat = true;
    }

    /*
    void Update()
    {
        if (startCombat == true)
        {
            if (turnTeam.Count == 0) // Will only really happen once
            {
                InitLoadCharacters();
            }
        }
    }
    */

    static void InitLoadCharacters()
    {
        act.AddRange(GameObject.FindGameObjectsWithTag("Player"));
        act.AddRange(GameObject.FindGameObjectsWithTag("NPC"));

        allPlayers.AddRange(GameObject.FindGameObjectsWithTag("Player"));
        allEnemies.AddRange(GameObject.FindGameObjectsWithTag("NPC"));

        characterCount = act.Count;

        foreach (GameObject obj in act)
        {
            obj.GetComponent<Character>().initiative = DiceRoll.inst.RollD20() + obj.GetComponent<Character>().dexterityMod;
            obj.GetComponent<TacticsMove>().FillTiles();
        }

        if (act.Count > 0)
        {
            act.Sort(delegate (GameObject b, GameObject a) // Pre-reverse
            {
                return (a.GetComponent<Character>().initiative).CompareTo(b.GetComponent<Character>().initiative);
            });
        }

        foreach(GameObject obj in act) // This may be reversed
        {
            initiativeOrder.Enqueue(obj.GetComponent<TacticsMove>());
        }

        UIManager.inst.CreateInitiatveUI();
        StartTurn();
    }
    
    
    void Update()
    {
        if(startCombat == true)
        {
            if(act.Count == 0) // Will only really happen once
            {
                InitLoadCharacters();
            }

            if (!musicStart)
            {
                int random = Random.Range(0, 4);
                SoundManager.inst.PlayAmbientAudio(random); // Background music
                musicStart = true;
            }

            CheckGameStatus();
            /*
            if (turnTeam.Count == 0) // Will only really happen once
            {
                InitTeamTurnQueue();
            }
            */
        }
    }

    public static void CheckGameStatus() // Check to see if the game needs to be ended
    {
        if (allPlayers.Count <= 0 || allEnemies.Count <= 0)
        {
            if (playOnce)
            {
                // End Combat
                if (allPlayers.Count > 0) // Enemy team defeated! Player wins!
                {
                    playOnce = false;
                    Debug.Log("All enemies defeated. Victory!");
                    IGOptions.inst.DisplayVictory();
                    return;
                }
                else // Player team defeated. Player loses (Also includes edge case where both teams are empty, in this case, the player still loses)
                {
                    playOnce = false;
                    Debug.Log("All player characters have died. Defeat!");
                    IGOptions.inst.DisplayDefeat();
                    return;
                }
            }
            else
            {

            }
        }
    }

    public static bool IsPlayer(TacticsMove currentUnit) // Check to see if given unit is a player, return true if so
    {
        return currentUnit.tag == "Player";
    }

    public static void StartTurn()
    {
        if (allPlayers.Count > 0 && allEnemies.Count > 0) // There is at least 1 unit on each side, keep going
        {
            initiativeOrder.Peek().BeginTurn();
            currentUnit = initiativeOrder.Peek();

            // Refresh the character's actions
            currentUnit.GetComponent<Character>().normalAction = 1;
            currentUnit.GetComponent<Character>().bonusAction = 1;
            currentUnit.GetComponent<Character>().reAction = 1;

            currentUnit.GetComponent<Character>().visSquare.SetActive(true); // Enabled its selection square

            StartTurnExtra();

            if (IsPlayer(currentUnit))
            {
                targetPlayer = null;
                UIManager.inst.GenerateAbilityUI();
                UIManager.inst.UpdateHUD();
                UIManager.inst.UpdateSpellSlots(currentUnit.GetComponent<Character>());
                UIManager.inst.EnableEndTurnButton(); // Allow the player to end their turn
                doOnce = true; // Used for guidance arrow
                CombatManager.inst.doneAttacking = false;
            }
        }
        else // One team is out of units
        {
            // End Combat
            if (allPlayers.Count > 0) // Enemy team defeated! Player wins!
            {
                Debug.Log("All enemies defeated. Victory!");
                IGOptions.inst.DisplayVictory();
            }
            else // Player team defeated. Player loses (Also includes edge case where both teams are empty, in this case, the player still loses)
            {
                Debug.Log("All player characters have died. Defeat!");
                IGOptions.inst.DisplayDefeat();
            }
        }
    }

    public static void EndTurn()
    {
        TacticsMove unit = initiativeOrder.Dequeue(); // Take the unit out of the queue (at the front)
        unit.GetComponent<Character>().visSquare.SetActive(false);
        unit.EndTurn(); // End its turn
        initiativeOrder.Enqueue(unit); // Put it back in the queue (at the back)

        UIManager.inst.NextCharacter();
        StartTurn();
    }

    public static void RemoveUnit(TacticsMove unit) // Remove unit from list
    {
        if (IsPlayer(unit))
        {
            allPlayers.Remove(unit.gameObject);
        }
        else
        {
            allEnemies.Remove(unit.gameObject);
        }
        act.Remove(unit.gameObject);
        
        UIManager.inst.RemoveCharacterFromInitiative(unit.GetComponent<Character>());

        unit.GetComponent<TacticsMove>().Destroy(); // Destroy the unit

        initiativeOrder = new Queue<TacticsMove>(initiativeOrder.Where(p => p != unit)); // "Remove" the unit by using Linq, other methods failed to work.

        CheckGameStatus();
    }

    static void StartTurnExtra()
    {
        if (currentUnit.GetComponent<Character>().buffs.Contains(18))
        {
            currentUnit.GetComponent<Character>().armorClass -= 5;
        }
    }

    /*
    static void InitTeamTurnQueue() // Initialize the team
    {
        List<TacticsMove> teamList = units[turnKey.Peek()];

        foreach(TacticsMove unit in teamList)
        {
            turnTeam.Enqueue(unit);
        }

        StartTurn();
    }

    public static void StartTurn() {
        if(turnTeam.Count > 0) // This team has atleast 1 unit
        {
            turnTeam.Peek().BeginTurn();
            currentUnit = turnTeam.Peek();

            // Refresh the character's actions
            currentUnit.GetComponent<Character>().normalAction = 1;
            currentUnit.GetComponent<Character>().bonusAction = 1;
            currentUnit.GetComponent<Character>().reAction = 1;

            if (IsPlayer(currentUnit))
            {
                UIManager.inst.GenerateAbilityUI();
                UIManager.inst.UpdateHUD();
                UIManager.inst.EnableEndTurnButton(); // Allow the player to end their turn
                doOnce = true; // Used for guidance arrow
                CombatManager.inst.doneAttacking = false;
                showGuide = true; // By default show the arrow [CHANGE THIS LATER]
            }
        }
        else // This team has no more units
        {
            // End Combat
            if (IsPlayer(currentUnit)) // (From last turn). Player wins!
            {

            }
            else // Player loses
            {

            }
        }
    }

    public static void EndTurn()
    {
        TacticsMove unit = turnTeam.Dequeue();
        unit.EndTurn();


        if(turnTeam.Count > 0) // Still going
        {
            StartTurn();
        }
        else // Finished everything
        {
            string team = turnKey.Dequeue(); // Next team
            turnKey.Enqueue(team);
            InitTeamTurnQueue();
        }
    }

    public static void AddUnit(TacticsMove unit) // Unit will add itself to the list
    {
        List<TacticsMove> list;

        if (!units.ContainsKey(unit.tag)) // Tag not in dictionary
        {
            list = new List<TacticsMove>();
            units[unit.tag] = list;

            if (!turnKey.Contains(unit.tag))
            {
                turnKey.Enqueue(unit.tag);
            }
        }
        else // Tag *is* in dictionary
        {
            list = units[unit.tag];
        }

        list.Add(unit);
        allUnits.Add(unit);
    }

    public static void RemoveUnit(TacticsMove unit) // Remove unit from list
    {
        // Not finished

        List<TacticsMove> list = units[unit.tag];

        allUnits.Remove(unit);
        list.Remove(unit);
    }
    */

    /*
     * Q: How can i remove a unit from team, or stop switching teams if there is no member left?
     * 
     * A: In the TurnManager script, the StartTurn() function checks to see if there are more than 0 units. 
     * Just add an else clause to do something (like end the game) when there are 0 units left. 
     * You could also remove the team from the Dictionary when the number of units hits 0. 
     * To remove a unit, you need to add a new function in the TurnManager that grabs the team list from the Dictionary, 
     * and then remove the unit from that list (use the Remove() function).
     * 
     * Q: How does it decide who goes first?
     * 
     * A: It might be from when the objects are first added to the hierarchy. 
     * You can try deleting the player and then adding it back in. It's not random. 
     * If you want to control which team goes first, I suggest adding something to the TurnManager that allows 
     * you to assign a variable which designates the starting team.
     * 
     */
}
