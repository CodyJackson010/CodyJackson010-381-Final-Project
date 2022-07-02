using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DirectAbilitiesFull;
using ClassList;

public class Character : MonoBehaviour
{
    /// <summary>
    /// Base class inherited by all characters, enemy or player. Holds the stats and information about said character.
    /// </summary>

    public int movementSpeed; // How far the player can move (divided by 5)
    public int armorClass; // How hard the player is to hit (higher = better)
    public int hitPoints; // The CURRENT health of the player (starts at max)
    public int maxHealth; // The absolute maximum health the player can have
    public int proficiency; // How good a character is with a weapon, this gets added on to attacks. Depends on level.
    public int characterLevel; // Which level this character is at (1 to 20)
    public int spellAttackMod; // Which modifier does this character use to cast spells?

    public string characterName;
    public string charaterSize;
    public string characterAlignment; // like we will ever use this
    public List<string> languagesKnown = new List<string>();
    public Classes characterClass;
    public string classString;
    public List<string> resistances = new List<string>(); // If a character is resistance to a type of attack they will take half damage.

    public int[] spellSlots = new int[10];
    public int classPoints; // Class specific points (Used by Sorcerer, monk, ?)

    public bool concentrating = false; // Spell concentration
    public int concAbilityId; // ID (DAF) of the ability being concentrated on
    public List<GameObject> concTargets = new List<GameObject>(); // List of who is the target of the concentration (if needed)

    public Sprite characterPfp; // Picture used to represent the character in the UI
    public int initiative; // When this character takes its turn in the turn order (Higher = betteR).

    /// <summary>
    /// All the abilities this character can use, stored in numerical form. To see what each number means, visit 'DirectAbilities.cs'
    /// </summary>
    public List<int> abilities = new List<int>();
    public List<int> usedAbilities = new List<int>(); // Contains a list of all abilities used should they only be activated once per X.

    public int abilityCount;
    public int passivePerception;
    public string characterRace;

    public List<int> buffs = new List<int>();
    public List<int> debuffs = new List<int>();

    // Actions
    public int normalAction;
    public int bonusAction;
    public int reAction;

    // Ability Stats
    public int strength;
    public int dexterity;
    public int constitution;
    public int intelligence;
    public int wisdom;
    public int charisma;

    // Modifiers for Stats
    public int strengthMod;
    public int dexterityMod;
    public int constitutionMod;
    public int intelligenceMod;
    public int wisdomMod;
    public int charismaMod;

    // Conditions
    public int vantageStatus = 0; // 0 = Normal, 1 = Advantage, 2 = Disadvantage

    // Visualization Square
    public GameObject visSquare;


    private void Awake()
    {
        visSquare.SetActive(false);

        // Assigning modifier value based on base stat roll
        strengthMod = ModifierHelper(strength);
        dexterityMod = ModifierHelper(dexterity);
        constitutionMod = ModifierHelper(constitution);
        intelligenceMod = ModifierHelper(intelligence);
        wisdomMod = ModifierHelper(wisdom);
        charismaMod = ModifierHelper(charisma);

        passivePerception = wisdom;
        movementSpeed = movementSpeed / 5; // Each tile is a 5ft cube
        abilityCount = abilities.Count;
        maxHealth = hitPoints;
        proficiency = LevelProficiency();

        if (this.gameObject.tag == "Player") // This character is a player
        {
            RetrieveAbilitiesFromClass(classString);
        }
        else // This character is an NPC
        {
            RetrieveAbilitiesFromClassNPC(classString);
        }
        
    }

    // Helper function that returns modifier value based on base stat passed
    private int ModifierHelper(int stat)
    {
        if (stat == 1)
            return -5;
        else if (stat == 2 || stat == 3)
            return -4;
        else if (stat == 4 || stat == 5)
            return -3;
        else if (stat == 6 || stat == 7)
            return -2;
        else if (stat == 8 || stat == 9)
            return -1;
        else if (stat == 10 || stat == 11)
            return 0;
        else if (stat == 12 || stat == 13)
            return 1;
        else if (stat == 14 || stat == 15)
            return 2;
        else if (stat == 16 || stat == 17)
            return 3;
        else if (stat == 18 || stat == 19)
            return 4;
        else if (stat == 10)
            return 5;
        else
        {
            return 0;
        }
    }

    private int LevelProficiency()
    {
        int i = characterLevel;

        if (i <= 4)
        {
            return 2;
        }
        else if (i <= 8 && i >= 5)
        {
            return 3;
        }
        else if (i <= 12 && i >= 9)
        {
            return 4;
        }
        else if (i <= 16 && i >= 13)
        {
            return 5;
        }
        else
        {
            return 6;
        }
    }


    public void RetrieveAbilitiesFromClass(string classString)
    {
        switch (classString)
        {
            case "Barbarian":
                spellAttackMod = 0;
                /*
                * ———————————No spells?———————————
                ⠀⣞⢽⢪⢣⢣⢣⢫⡺⡵⣝⡮⣗⢷⢽⢽⢽⣮⡷⡽⣜⣜⢮⢺⣜⢷⢽⢝⡽⣝
                ⠸⡸⠜⠕⠕⠁⢁⢇⢏⢽⢺⣪⡳⡝⣎⣏⢯⢞⡿⣟⣷⣳⢯⡷⣽⢽⢯⣳⣫⠇
                ⠀⠀⢀⢀⢄⢬⢪⡪⡎⣆⡈⠚⠜⠕⠇⠗⠝⢕⢯⢫⣞⣯⣿⣻⡽⣏⢗⣗⠏⠀
                ⠀⠪⡪⡪⣪⢪⢺⢸⢢⢓⢆⢤⢀⠀⠀⠀⠀⠈⢊⢞⡾⣿⡯⣏⢮⠷⠁⠀⠀
                ⠀⠀⠀⠈⠊⠆⡃⠕⢕⢇⢇⢇⢇⢇⢏⢎⢎⢆⢄⠀⢑⣽⣿⢝⠲⠉⠀⠀⠀⠀
                ⠀⠀⠀⠀⠀⡿⠂⠠⠀⡇⢇⠕⢈⣀⠀⠁⠡⠣⡣⡫⣂⣿⠯⢪⠰⠂⠀⠀⠀⠀
                ⠀⠀⠀⠀⡦⡙⡂⢀⢤⢣⠣⡈⣾⡃⠠⠄⠀⡄⢱⣌⣶⢏⢊⠂⠀⠀⠀⠀⠀⠀
                ⠀⠀⠀⠀⢝⡲⣜⡮⡏⢎⢌⢂⠙⠢⠐⢀⢘⢵⣽⣿⡿⠁⠁⠀⠀⠀⠀⠀⠀⠀
                ⠀⠀⠀⠀⠨⣺⡺⡕⡕⡱⡑⡆⡕⡅⡕⡜⡼⢽⡻⠏⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
                ⠀⠀⠀⠀⣼⣳⣫⣾⣵⣗⡵⡱⡡⢣⢑⢕⢜⢕⡝⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
                ⠀⠀⠀⣴⣿⣾⣿⣿⣿⡿⡽⡑⢌⠪⡢⡣⣣⡟⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
                ⠀⠀⠀⡟⡾⣿⢿⢿⢵⣽⣾⣼⣘⢸⢸⣞⡟⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
                ⠀⠀⠀⠀⠁⠇⠡⠩⡫⢿⣝⡻⡮⣒⢽⠋⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
                —————————————————————————————
                                    */
                
                break;

            case "Bard":
                spellAttackMod = charismaMod;
                spellSlots[0] = 999; // Cantrips

                if (characterLevel > 2)
                {
                    spellSlots[1] = 4;
                }
                if (characterLevel > 3)
                {
                    spellSlots[2] = 3;
                }
                if (characterLevel > 5)
                {
                    spellSlots[3] = 3;
                }
                if (characterLevel > 8)
                {
                    spellSlots[4] = 3;
                }
                if (characterLevel > 9)
                {
                    spellSlots[5] = 2;
                }
                if (characterLevel > 10)
                {
                    spellSlots[6] = 1;
                }
                if (characterLevel > 12)
                {
                    spellSlots[7] = 1;
                }
                if (characterLevel > 14)
                {
                    spellSlots[8] = 1;
                }
                if (characterLevel > 16)
                {
                    spellSlots[9] = 1;
                }
                if (characterLevel > 17)
                {
                    spellSlots[5] = 3;
                }
                if (characterLevel > 18)
                {
                    spellSlots[6] = 2;
                }

                switch (characterLevel)
                {
                    case 1:
                        spellSlots[1] = 2;
                        break;

                    case 2:
                        spellSlots[1] = 3;
                        break;

                    case 3:
                        spellSlots[2] = 2;
                        break;

                    case 5:
                        spellSlots[3] = 2;
                        break;

                    case 7:
                        spellSlots[4] = 1;
                        break;

                    case 8:
                        spellSlots[4] = 2;
                        break;

                    case 9:
                        spellSlots[5] = 1;
                        break;

                    case 20:
                        spellSlots[7] = 2;
                        break;

                    default:

                        break;
                }
                break;

            case "Cleric":
                spellAttackMod = wisdomMod;
                spellSlots[0] = 999; // Cantrips

                if (characterLevel > 2)
                {
                    spellSlots[1] = 4;
                }
                if (characterLevel > 3)
                {
                    spellSlots[2] = 3;
                }
                if (characterLevel > 5)
                {
                    spellSlots[3] = 3;
                }
                if (characterLevel > 8)
                {
                    spellSlots[4] = 3;
                }
                if (characterLevel > 9)
                {
                    spellSlots[5] = 2;
                }
                if (characterLevel > 10)
                {
                    spellSlots[6] = 1;
                }
                if (characterLevel > 12)
                {
                    spellSlots[7] = 1;
                }
                if (characterLevel > 14)
                {
                    spellSlots[8] = 1;
                }
                if (characterLevel > 16)
                {
                    spellSlots[9] = 1;
                }
                if (characterLevel > 17)
                {
                    spellSlots[5] = 3;
                }
                if (characterLevel > 18)
                {
                    spellSlots[6] = 2;
                }

                switch (characterLevel)
                {
                    case 1:
                        spellSlots[1] = 2;
                        break;

                    case 2:
                        spellSlots[1] = 3;
                        break;

                    case 3:
                        spellSlots[2] = 2;
                        break;

                    case 5:
                        spellSlots[3] = 2;
                        break;

                    case 7:
                        spellSlots[4] = 1;
                        break;

                    case 8:
                        spellSlots[4] = 2;
                        break;

                    case 9:
                        spellSlots[5] = 1;
                        break;

                    case 20:
                        spellSlots[7] = 2;
                        break;

                    default:

                        break;
                }
                break;

            case "Druid":
                spellAttackMod = wisdomMod;
                spellSlots[0] = 999; // Cantrips

                if (characterLevel > 2)
                {
                    spellSlots[1] = 4;
                }
                if (characterLevel > 3)
                {
                    spellSlots[2] = 3;
                }
                if (characterLevel > 5)
                {
                    spellSlots[3] = 3;
                }
                if (characterLevel > 8)
                {
                    spellSlots[4] = 3;
                }
                if (characterLevel > 9)
                {
                    spellSlots[5] = 2;
                }
                if (characterLevel > 10)
                {
                    spellSlots[6] = 1;
                }
                if (characterLevel > 12)
                {
                    spellSlots[7] = 1;
                }
                if (characterLevel > 14)
                {
                    spellSlots[8] = 1;
                }
                if (characterLevel > 16)
                {
                    spellSlots[9] = 1;
                }
                if (characterLevel > 17)
                {
                    spellSlots[5] = 3;
                }
                if (characterLevel > 18)
                {
                    spellSlots[6] = 2;
                }

                switch (characterLevel)
                {
                    case 1:
                        spellSlots[1] = 2;
                        break;

                    case 2:
                        spellSlots[1] = 3;
                        break;

                    case 3:
                        spellSlots[2] = 2;
                        break;

                    case 5:
                        spellSlots[3] = 2;
                        break;

                    case 7:
                        spellSlots[4] = 1;
                        break;

                    case 8:
                        spellSlots[4] = 2;
                        break;

                    case 9:
                        spellSlots[5] = 1;
                        break;

                    case 20:
                        spellSlots[7] = 2;
                        break;

                    default:

                        break;
                }
                break;

            case "Fighter":
                spellAttackMod = intelligenceMod; // Using Eldritch Knight
                spellSlots[0] = 999; // Cantrips

                abilities.Add(9); // Second Wind

                if(characterLevel > 1)
                {
                    abilities.Add(10); // Action Surge
                }
                if (characterLevel > 3)
                {
                    spellSlots[1] = 3;
                }
                if (characterLevel > 6)
                {
                    spellSlots[1] = 4;
                    spellSlots[2] = 2;
                }
                if (characterLevel > 9)
                {
                    spellSlots[2] = 3;
                }
                if (characterLevel > 12)
                {
                    spellSlots[3] = 2;
                }
                if (characterLevel > 15)
                {
                    spellSlots[3] = 3;
                }
                if (characterLevel > 16)
                {
                    abilities.Add(9); // Second Wind
                }
                if (characterLevel > 18)
                {
                    spellSlots[4] = 2;
                }


                switch (characterLevel)
                {

                    case 3:
                        spellSlots[1] = 2;
                        break;

                    default:

                        break;
                }
                break;

            case "Monk":
                spellAttackMod = wisdomMod; // Using Way of the Four Elements
                if(characterLevel > 1)
                {
                    classPoints = characterLevel;
                }
                break;

            case "Paladin":
                spellAttackMod = charismaMod;

                spellSlots[0] = 999; // Cantrips

                if (characterLevel > 1)
                {
                    spellSlots[1] = 2;
                }
                if (characterLevel > 2)
                {
                    spellSlots[1] = 3;
                }
                if (characterLevel > 5)
                {
                    spellSlots[1] = 4;
                    spellSlots[2] = 2;
                }
                if (characterLevel > 6)
                {
                    spellSlots[2] = 3;
                }
                if (characterLevel > 8)
                {
                    spellSlots[3] = 2;
                }
                if (characterLevel > 10)
                {
                    spellSlots[3] = 3;
                }
                if (characterLevel > 12)
                {
                    spellSlots[4] = 1;
                }
                if (characterLevel > 14)
                {
                    spellSlots[4] = 2;
                }
                if (characterLevel > 16)
                {
                    spellSlots[4] = 3;
                    spellSlots[5] = 1;
                }
                if (characterLevel > 18)
                {
                    spellSlots[5] = 2;
                }

                break;

            case "Ranger":
                spellAttackMod = wisdomMod;

                if (characterLevel > 1)
                {
                    spellSlots[1] = 2;
                }
                if (characterLevel > 2)
                {
                    spellSlots[1] = 3;
                }
                if (characterLevel > 4)
                {
                    spellSlots[1] = 4;
                    spellSlots[2] = 2;
                }
                if (characterLevel > 6)
                {
                    spellSlots[2] = 3;
                }
                if (characterLevel > 8)
                {
                    spellSlots[3] = 2;
                }
                if (characterLevel > 10)
                {
                    spellSlots[3] = 3;
                }
                if (characterLevel > 12)
                {
                    spellSlots[4] = 1;
                }
                if (characterLevel > 14)
                {
                    spellSlots[4] = 2;
                }
                if (characterLevel > 16)
                {
                    spellSlots[5] = 2;
                }
                if (characterLevel > 18)
                {
                    spellSlots[5] = 2;
                }

                break;

            case "Rogue":
                spellAttackMod = intelligenceMod; // Using arcane trickster
                spellSlots[0] = 999; // Cantrips

                if (characterLevel > 3)
                {
                    spellSlots[1] = 3;
                }
                if (characterLevel > 6)
                {
                    spellSlots[1] = 4;
                    spellSlots[2] = 2;
                }
                if (characterLevel > 9)
                {
                    spellSlots[2] = 3;
                }
                if (characterLevel > 12)
                {
                    spellSlots[3] = 2;
                }
                if (characterLevel > 15)
                {
                    spellSlots[3] = 3;
                }
                if (characterLevel > 18)
                {
                    spellSlots[4] = 2;
                }


                switch (characterLevel)
                {

                    case 3:
                        spellSlots[1] = 2;
                        break;

                    default:

                        break;
                }
                break;

            case "Sorcerer":
                spellAttackMod = charismaMod;
                if (characterLevel > 1)
                {
                    classPoints = characterLevel;
                }

                spellSlots[0] = 999; // Cantrips
                if (characterLevel > 2)
                {
                    spellSlots[1] = 4;
                }
                if (characterLevel > 3)
                {
                    spellSlots[2] = 3;
                }
                if (characterLevel > 5)
                {
                    spellSlots[3] = 3;
                }
                if (characterLevel > 8)
                {
                    spellSlots[4] = 3;
                }
                if (characterLevel > 9)
                {
                    spellSlots[5] = 2;
                }
                if (characterLevel > 10)
                {
                    spellSlots[6] = 1;
                }
                if (characterLevel > 12)
                {
                    spellSlots[7] = 1;
                }
                if (characterLevel > 14)
                {
                    spellSlots[8] = 1;
                }
                if (characterLevel > 16)
                {
                    spellSlots[9] = 1;
                }
                if (characterLevel > 17)
                {
                    spellSlots[5] = 3;
                }
                if (characterLevel > 18)
                {
                    spellSlots[6] = 2;
                }

                switch (characterLevel)
                {
                    case 1:
                        spellSlots[1] = 2;
                        break;

                    case 2:
                        spellSlots[1] = 3;
                        break;

                    case 3:
                        spellSlots[2] = 2;
                        break;

                    case 5:
                        spellSlots[3] = 2;
                        break;

                    case 7:
                        spellSlots[4] = 1;
                        break;

                    case 9:
                        spellSlots[5] = 1;
                        break;

                    default:

                        break;
                }

                break;

            case "Warlock":
                spellAttackMod = charismaMod;
                spellSlots[0] = 999; // Cantrips
                if (characterLevel == 1)
                {
                    spellSlots[1] = 1;
                }
                if (characterLevel > 1)
                {
                    spellSlots[1] = 2;
                }
                if (characterLevel > 10)
                {
                    spellSlots[1] = 3;
                }
                if (characterLevel > 16)
                {
                    spellSlots[1] = 4;
                }
                break;

            case "Wizard":
                spellAttackMod = intelligenceMod;
                spellSlots[0] = 999; // Cantrips
                if (characterLevel > 2)
                {
                    spellSlots[1] = 4;
                }
                if (characterLevel > 3)
                {
                    spellSlots[2] = 3;
                }
                if (characterLevel > 5)
                {
                    spellSlots[3] = 3;
                }
                if (characterLevel > 8)
                {
                    spellSlots[4] = 3;
                }
                if (characterLevel > 9)
                {
                    spellSlots[5] = 2;
                }
                if (characterLevel > 10)
                {
                    spellSlots[6] = 1;
                }
                if (characterLevel > 12)
                {
                    spellSlots[7] = 1;
                }
                if (characterLevel > 14)
                {
                    spellSlots[8] = 1;
                }
                if (characterLevel > 16)
                {
                    spellSlots[9] = 1;
                }
                if (characterLevel > 17)
                {
                    spellSlots[5] = 3;
                }
                if (characterLevel > 18)
                {
                    spellSlots[6] = 2;
                }

                switch (characterLevel)
                {
                    case 1:
                        spellSlots[1] = 2;
                        break;

                    case 2:
                        spellSlots[1] = 3;
                        break;

                    case 3:
                        spellSlots[2] = 2;
                        break;

                    case 5:
                        spellSlots[3] = 2;
                        break;

                    case 7:
                        spellSlots[4] = 1;
                        break;

                    case 9:
                        spellSlots[5] = 1;
                        break;

                    default:

                        break;
                }
                break;
        }
    }

    public void RetrieveAbilitiesFromClassNPC(string classString)
    {
        switch (classString)
        {
            case "Striker":

                break;

            case "Defender":

                break;

            case "Controller":
                spellAttackMod = wisdomMod;
                spellSlots[0] = 999; // Cantrips
                if (characterLevel > 2)
                {
                    spellSlots[1] = 4;
                }
                if (characterLevel > 3)
                {
                    spellSlots[2] = 3;
                }
                if (characterLevel > 5)
                {
                    spellSlots[3] = 3;
                }
                if (characterLevel > 8)
                {
                    spellSlots[4] = 3;
                }
                if (characterLevel > 9)
                {
                    spellSlots[5] = 2;
                }
                if (characterLevel > 10)
                {
                    spellSlots[6] = 1;
                }
                if (characterLevel > 12)
                {
                    spellSlots[7] = 1;
                }
                if (characterLevel > 14)
                {
                    spellSlots[8] = 1;
                }
                if (characterLevel > 16)
                {
                    spellSlots[9] = 1;
                }
                if (characterLevel > 17)
                {
                    spellSlots[5] = 3;
                }
                if (characterLevel > 18)
                {
                    spellSlots[6] = 2;
                }

                switch (characterLevel)
                {
                    case 1:
                        spellSlots[1] = 2;
                        break;

                    case 2:
                        spellSlots[1] = 3;
                        break;

                    case 3:
                        spellSlots[2] = 2;
                        break;

                    case 5:
                        spellSlots[3] = 2;
                        break;

                    case 7:
                        spellSlots[4] = 1;
                        break;

                    case 9:
                        spellSlots[5] = 1;
                        break;

                    default:

                        break;
                }
                break;

            case "Leader":
                spellAttackMod = charismaMod;

                spellSlots[0] = 999; // Cantrips

                if (characterLevel > 1)
                {
                    spellSlots[1] = 2;
                }
                if (characterLevel > 2)
                {
                    spellSlots[1] = 3;
                }
                if (characterLevel > 5)
                {
                    spellSlots[1] = 4;
                    spellSlots[2] = 2;
                }
                if (characterLevel > 6)
                {
                    spellSlots[2] = 3;
                }
                if (characterLevel > 8)
                {
                    spellSlots[3] = 2;
                }
                if (characterLevel > 10)
                {
                    spellSlots[3] = 3;
                }
                if (characterLevel > 12)
                {
                    spellSlots[4] = 1;
                }
                if (characterLevel > 14)
                {
                    spellSlots[4] = 2;
                }
                if (characterLevel > 16)
                {
                    spellSlots[4] = 3;
                    spellSlots[5] = 1;
                }
                if (characterLevel > 18)
                {
                    spellSlots[5] = 2;
                }
                break;
        }
    }
}
