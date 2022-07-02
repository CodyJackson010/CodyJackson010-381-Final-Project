using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DirectAbilitiesFull
{
    /// <summary>
    /// This contains every type of attack available, including; melee, ranged, and spells.
    /// Currently located on CombatManager.
    /// </summary>

    // Do I like this solution? No. Is it user friendly? No. Does it work? Yes.
    // I hate it, but it's the only thing that will work.

    // Fields not updating in the inspector? Try resetting the component. You will have to re-add the sprites though (try copying before you reset it all).

    public class DirectAbilities : MonoBehaviour
    {

        // Attack types
        public string[] attackTypes = new string[] {"acid",         // 0
                                                          "bludgeoning",  // 1
                                                          "cold",         // 2
                                                          "fire",         // 3
                                                          "force",        // 4
                                                          "lightning",    // 5
                                                          "necrotic",     // 6
                                                          "piercing",     // 7
                                                          "poison",       // 8
                                                          "radiant",      // 9
                                                          "slashing",     // 10
                                                          "thunder" };    // 11

        // -- Welcome to the encyclopedia, this is where dreams die. -- //

        public string[] abilityNames = new string[]{

            "Debug Melee Attack", // 0 (Debug)
            "Scimitar",           // 1 (Goblin melee attack)
            "Light Crossbow",     // 2 (Bandit ranged attack)
            "Scorching Ray",      // 3 (Scorching ray, ranged multi-target spell attack)
            "Longsword",          // 4 (Longsword, melee attack) 
            "Fireball",           // 5 (Fireball, spell AOE attack)
            "Cure Wounds",        // 6 (Cure wounds, melee healing spell)
            "Healing Word",       // 7 (Healing word, ranged healing spell)
            "Bless",              // 8 (Bless, ranged spell buff)
            "Second Wind",        // 9 (Second wind, self heal)
            "Action Surge",       // 10 (Action surge, self ability)
            "Firebolt",           // 11 (Firebolt, ranged spell cantrip)
            "Bane",               // 12 (Bane, ranged spell debuff)
            "Sacred Flame",       // 13 (Sacred flame, ranged spell cantrip)
            "Shortbow",           // 14 (Shortbow, basic ranged attack)
            "Club",               // 15 (Club, basic melee attack)
            "Javelin",            // 16 (Javelin, basic thrown ranged attack)
            "Shield of Faith",    // 17 (Shield of Faith, ranged ability buff)
            "Shield",             // 18 (Shield, self spell buff)
            "Chaos Bolt"          // 19 (Chaos Bolt, ranged spell)
        };

        public string[] abilityDescriptions = new string[]
        {
            "(A) A simple melee attack that does a small amount of damage to a single target.", // 0
            "(A) A slashing attack with a scimitar.",                                           // 1
            "(A) A ranged attack using a light crossbow that shoots over a moderate distance.", // 2
            "(A) You create three rays of fire and hurl them at targets within range. " +
            "You can hurl them at one target or several. Make a ranged spell attack for each ray. " +
            "On a hit, the target takes 2d6 fire damage.",                                      // 3
            "(A) A large sword with a long blade, deals 1d8 slashing damage.",                  // 4
            "(A) A bright streak flashes from your pointing finger to a point you choose within range and then " +
            "blossoms with a low roar into an explosion of flame. Each creature in a " +
            "20-foot-radius sphere centered on that point must make a Dexterity saving throw. " +
            "A target takes 8d6 fire damage on a failed save, or half as much damage " +
            "on a successful one.",                                                             // 5
            "(A) A creature you touch regains a number of hit points equal to 1d8 + your" +
            " spellcasting ability modifier.",                                                  // 6
            "(B) A creature of your choice that you can see within range regains hit points " +
            "equal to 1d4 + your spellcasting ability modifier.",                               // 7
            "(A) You bless up to three creatures of your choice within range. Whenever a target" +
            " makes an attack roll or a saving throw before the spell ends, the target can " +
            "roll a d4 and add the number rolled to the attack roll or saving throw.",          // 8
            "(F) You have a limited well of stamina that you can draw on to protect yourself " +
            "from harm. On your turn, you can use a bonus action to regain hit points equal " +
            "to 1d10 + your fighter level. Once you use this feature, you must finish a " +
            "short or long rest before you can use it again.",                                  // 9
            "(F) On your turn, you can take one additional action. Once you use this feature, " +
            "you must finish a short or long rest before you can use it again.",                // 10
            "(A) You hurl a mote of fire at a creature or object within range. " +
            "Make a ranged spell attack against the target. On a hit, the target " +
            "takes 1d10 fire damage.",                                                          // 11
            "(A) Up to three creatures of your choice that you can see within range must make " +
            "Charisma saving throws. Whenever a target that fails this saving throw makes an " +
            "attack roll or a saving throw before the spell ends, the target must roll a d4 " +
            "and subtract the number rolled from the attack roll or saving throw.",             // 12
            "(A) Flame-like radiance descends on a creature that you can see within range. " +
            "The target must succeed on a Dexterity saving throw or take 1d8 radiant damage. " +
            "The target gains no benefit from cover for this saving throw.",                    // 13
            "(A) A basic ranged attack with a shortbow, deals 1d6 piercing damage.",            // 14
            "(A) A basic melee attack with a wooden club, deals 1d4 bludgening damage.",        // 15
            "(A) A simple javelin thrown at an attacker, deals 1d6 piercing damage.",           // 16
            "(B) A shimmering field appears and surrounds a creature of your choice " +
            "within range, granting it a +2 bonus to AC for the duration.",                     // 17
            "(R) An invisible barrier of magical force appears and protects you. Until the " +
            "start of your next turn, you have a +5 bonus to AC, including against the " +
            "triggering attack, and you take no damage from magic missile.",                    // 18
            "(A) You hurl an undulating, warbling mass of chaotic energy at one creature in " +
            "range. Make a ranged spell attack against the target. On a hit, the target " +
            "takes 2d8 + 1d6 damage. Choose one of the d8s. The number rolled on that " +
            "die determines the attack's damage type."                                          // 19
        };

        public Sprite[] abilityIcon = new Sprite[20];
        // 0 - Debug Image
        // 1 - Twisted dagger
        // 2 - Light Crossbow
        // 3 - Scorching Ray
        // 4 - Longsword
        // 5 - Fireball
        // 6 - Cure Wounds
        // 7 - Healing word
        // 8 - Bless
        // 9 - Second Wind
        // 10 - Action Surge
        // 11 - Firebolt
        // 12 - Bane
        // 13 - Sacred Flame
        // 14 - Shortbow
        // 15 - Club
        // 16 - Javelin
        // 17 - Shield of Faith
        // 18 - Shield
        // 19 - Chaos Bolt
        // # - ???

        /// <summary>
        /// The minimum required ranged a target must be in to be able to reach with this attack (in ft).
        /// 0 = Melee | -1 = Self
        /// </summary>
        public int[] requiredRange = new int[]
        {
            5,   // 0
            5,   // 1
            80,  // 2
            120, // 3
            0,   // 4
            150, // 5
            5,   // 6
            60,  // 7
            30,  // 8
            -1,  // 9
            -1,  // 10
            120, // 11
            30,  // 12
            60,  // 13
            80,  // 14
            5,   // 15
            30,  // 16
            60,  // 17
            -1,  // 18
            120  // 19
        };

        /// <summary>
        /// The maximum possible range this attack can be made from, shooting from extreme distances causes disadvantage for the attacker!
        /// </summary>
        public int[] maxRange = new int[]
        {
            // Where 0 = Melee ranged or not needed
            0,    // 0
            0,    // 1
            320,  // 2
            0,    // 3
            0,    // 4
            0,    // 5
            0,    // 6
            0,    // 7
            0,    // 8
            0,    // 9
            0,    // 10
            0,    // 11
            0,    // 12
            0,    // 13
            320,  // 14
            0,    // 15
            120,  // 16
            0,    // 17
            0,    // 18
            0     // 19
        };

        /// <summary>
        /// The type of attack this is; melee, ranged, spell (single target), spell (multi target), spell (AOE), etc.
        /// MS - Melee Single | M-# - Multi Melee | RS - Ranged Single | RM-# - Ranged Multi(#) | SS - Spell Single | S-# - Spell Multi(#) | SA - Spell AOE
        /// ES - Effect Single | E-# Effect Multi | EA - Effect Aoe
        /// </summary>
        public string[] targetType = new string[]
        {
            "ms",  // 0
            "ms",  // 1
            "rs",  // 2
            "s-3", // 3
            "ms",  // 4
            "sa",  // 5
            "es",  // 6
            "es",  // 7
            "e-3", // 8
            "es",  // 9
            "es",  // 10
            "ss",  // 11
            "e-3", // 12
            "ss",  // 13
            "rs",  // 14
            "ms",  // 15
            "rs",  // 16
            "es",  // 17
            "es",  // 18
            "ss"   // 19
        };

        /// <summary>
        /// If this is an AOE attack, how big is the radius? 0 = not AOE
        /// </summary>
        public int[] targetRadius = new int[]
        {
            0, // 0
            0, // 1
            0, // 2
            0, // 3
            0, // 4
            20,// 5
            0, // 6
            0, // 7
            0, // 8
            0, // 9
            0, // 10
            0, // 11
            0, // 12
            0, // 13
            0, // 14
            0, // 15
            0, // 16
            0, // 17
            0, // 18
            0  // 19
        };

        /// <summary>
        /// If this is a spell, what kind of spell slot does it require? 20 == None
        /// </summary>
        public int[] neededSpellSlot = new int[]
        {
            20,  // 0
            20,  // 1
            20,  // 2
            2,   // 3
            20,  // 4
            3,   // 5
            1,   // 6
            1,   // 7
            1,   // 8
            20,  // 9
            20,  // 10
            0,   // 11
            1,   // 12
            0,   // 13
            20,  // 14
            20,  // 15
            20,  // 16
            1,   // 17
            1,   // 18
            1    // 19
        };

        /// <summary>
        /// Intent of the attack. 0 = Harm, 1 = Aid, 2 = Self
        /// </summary>
        public int[] abilityIntent = new int[]
        {
            0,  // 0
            0,  // 1
            0,  // 2
            0,  // 3
            0,  // 4
            0,  // 5
            1,  // 6
            1,  // 7
            1,  // 8
            2,  // 9
            2,  // 10
            0,  // 11
            0,  // 12
            0,  // 13
            0,  // 14
            0,  // 15
            0,  // 16
            1,  // 17
            2,  // 18
            0   // 19
        };

        /// <summary>
        /// What is required to use this ability? Action, bonus action, reaction, free (split by a "-") Spell slot #?
        /// Uses[a, b, r, f] Ex: a-2
        /// </summary>
        public string[] costOfUse = new string[]
        {
            "a",    // 0
            "a",    // 1
            "a",    // 2
            "a-2",  // 3
            "a",    // 4
            "a-3",  // 5
            "a-1",  // 6
            "b-1",  // 7
            "a-1",  // 8
            "f",    // 9
            "f",    // 10
            "a-0",  // 11
            "a-1",  // 12
            "a-0",  // 13
            "a",    // 14
            "a",    // 15
            "a",    // 16
            "b-1",  // 17
            "r-1",  // 18
            "a-1"   // 19
        };

        /// <summary>
        /// Can this ability only be used a certain number of times? (Daily cooldown?)
        /// -1 is no limits, -5 is tied to spell slots
        /// </summary>
        public int[] limitedUse = new int[]
        {
            -1,  // 0
            -1,  // 1
            -1,  // 2
            -5,  // 3
            -1,  // 4
            -5,  // 5
            -5,  // 6
            -5,  // 7
            -5,  // 8
            1,   // 9
            1,   // 10
            -5,  // 11
            -5,  // 12
            -5,  // 13
            -1,  // 14
            -1,  // 15
            -1,  // 16
            -5,  // 17
            -5,  // 18
            -5   // 19
        };
    }


    /*
    public class Ranged : MonoBehaviour
    {
        // Attack types
        public string[] attackTypes = new string[] {"acid",         // 0
                                                    "bludgeoning",  // 1
                                                    "cold",         // 2
                                                    "fire",         // 3
                                                    "force",        // 4
                                                    "lightning",    // 5
                                                    "necrotic",     // 6
                                                    "piercing",     // 7
                                                    "poison",       // 8
                                                    "radiant",      // 9
                                                    "slashing",     // 10
                                                    "thunder" };    // 11

        public string abilityName;
        public string abilityDesc;
        public Image abilityIcon;
        public string attackType;
        public Image[] abilityIconList = new Image[50];

        public Dictionary<string, Ranged> rangedAbilities = new Dictionary<string, Ranged>();

        public void Awake() // Set up all the abilities
        {
            Ranged newAttack = new Ranged("Debug Attack", "Basic debug melee attack.", 0, 10);
            rangedAbilities.Add("Debug Melee", newAttack);



        }

        public Ranged(string name, string desc, int iconID, int typeID)
        {
            abilityName = name;
            abilityDesc = desc;
            abilityIcon = abilityIconList[iconID];
            attackType = attackTypes[typeID];
        }
    }
    */


    /*
    public class DirectAbilities : MonoBehaviour
    {
        // Attack types
        public string[] attackTypes = new string[] {"acid",         // 0
                                                    "bludgeoning",  // 1
                                                    "cold",         // 2
                                                    "fire",         // 3
                                                    "force",        // 4
                                                    "lightning",    // 5
                                                    "necrotic",     // 6
                                                    "piercing",     // 7
                                                    "poison",       // 8
                                                    "radiant",      // 9
                                                    "slashing",     // 10
                                                    "thunder" };    // 11

        public string abilityName;
        public string abilityDesc;
        public Image abilityIcon;
        public string attackType;
        public Image[] abilityIconList = new Image[50];

        public Dictionary<string, Melee> meleeAbilities = new Dictionary<string, Melee>();
        public Dictionary<string, Ranged> rangedAbilities = new Dictionary<string, Ranged>();
        public Dictionary<string, Spell> spellAbilities = new Dictionary<string, Spell>();
        public Dictionary<string, Ability> genericAbilities = new Dictionary<string, Ability>();

        // -------------------------------------------------------------------------------------------------------- //

        public void Awake() // Set up all the abilities
        {
            // --- MELEE
            Melee newAttack = new Melee ("Debug Attack", "Basic debug melee attack.", 0, 10);
            meleeAbilities.Add("Debug Melee", newAttack);
            // --- RANGED

            // --- SPELL



            // --- ABILITY



        }

        public class Melee
        {
            
            public string abilityName;
            public string abilityDesc;
            public Image abilityIcon;
            public string attackType;
            public Image[] abilityIconList = new Image[10];
            public string[] attackTypes = new string[] {"acid",         // 0
                                                    "bludgeoning",  // 1
                                                    "cold",         // 2
                                                    "fire",         // 3
                                                    "force",        // 4
                                                    "lightning",    // 5
                                                    "necrotic",     // 6
                                                    "piercing",     // 7
                                                    "poison",       // 8
                                                    "radiant",      // 9
                                                    "slashing",     // 10
                                                    "thunder" };    // 11

            
            public Melee(string name, string desc, int iconID, int typeID)
            {
                abilityName = name;
                abilityDesc = desc;
                abilityIcon = abilityIconList[iconID];
                attackType = attackTypes[typeID];
            }
        }

        public class Ranged
        {
            public float regularDist; // For ranged attacks
            public float longDist; // For ranged attacks


        }

        public class Spell
        {
            public int[] spellSlotLvl = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            public string[] duration = new string[] { "Instantaneous", "1 minute", "10 minutes", "1 hour" }; // Goes up to 24 hours but not necessary for scope
            public string[] targetType = new string[] { "Single", "Split", "AOE" };
            public float maxSpellRange; // For spell attacks


        }

        public class Ability
        {

        }
    }
    */
}