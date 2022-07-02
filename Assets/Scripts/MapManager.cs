using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapManager : MonoBehaviour
{
    /// <summary>
    /// This script is largely responsible for map generation. Building the grid in different ways, assigning weight to specific tiles, and
    /// organizing tiles automatically.It also places player characters and enemy characters.
    /// </summary>

    public int rows;
    public int columns;
    public int height;
    int locHeight; // Local Height
    protected int scale = 1; // Units between each tile [!FOR THE LOVE OF GOD DO NOT CHANGE THIS IT WILL BREAK EVERYTHING!]

    public GameObject tilePref; // The tile being used for movement and navigation
    public GameObject worldCubePref; // An untargetable/unwalkable cube to be placed in the world


    public GameObject cameraRef; // Reference to Camera, relocate it for different maps

    Vector3 bottomLeftLocation = new Vector3(0, 0, 0);

    public GameObject[,] gridArray; // 2D Array of all tiles
    public GameObject[,,] tileArray; // 3D Array of all tiles
    public GameObject[,,] worldCubeArray; // 3D Array of all world Cubes

    public float characterHalfHeight = 1.45f; // ~The centerpoint of any character. Used for spawning

    public GameObject playerParent; // Parent for all player characters
    public GameObject enemyParent; // Parent for all enemy characters
    public GameObject gridParent; // Parent for tiles
    public GameObject objParent; // Parent for all world objects

    public GameObject lightRef; // Reference to sun

    // -------------- CHARACTERS ------------------------------------- //

    // < ENEMIES >

    public GameObject melee_enemyPref; // Prefab for a melee enemy
    public GameObject ranged_enemyPref; // Prefab for a ranged enemy
    public GameObject target_enemyPref; // Prefab for a high hp/low hit enemy

    public GameObject goblin_Pref; // Basic Goblin
    public GameObject goblinHealer_Pref; // Healer Goblin

    // < PLAYERS >

    public GameObject playerPref; // The player being placed
    public GameObject clericKremPref; // Cleric Krem
    public GameObject palandinPref; // Paladin
    public GameObject paladinLL; // Lvl1 Paladin
    public GameObject fighterLL; // Lvl1 Fighter
    public GameObject clericLL; // Lvl1 cleric
    public GameObject sorcererLL; // Lvl sorcerer (my boy Adrian)

    // -------------- OBJECTS --------------------------------------- //

    // < TREES >

    public GameObject tree1;
    public GameObject tree2;

    // --------------------------------------------------------------- //

    public static MapManager inst;
    public int mapChoice = 3; // Which map to load

    void Awake()
    {
        inst = this;

        gridParent = new GameObject("GridParent");
        enemyParent = new GameObject("EnemyParent");
        playerParent = new GameObject("PlayerParent");
        objParent = new GameObject("ObjectParent");
        gridParent.transform.parent = this.transform;
        enemyParent.transform.parent = this.transform;
        playerParent.transform.parent = this.transform;
        objParent.transform.parent = this.transform;

        mapChoice = PlayerPrefs.GetInt("encounter");

        switch (mapChoice)
        {
            case 1:
                GM_Debug();
                break;

            case 2:
                lightRef.GetComponent<Light>().intensity = 1f;
                GM_Forest();
                break;

            case 3:
                lightRef.GetComponent<Light>().intensity = 0.5f;
                GM_Citadel();
                break;

            case 4:
                GM_Hermit();
                break;

            default:
                Debug.LogError("Invalid mapChoice value.");
                break;
        }

        /*
        if (tilePref)
        {
            //GM_Debug(); // Generate the "Debug" Map
            GM_Forest(); // Generate the "Forest" Map
        }
        else
        {
            print("missing prefab for tiles, please assign one.");
        }
        */
    }

    void GM_Debug()
    {
        cameraRef.transform.position = new Vector3(7, 0, 7);

        columns = rows = 14;
        gridArray = new GameObject[columns, rows];

        // Spawn in a 14x14 Platform

        for (int i = 0; i < columns; i++)
        {
            for (int j = 0; j < rows; j++)
            {
                GameObject obj = Instantiate(tilePref, new Vector3(bottomLeftLocation.x + i, bottomLeftLocation.y, bottomLeftLocation.z + j), Quaternion.identity);
                obj.transform.parent = gridParent.transform;
                gridArray[i, j] = obj;
            }
        }

        // Custom Geometry

        RelocateTile(0, 0, 0, 1, 0);

        RelocateTile(7, 3, 0, 1, 0);
        RelocateTile(7, 4, 0, 2, 0);
        RelocateTile(7, 5, 0, 3, 0);
        RelocateTile(7, 6, 0, 3, 0);
        RelocateTile(7, 7, 0, 3, 0);
        RelocateTile(7, 8, 0, 2, 0);
        RelocateTile(7, 9, 0, 1, 0);

        // Add weight to certain tiles

        /*
         *  Be careful with setting weights too high. 
         *  A value of 5 is enough to make an character avoid it. 
         *  Use 50 to make a tile is unwalkable.
         */

        SetTileWeight(4, 4, 5);
        SetTileWeight(5, 4, 5);
        SetTileWeight(6, 4, 5);

        SetTileWeight(9, 4, 50);
        SetTileWeight(10, 4, 50);
        SetTileWeight(11, 4, 50);
        SetTileWeight(9, 5, 50);
        SetTileWeight(9, 6, 50);

        /*
        SetTileWeight(5, 1, 50);
        SetTileWeight(4, 1, 50);
        SetTileWeight(3, 1, 50);
        SetTileWeight(5, 0, 50);
        SetTileWeight(3, 0, 50);
        */

        // Spawn characters in

        PlacePlayerCharacter(4, 0, playerParent);
        //PlacePlayerCharacter(9, 0, playerParent);
        PlaceEnemyCharacter(8, 13, enemyParent, melee_enemyPref);
        PlaceEnemyCharacter(2, 13, enemyParent, melee_enemyPref);

        //SetTileMaterial(7, 7, "dirt");
    }

    /*
     * ================
     * j
     * ^
     * i, j -> i
     * ================
     */

    void GM_Forest()
    {
        cameraRef.transform.position = new Vector3(9, 2, 9);

        rows = 18;
        columns = 24;
        height = 8;

        string texture = "grass3";

        tileArray = new GameObject[columns, height, rows];
        worldCubeArray = new GameObject[columns, height, rows];

        // Begin by spawning rectangular areas.

        // Area 1 (Top Left, 6x8, Height 5) - Width x Height

        for (int i = 0; i < 6; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                CreateTileHere(i, 5, j + 10);
                SetTileMaterial3D(i, 5, j + 10, texture);
            }
        }

        // Area 5 (Top right of Area 1, 4x4, Height 5)

        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                CreateTileHere(i + 6, 5, j + 14);
                SetTileMaterial3D(i + 6, 5, j + 14, texture);
            }
        }

        // Area 6 (Bottom Left, 6x5, Height 2)

        for (int i = 0; i < 6; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                CreateTileHere(i, 2, j);
                SetTileMaterial3D(i, 2, j, texture);
            }
        }

        // Area 7 (To the right of Area 6, 5x10, Height 2)

        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                CreateTileHere(i + 6, 2, j);
                SetTileMaterial3D(i + 6, 2, j, texture);
            }
        }

        // Area 2 (Central Top to Bottom, 7x18, Height 2)

        for (int i = 0; i < 7; i++)
        {
            for (int j = 0; j < 18; j++)
            {
                CreateTileHere(i + 11, 2, j);
                SetTileMaterial3D(i + 11, 2, j, texture);
            }
        }

        // Area 3 (Top Right of Area 2, 4x4, Height 2)

        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                CreateTileHere(i + 18, 2, j + 14);
                SetTileMaterial3D(i + 18, 2, j + 14, texture);
            }
        }

        // Area 5 (Bottom Right, 5x8, Height 0)

        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                CreateTileHere(i + 19, 0, j);
                SetTileMaterial3D(i + 19, 0, j, texture);
            }
        }

        // Area 5 (On top of Area 5, 2x6, Height 0)

        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                CreateTileHere(i + 22, 0, j + 8);
                SetTileMaterial3D(i + 22, 0, j + 8, texture);
            }
        }

        // Slope up from Area 6 to 1

        CreateTileHere(0, 2, 5);
        SetTileMaterial3D(0, 2, 5, texture);
        CreateTileHere(1, 2, 5);
        SetTileMaterial3D(1, 2, 5, texture);

        CreateTileHere(0, 3, 6);
        SetTileMaterial3D(0, 3, 6, texture);
        CreateTileHere(1, 3, 6);
        SetTileMaterial3D(1, 3, 6, texture);

        CreateTileHere(0, 3, 7);
        SetTileMaterial3D(0, 3, 7, texture);
        CreateTileHere(1, 3, 7);
        SetTileMaterial3D(1, 3, 7, texture);

        CreateTileHere(0, 4, 8);
        SetTileMaterial3D(0, 4, 8, texture);
        CreateTileHere(1, 4, 8);
        SetTileMaterial3D(1, 4, 8, texture);

        CreateTileHere(0, 5, 9);
        SetTileMaterial3D(0, 5, 9, texture);
        CreateTileHere(1, 5, 9);
        SetTileMaterial3D(1, 5, 9, texture);
        CreateTileHere(2, 5, 9);
        SetTileMaterial3D(2, 5, 9, texture);
        CreateTileHere(3, 5, 9);
        SetTileMaterial3D(3, 5, 9, texture);

        CreateTileHere(4, 5, 9); // Extra cliff
        SetTileMaterial3D(4, 5, 9, texture);
        CreateTileHere(5, 5, 9);
        SetTileMaterial3D(5, 5, 9, texture);

        // Angle below Area 8

        locHeight = 5;

        CreateTileHere(6, locHeight, 13);
        SetTileMaterial3D(6, locHeight, 13, texture);
        CreateTileHere(6, locHeight, 12);
        SetTileMaterial3D(6, locHeight, 12, texture);
        CreateTileHere(6, locHeight, 11);
        SetTileMaterial3D(6, locHeight, 11, texture);
        CreateTileHere(6, locHeight, 10);
        SetTileMaterial3D(6, locHeight, 10, texture);

        CreateTileHere(7, locHeight, 13);
        SetTileMaterial3D(7, locHeight, 13, texture);
        CreateTileHere(7, locHeight, 12);
        SetTileMaterial3D(7, locHeight, 12, texture);
        CreateTileHere(7, locHeight, 11);
        SetTileMaterial3D(7, locHeight, 11, texture);

        CreateTileHere(8, locHeight, 13);
        SetTileMaterial3D(8, locHeight, 13, texture);
        CreateTileHere(8, locHeight, 12);
        SetTileMaterial3D(8, locHeight, 12, texture);

        CreateTileHere(9, locHeight, 13);
        SetTileMaterial3D(9, locHeight, 13, texture);

        // Angle adjoining the previous angle + 4 blocks on top

        locHeight = 2;

        CreateTileHere(7, locHeight, 10);
        SetTileMaterial3D(7, locHeight, 10, texture);

        CreateTileHere(8, locHeight, 10);
        SetTileMaterial3D(8, locHeight, 10, texture);
        CreateTileHere(8, locHeight, 11);
        SetTileMaterial3D(8, locHeight, 11, texture);

        CreateTileHere(9, locHeight, 10);
        SetTileMaterial3D(9, locHeight, 10, texture);
        CreateTileHere(9, locHeight, 11);
        SetTileMaterial3D(9, locHeight, 11, texture);
        CreateTileHere(9, locHeight, 12);
        SetTileMaterial3D(9, locHeight, 12, texture);

        CreateTileHere(10, locHeight, 10);
        SetTileMaterial3D(10, locHeight, 10, texture);
        CreateTileHere(10, locHeight, 11);
        SetTileMaterial3D(10, locHeight, 11, texture);
        CreateTileHere(10, locHeight, 12);
        SetTileMaterial3D(10, locHeight, 12, texture);
        CreateTileHere(10, locHeight, 13);
        SetTileMaterial3D(10, locHeight, 13, texture);

        CreateTileHere(10, locHeight, 14);
        SetTileMaterial3D(10, locHeight, 14, texture);
        CreateTileHere(10, locHeight, 15);
        SetTileMaterial3D(10, locHeight, 15, texture);
        CreateTileHere(10, locHeight, 16);
        SetTileMaterial3D(10, locHeight, 16, texture);
        CreateTileHere(10, locHeight, 17);
        SetTileMaterial3D(10, locHeight, 17, texture);

        // Area to the right of the boulders above Area 6

        locHeight = 2;

        CreateTileHere(4, locHeight, 5);
        SetTileMaterial3D(4, locHeight, 5, texture);
        CreateTileHere(4, locHeight, 6);
        SetTileMaterial3D(4, locHeight, 6, texture);

        CreateTileHere(5, locHeight, 5);
        SetTileMaterial3D(5, locHeight, 5, texture);
        CreateTileHere(5, locHeight, 6);
        SetTileMaterial3D(5, locHeight, 6, texture);
        CreateTileHere(5, locHeight, 7);
        SetTileMaterial3D(5, locHeight, 7, texture);
        CreateTileHere(5, locHeight, 8);
        SetTileMaterial3D(5, locHeight, 8, texture);

        // Area below Area 3

        CreateTileHere(18, locHeight, 13);
        SetTileMaterial3D(18, locHeight, 13, texture);
        CreateTileHere(19, locHeight, 13);
        SetTileMaterial3D(19, locHeight, 13, texture);
        CreateTileHere(20, locHeight, 13);
        SetTileMaterial3D(20, locHeight, 13, texture);
        CreateTileHere(21, locHeight, 13);
        SetTileMaterial3D(21, locHeight, 13, texture);

        CreateTileHere(18, locHeight, 12);
        SetTileMaterial3D(18, locHeight, 12, texture);
        CreateTileHere(19, locHeight, 12);
        SetTileMaterial3D(19, locHeight, 12, texture);
        CreateTileHere(20, locHeight, 12);
        SetTileMaterial3D(20, locHeight, 12, texture);
        CreateTileHere(21, locHeight, 12);
        SetTileMaterial3D(21, locHeight, 12, texture);

        CreateTileHere(18, locHeight, 11);
        SetTileMaterial3D(18, locHeight, 11, texture);
        CreateTileHere(19, locHeight, 11);
        SetTileMaterial3D(19, locHeight, 11, texture);
        CreateTileHere(20, locHeight, 11);
        SetTileMaterial3D(20, locHeight, 11, texture);

        CreateTileHere(18, locHeight, 10);
        SetTileMaterial3D(18, locHeight, 10, texture);
        CreateTileHere(19, locHeight, 10);
        SetTileMaterial3D(19, locHeight, 10, texture);
        CreateTileHere(20, locHeight, 10);
        SetTileMaterial3D(20, locHeight, 10, texture);

        CreateTileHere(18, locHeight, 9);
        SetTileMaterial3D(18, locHeight, 9, texture);
        CreateTileHere(19, locHeight, 9);
        SetTileMaterial3D(19, locHeight, 9, texture);
        CreateTileHere(20, locHeight, 9);
        SetTileMaterial3D(20, locHeight, 9, texture);

        CreateTileHere(18, locHeight, 8);
        SetTileMaterial3D(18, locHeight, 8, texture);
        CreateTileHere(19, locHeight, 8);
        SetTileMaterial3D(19, locHeight, 8, texture);

        CreateTileHere(18, locHeight, 7);
        SetTileMaterial3D(18, locHeight, 7, texture);

        // Slim area below previous area

        CreateTileHere(18, locHeight, 3);
        SetTileMaterial3D(18, locHeight, 3, texture);
        CreateTileHere(18, locHeight, 2);
        SetTileMaterial3D(18, locHeight, 2, texture);
        CreateTileHere(18, locHeight, 1);
        SetTileMaterial3D(18, locHeight, 1, texture);
        CreateTileHere(18, locHeight, 0);
        SetTileMaterial3D(18, locHeight, 0, texture);

        // Area to the right of Area 3

        CreateTileHere(22, locHeight, 17);
        SetTileMaterial3D(22, locHeight, 17, texture);
        CreateTileHere(22, locHeight, 16);
        SetTileMaterial3D(22, locHeight, 16, texture);
        CreateTileHere(22, locHeight, 15);
        SetTileMaterial3D(22, locHeight, 15, texture);
        CreateTileHere(22, locHeight, 14);
        SetTileMaterial3D(22, locHeight, 14, texture);

        locHeight = 0;

        CreateTileHere(23, locHeight, 17);
        SetTileMaterial3D(23, locHeight, 17, texture);
        CreateTileHere(23, locHeight, 16);
        SetTileMaterial3D(23, locHeight, 16, texture);
        CreateTileHere(23, locHeight, 15);
        SetTileMaterial3D(23, locHeight, 15, texture);
        CreateTileHere(23, locHeight, 14);
        SetTileMaterial3D(23, locHeight, 14, texture);

        // - Lower 3 Area to the left of Area 5

        CreateTileHere(18, locHeight + 1, 4);
        SetTileMaterial3D(18, locHeight + 1, 4, texture);
        CreateTileHere(18, locHeight, 5);
        SetTileMaterial3D(18, locHeight, 5, texture);
        CreateTileHere(18, locHeight, 6);
        SetTileMaterial3D(18, locHeight, 6, texture);

        // - Other triangle area to the left of Area 4

        CreateTileHere(20, locHeight, 8);
        SetTileMaterial3D(20, locHeight, 8, texture);

        CreateTileHere(21, locHeight, 8);
        SetTileMaterial3D(21, locHeight, 8, texture);
        CreateTileHere(21, locHeight, 9);
        SetTileMaterial3D(21, locHeight, 9, texture);
        CreateTileHere(21, locHeight, 10);
        SetTileMaterial3D(21, locHeight, 10, texture);
        CreateTileHere(21, locHeight, 11);
        SetTileMaterial3D(21, locHeight, 11, texture);

        //
        // Create the boulders on the left
        //

        texture = "andesite";

        CreateWorldCubeHere(2, 3, 5, texture);
        CreateWorldCubeHere(3, 3, 5, texture);
        CreateWorldCubeHere(2, 4, 5, texture);
        CreateWorldCubeHere(3, 4, 5, texture);

        CreateWorldCubeHere(2, 3, 6, texture);
        CreateWorldCubeHere(3, 3, 6, texture);
        CreateWorldCubeHere(2, 4, 6, texture);
        CreateWorldCubeHere(3, 4, 6, texture);
        CreateWorldCubeHere(2, 5, 6, texture);

        CreateWorldCubeHere(2, 3, 7, texture);
        CreateWorldCubeHere(3, 3, 7, texture);
        CreateWorldCubeHere(2, 4, 7, texture);
        CreateWorldCubeHere(3, 4, 7, texture);
        CreateWorldCubeHere(2, 5, 7, texture);
        CreateWorldCubeHere(3, 5, 7, texture);

        CreateWorldCubeHere(2, 3, 8, texture);
        CreateWorldCubeHere(3, 3, 8, texture);
        CreateWorldCubeHere(2, 4, 8, texture);
        CreateWorldCubeHere(3, 4, 8, texture);
        CreateWorldCubeHere(2, 5, 8, texture);
        CreateWorldCubeHere(3, 5, 8, texture);
        CreateWorldCubeHere(2, 6, 8, texture);
        CreateWorldCubeHere(3, 6, 8, texture);

        CreateWorldCubeHere(4, 3, 7, texture);
        CreateWorldCubeHere(4, 3, 8, texture);
        CreateWorldCubeHere(4, 4, 7, texture);
        CreateWorldCubeHere(4, 4, 8, texture);
        CreateWorldCubeHere(4, 5, 8, texture);

        // - Wall along the left side face

        CreateWorldCubeHere(4, 3, 9, texture);
        CreateWorldCubeHere(4, 4, 9, texture);

        CreateWorldCubeHere(5, 3, 9, texture);
        CreateWorldCubeHere(5, 4, 9, texture);

        CreateWorldCubeHere(6, 3, 10, texture);
        CreateWorldCubeHere(6, 4, 10, texture);

        CreateWorldCubeHere(7, 3, 11, texture);
        CreateWorldCubeHere(7, 4, 11, texture);

        CreateWorldCubeHere(8, 3, 12, texture);
        CreateWorldCubeHere(8, 4, 12, texture);

        CreateWorldCubeHere(9, 3, 13, texture);
        CreateWorldCubeHere(9, 4, 13, texture);
        CreateWorldCubeHere(9, 3, 14, texture);
        CreateWorldCubeHere(9, 4, 14, texture);
        CreateWorldCubeHere(9, 3, 15, texture);
        CreateWorldCubeHere(9, 4, 15, texture);
        CreateWorldCubeHere(9, 3, 16, texture);
        CreateWorldCubeHere(9, 4, 16, texture);
        CreateWorldCubeHere(9, 3, 17, texture);
        CreateWorldCubeHere(9, 4, 17, texture);

        // - Wall along the right face

        CreateWorldCubeHere(18, 1, 0, texture);
        CreateWorldCubeHere(18, 1, 1, texture);
        CreateWorldCubeHere(18, 1, 2, texture);
        CreateWorldCubeHere(18, 1, 3, texture);

        CreateWorldCubeHere(17, 1, 4, texture);
        CreateWorldCubeHere(17, 1, 5, texture);
        CreateWorldCubeHere(17, 1, 6, texture);

        CreateWorldCubeHere(18, 1, 7, texture);

        CreateWorldCubeHere(19, 1, 8, texture);

        CreateWorldCubeHere(20, 1, 9, texture);
        CreateWorldCubeHere(20, 1, 10, texture);
        CreateWorldCubeHere(20, 1, 11, texture);

        CreateWorldCubeHere(21, 1, 12, texture);
        CreateWorldCubeHere(21, 1, 13, texture);

        CreateWorldCubeHere(22, 1, 14, texture);
        CreateWorldCubeHere(22, 1, 15, texture);
        CreateWorldCubeHere(22, 1, 16, texture);
        CreateWorldCubeHere(22, 1, 17, texture);

        // --- Place Trees

        
        texture = "oak_log_side";

        RemoveTile3D(5, 2, 0);
        CreateWorldCubeHere(5, 3, 0, texture);
        CreateWorldCubeHere(5, 4, 0, texture);
        CreateObjectHere(5, 3, 0, tree1);

        RemoveTile3D(16, 2, 7);
        CreateWorldCubeHere(16, 3, 7, texture);
        CreateWorldCubeHere(16, 4, 7, texture);
        CreateObjectHere(16, 3, 7, tree2);

        RemoveTile3D(17, 2, 11);
        CreateWorldCubeHere(17, 3, 11, texture);
        CreateWorldCubeHere(17, 4, 11, texture);
        CreateObjectHere(17, 3, 11, tree2);

        RemoveTile3D(15, 2, 9);
        CreateWorldCubeHere(15, 3, 9, texture);
        CreateWorldCubeHere(15, 4, 9, texture);
        CreateObjectHere(15, 3, 9, tree1);

        RemoveTile3D(14, 2, 17);
        CreateWorldCubeHere(14, 3, 17, texture);
        CreateWorldCubeHere(14, 4, 17, texture);
        CreateObjectHere(14, 3, 17, tree1);

        RemoveTile3D(3, 5, 13);
        CreateWorldCubeHere(3, 6, 13, texture);
        CreateWorldCubeHere(3, 7, 13, texture);
        CreateObjectHere(3, 6, 13, tree2);

        // Make path

        texture = "coarse_dirt";

        SetTileMaterial3D(9, 2, 0, texture);
        SetTileMaterial3D(9, 2, 1, texture);
        SetTileMaterial3D(10, 2, 2, texture);
        SetTileMaterial3D(10, 2, 3, texture);
        SetTileMaterial3D(11, 2, 4, texture);
        SetTileMaterial3D(11, 2, 5, texture);
        SetTileMaterial3D(11, 2, 6, texture);
        SetTileMaterial3D(11, 2, 7, texture);
        SetTileMaterial3D(11, 2, 8, texture);
        SetTileMaterial3D(12, 2, 9, texture);
        SetTileMaterial3D(12, 2, 10, texture);
        SetTileMaterial3D(12, 2, 11, texture);
        SetTileMaterial3D(12, 2, 12, texture);
        SetTileMaterial3D(13, 2, 13, texture);
        SetTileMaterial3D(14, 2, 14, texture);
        SetTileMaterial3D(15, 2, 15, texture);
        SetTileMaterial3D(16, 2, 15, texture);
        SetTileMaterial3D(17, 2, 16, texture);
        SetTileMaterial3D(18, 2, 17, texture);
        SetTileMaterial3D(19, 2, 17, texture);


        // Spawn characters in

        PlacePlayerCharacter3D(9, 2, 2, playerParent, playerPref);
        PlacePlayerCharacter3D(12, 2, 1, playerParent, clericKremPref);
        PlacePlayerCharacter3D(8, 2, 3, playerParent, palandinPref);

        PlaceEnemyCharacter3D(14, 2, 14, enemyParent, melee_enemyPref, 180);
        PlaceEnemyCharacter3D(15, 2, 16, enemyParent, melee_enemyPref, 180);
        PlaceEnemyCharacter3D(20, 2, 13, enemyParent, melee_enemyPref, 180);
        PlaceEnemyCharacter3D(8, 5, 15, enemyParent, ranged_enemyPref, 180);

        //PlaceEnemyCharacter3D(14, 2, 14, enemyParent, target_enemyPref, 180);

    }


    void GM_Citadel()
    {
        cameraRef.transform.position = new Vector3(15, 0, 8);

        rows = 20;
        columns = 30;
        height = 4;

        string texture = "smooth_stone";

        tileArray = new GameObject[columns, height, rows];
        worldCubeArray = new GameObject[columns, height, rows];

        // Begin by spawning rectangular areas.

        // Area 1 (4x4) - Width x Height

        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                CreateTileHere(i + 2, 0, j + 1);
                SetTileMaterial3D(i + 2, 0, j + 1, texture);
            }
        }

        // Area 2 (11x8) - Width x Height

        for (int i = 0; i < 11; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                CreateTileHere(i + 7, 0, j + 1);
                SetTileMaterial3D(i + 7, 0, j + 1, texture);
            }
        }

        // Area 3 (6x4) - Width x Height

        for (int i = 0; i < 6; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                CreateTileHere(i + 12, 0, j + 9);
                SetTileMaterial3D(i + 12, 0, j + 9, texture);
            }
        }

        // Area 4 (4x4) - Width x Height

        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                CreateTileHere(i + 19, 0, j);
                SetTileMaterial3D(i + 19, 0, j, texture);
            }
        }

        // Area 5 (10x4) - Width x Height

        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                CreateTileHere(i + 19, 0, j + 5);
                SetTileMaterial3D(i + 19, 0, j + 5, texture);
            }
        }

        // Area 6 (6x2) - Width x Height

        for (int i = 0; i < 6; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                CreateTileHere(i + 19, 0, j + 12);
                SetTileMaterial3D(i + 19, 0, j + 12, texture);
            }
        }

        // Area 7 (10x2) - Width x Height

        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                CreateTileHere(i + 12, 0, j + 14);
                SetTileMaterial3D(i + 12, 0, j + 14, texture);
            }
        }

        // Area 8 (2x4) - Width x Height

        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                CreateTileHere(i + 9, 0, j + 12);
                SetTileMaterial3D(i + 9, 0, j + 12, texture);
            }
        }

        // Area 9 (5x8) - Width x Height

        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                CreateTileHere(i + 4, 0, j + 10);
                SetTileMaterial3D(i + 4, 0, j + 10, texture);
            }
        }

        // Area 10 (2x4) - Width x Height

        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                CreateTileHere(i + 2, 0, j + 12);
                SetTileMaterial3D(i + 2, 0, j + 12, texture);
            }
        }

        // Connections

        CreateTileHere(6, 0, 3);
        SetTileMaterial3D(6, 0, 3, texture);

        CreateTileHere(18, 0, 6);
        SetTileMaterial3D(18, 0, 6, texture);
        CreateTileHere(18, 0, 7);
        SetTileMaterial3D(18, 0, 7, texture);

        CreateTileHere(21, 0, 4);
        SetTileMaterial3D(21, 0, 4, texture);

        CreateTileHere(20, 0, 9);
        SetTileMaterial3D(20, 0, 9, texture);
        CreateTileHere(20, 0, 10);
        SetTileMaterial3D(20, 0, 10, texture);
        CreateTileHere(20, 0, 11);
        SetTileMaterial3D(20, 0, 11, texture);

        CreateTileHere(24, 0, 9);
        SetTileMaterial3D(24, 0, 9, texture);
        CreateTileHere(24, 0, 10);
        SetTileMaterial3D(24, 0, 10, texture);
        CreateTileHere(24, 0, 11);
        SetTileMaterial3D(24, 0, 11, texture);

        CreateTileHere(14, 0, 13);
        SetTileMaterial3D(14, 0, 13, texture);

        CreateTileHere(11, 0, 14);
        SetTileMaterial3D(11, 0, 14, texture);

        CreateTileHere(11, 0, 9);
        SetTileMaterial3D(11, 0, 9, texture);
        CreateTileHere(11, 0, 10);
        SetTileMaterial3D(11, 0, 10, texture);

        CreateTileHere(10, 0, 9);
        SetTileMaterial3D(10, 0, 9, texture);
        CreateTileHere(10, 0, 10);
        SetTileMaterial3D(10, 0, 10, texture);

        CreateTileHere(9, 0, 9);
        SetTileMaterial3D(9, 0, 9, texture);
        CreateTileHere(9, 0, 10);
        SetTileMaterial3D(9, 0, 10, texture);
        CreateTileHere(9, 0, 11);
        SetTileMaterial3D(9, 0, 11, texture);

        CreateTileHere(9, 0, 16);
        SetTileMaterial3D(9, 0, 16, texture);

        CreateTileHere(7, 0, 18);
        SetTileMaterial3D(7, 0, 18, texture);
        CreateTileHere(6, 0, 18);
        SetTileMaterial3D(6, 0, 18, texture);
        CreateTileHere(5, 0, 18);
        SetTileMaterial3D(5, 0, 18, texture);

        CreateTileHere(3, 0, 16);
        SetTileMaterial3D(3, 0, 16, texture);

        CreateTileHere(1, 0, 13);
        SetTileMaterial3D(1, 0, 13, texture);
        CreateTileHere(1, 0, 14);
        SetTileMaterial3D(1, 0, 14, texture);

        CreateTileHere(3, 0, 11);
        SetTileMaterial3D(3, 0, 11, texture);

        // Place walls

        for (int i = 0; i < 17; i++) // Length of 17 (1 to 18), horizontal
        {
            CreateWallHere(i + 1, 1, 0, texture, 1);
        }

        for (int i = 0; i < 4; i++) // Vertical
        {
            CreateWallHere(1, 1, i + 1, texture, 1);
        }

        for (int i = 0; i < 6; i++)
        {
            CreateWallHere(i + 1, 1, 5, texture, 1);
        }

        CreateWallHere(6, 1, 1, texture, 1);
        CreateWallHere(6, 1, 2, texture, 1);
        CreateWallHere(6, 1, 4, texture, 1);

        for (int i = 0; i < 3; i++) // Vertical
        {
            CreateWallHere(6, 1, i + 6, texture, 1);
        }

        for (int i = 0; i < 6; i++)
        {
            CreateWallHere(i + 3, 1, 9, texture, 1);
        }

        CreateWallHere(2, 1, 10, texture, 1); // Warp left
        CreateWallHere(3, 1, 10, texture, 1);

        CreateWallHere(2, 1, 11, texture, 1);
        CreateWallHere(1, 1, 11, texture, 1);

        CreateWallHere(1, 1, 12, texture, 1);
        CreateWallHere(0, 1, 12, texture, 1);

        CreateWallHere(0, 1, 13, texture, 1); // Up
        CreateWallHere(0, 1, 14, texture, 1);

        CreateWallHere(0, 1, 15, texture, 1); // Wrap right
        CreateWallHere(1, 1, 15, texture, 1);

        CreateWallHere(1, 1, 16, texture, 1);
        CreateWallHere(2, 1, 16, texture, 1);

        CreateWallHere(2, 1, 17, texture, 1);
        CreateWallHere(3, 1, 17, texture, 1);

        CreateWallHere(3, 1, 18, texture, 1);
        CreateWallHere(4, 1, 18, texture, 1);

        for (int i = 0; i < 5; i++) // Topper
        {
            CreateWallHere(i + 4, 1, 19, texture, 1);
        }

        CreateWallHere(8, 1, 18, texture, 1); // Wrap down, right
        CreateWallHere(9, 1, 18, texture, 1);

        CreateWallHere(9, 1, 17, texture, 1);
        CreateWallHere(10, 1, 17, texture, 1);

        for (int i = 0; i < 12; i++) // Top wall (Area 7)
        {
            CreateWallHere(i + 10, 1, 16, texture, 1);
        }

        for (int i = 0; i < 3; i++) // Down
        {
            CreateWallHere(21, 1, i + 14, texture, 1);
        }

        for (int i = 0; i < 3; i++)
        {
            CreateWallHere(i + 22, 1, 14, texture, 1);
        }

        for (int i = 0; i < 6; i++) // Down
        {
            CreateWallHere(25, 1, i + 9, texture, 1);
        }

        CreateWallHere(26, 1, 9, texture, 1);
        CreateWallHere(28, 1, 9, texture, 1);
        CreateWallHere(29, 1, 9, texture, 1);
        CreateWallHere(29, 1, 8, texture, 1);
        CreateWallHere(29, 1, 5, texture, 1);
        CreateWallHere(29, 1, 4, texture, 1);

        for (int i = 0; i < 7; i++) // Bottom of area 5
        {
            CreateWallHere(i + 22, 1, 4, texture, 1);
        }

        for (int i = 0; i < 2; i++)
        {
            CreateWallHere(i + 19, 1, 4, texture, 1);
        }

        for (int i = 0; i < 6; i++) // Up
        {
            CreateWallHere(18, 1, i, texture, 1);
        }

        for (int i = 0; i < 6; i++) // Up
        {
            CreateWallHere(18, 1, i + 8, texture, 1);
        }

        for (int i = 0; i < 3; i++) // Brick
        {
            CreateWallHere(19, 1, i + 9, texture, 1);
        }
        for (int i = 0; i < 3; i++) // Brick
        {
            CreateWallHere(21, 1, i + 9, texture, 1);
        }
        for (int i = 0; i < 3; i++) // Brick
        {
            CreateWallHere(22, 1, i + 9, texture, 1);
        }
        for (int i = 0; i < 3; i++) // Brick
        {
            CreateWallHere(23, 1, i + 9, texture, 1);
        }

        for (int i = 0; i < 3; i++) // Top of area 3
        {
            CreateWallHere(i + 11, 1, 13, texture, 1);
        }
        for (int i = 0; i < 3; i++)
        {
            CreateWallHere(i + 15, 1, 13, texture, 1);
        }

        CreateWallHere(11, 1, 12, texture, 1);
        CreateWallHere(10, 1, 11, texture, 1);

        CreateWallHere(11, 1, 15, texture, 1);

        CreateWallHere(23, 1, 2, texture, 1);
        CreateWallHere(23, 1, 3, texture, 1);

        CreateWallHere(11, 1, 11, texture, 1);

        // Place Players

        PlacePlayerCharacter3D(21, 0, 7, playerParent, fighterLL, 270);
        PlacePlayerCharacter3D(20, 0, 8, playerParent, paladinLL, 270);
        PlacePlayerCharacter3D(19, 0, 6, playerParent, clericLL, 270);
        PlacePlayerCharacter3D(24, 0, 5, playerParent, sorcererLL, 270);

        // Place Enemies

        PlaceEnemyCharacter3D(5, 0, 3, enemyParent, goblin_Pref, 90);
        PlaceEnemyCharacter3D(9, 0, 3, enemyParent, goblin_Pref, 90);
        //PlaceEnemyCharacter3D(6, 0, 11, enemyParent, goblin_Pref, 90);
        PlaceEnemyCharacter3D(10, 0, 7, enemyParent, goblin_Pref, 90);
        PlaceEnemyCharacter3D(4, 0, 12, enemyParent, goblin_Pref, 90);
        PlaceEnemyCharacter3D(12, 0, 2, enemyParent, goblin_Pref, 90);
        PlaceEnemyCharacter3D(3, 0, 2, enemyParent, goblin_Pref, 90);
        PlaceEnemyCharacter3D(5, 0, 15, enemyParent, goblin_Pref, 90);

        PlaceEnemyCharacter3D(3, 0, 14, enemyParent, goblinHealer_Pref, 90);



    }

    void GM_Hermit()
    {

    }

    public void RelocateTile(int arrayLocX, int arrayLocY, int xBump, int yBump, int zBump) // Relocate a tile based on given inputs
    {
        if(gridArray[arrayLocX, arrayLocY] != null)
        {
            gridArray[arrayLocX, arrayLocY].transform.position = new Vector3(gridArray[arrayLocX, arrayLocY].transform.position.x + xBump, gridArray[arrayLocX, arrayLocY].transform.position.y + yBump, gridArray[arrayLocX, arrayLocY].transform.position.z + zBump);
        }
        else
        {
            Debug.LogError("ERROR: Tile does not exist at this location.");
        }
    }

    public void SetTileWeight(int arrayLocX, int arrayLocY, float newWeight) // Set the weight of a certain tile
    {
        if (gridArray[arrayLocX, arrayLocY] != null)
        {
            if (newWeight == 50)
            {
                gridArray[arrayLocX, arrayLocY].GetComponentInChildren<Tile>().walkable = false;
            }
            else
            {
                gridArray[arrayLocX, arrayLocY].GetComponentInChildren<Tile>().SetWeight(newWeight);
            }
        }
        else
        {
            Debug.LogError("ERROR: Tile does not exist at this location.");
        }
    }

    public void RemoveTile(int arrayLocX, int arrayLocY) // May or may not work
    {
        GameObject obj = gridArray[arrayLocX, arrayLocY];
        Destroy(obj);
    }

    public void RemoveTile3D(int arrayLocX, int arrayLocY, int arrayLocZ) // May or may not work
    {
        //GameObject obj = tileArray[arrayLocX, arrayLocY, arrayLocY];
        //tileArray[arrayLocX, arrayLocY, arrayLocY] = null;
        //Destroy(obj);
        Destroy(tileArray[arrayLocX, arrayLocY, arrayLocZ]);
    }

    // -------------------------------------------------------------------------------------------------------------------- //

    public void PlacePlayerCharacter(int xPos, int zPos, GameObject playerParent, int rotation = 0) // Place a player character above a tile
    {
        GameObject obj = Instantiate(playerPref, new Vector3(xPos, characterHalfHeight, zPos), Quaternion.identity);
        obj.transform.parent = playerParent.transform;

        obj.AddComponent<CameraHelper>();
    }

    public void PlaceEnemyCharacter(int xPos, int zPos, GameObject enemyParent, GameObject enemyType, int rotation = 0) // Place an enemy character above a tile
    {
        GameObject obj = Instantiate(enemyType, new Vector3(xPos, characterHalfHeight, zPos), Quaternion.identity);

        if(rotation != 0)
        {
            obj.transform.Rotate(0, rotation, 0); // Rotate if needed
        }

        obj.transform.parent = enemyParent.transform;

        obj.AddComponent<CameraHelper>();
    }

    public void PlacePlayerCharacter3D(int xPos, int yPos, int zPos, GameObject playerParent, GameObject prefToSpawn, int rotation = 0) // Place a player character above a tile
    {
        GameObject obj = Instantiate(prefToSpawn, new Vector3(xPos, yPos + characterHalfHeight, zPos), Quaternion.identity);

        if (rotation != 0)
        {
            obj.transform.Rotate(0, rotation, 0); // Rotate if needed
        }

        obj.transform.parent = playerParent.transform;

        obj.AddComponent<CameraHelper>();
    }

    public void PlaceEnemyCharacter3D(int xPos, int yPos, int zPos, GameObject enemyParent, GameObject enemyType, int rotation = 0) // Place an enemy character above a tile
    {
        GameObject obj = Instantiate(enemyType, new Vector3(xPos, yPos + characterHalfHeight, zPos), Quaternion.identity);

        if (rotation != 0)
        {
            obj.transform.Rotate(0, rotation, 0); // Rotate if needed
        }

        obj.transform.parent = enemyParent.transform;

        obj.AddComponent<CameraHelper>();
    }

    // ---------------------------------------------------------------------------------------------------------------------------- //

    public void SetTileMaterial(int arrayLocX, int arrayLocY, string desiredMat) // Set a certain tile to have a desired texture
    {
        Material newMaterial = Resources.Load<Material>("Materials/Minecraft/Materials/" + desiredMat);

        if(newMaterial == null)
        {
            Debug.LogError("Failed to set tile material! - " + desiredMat);
        }
        else
        {
            gridArray[arrayLocX, arrayLocY].GetComponentInChildren<GenericCube>().SetMaterial(newMaterial);
        }
    }

    public void CreateTileHere(int arrayLocX, int arrayLocY, int arrayLocZ)
    {
        GameObject obj = Instantiate(tilePref, new Vector3(bottomLeftLocation.x + arrayLocX, bottomLeftLocation.y + arrayLocY, bottomLeftLocation.z + arrayLocZ), Quaternion.identity);
        obj.transform.parent = gridParent.transform;
        tileArray[(int)bottomLeftLocation.x + arrayLocX, (int)bottomLeftLocation.y + arrayLocY, (int)bottomLeftLocation.z + arrayLocZ] = obj;
    }

    public void SetTileMaterial3D(int arrayLocX, int arrayLocY, int arrayLocZ, string desiredMat) // Set a certain tile to have a desired texture
    {
        Material newMaterial = Resources.Load<Material>("Materials/Minecraft/Materials/" + desiredMat);

        if (newMaterial == null)
        {
            Debug.LogError("Failed to set tile material! - " + desiredMat);
        }
        else
        {
            tileArray[arrayLocX, arrayLocY, arrayLocZ].GetComponentInChildren<GenericCube>().SetMaterial(newMaterial);
        }
    }

    public void CreateWorldCubeHere(int arrayLocX, int arrayLocY, int arrayLocZ, string desiredMat, int special = 0)
    {
        if(special == 1)
        {
            GameObject obj = Instantiate(worldCubePref, new Vector3(bottomLeftLocation.x + arrayLocX, bottomLeftLocation.y + arrayLocY, bottomLeftLocation.z + arrayLocZ), Quaternion.identity);
            obj.transform.parent = gridParent.transform;
            worldCubeArray[(int)bottomLeftLocation.x + arrayLocX, (int)bottomLeftLocation.y + arrayLocY, (int)bottomLeftLocation.z + arrayLocZ] = obj;

            int random = Random.Range(0, 10);

            if (random <= 7)
            {
                desiredMat = "stone_bricks";
            }
            else if (random == 8)
            {
                desiredMat = "cracked_stone_bricks";
            }
            else if (random == 9)
            {
                desiredMat = "mossy_stone_bricks";
            }
            else
            {
                desiredMat = "cobblestone";
            }

            Material newMaterial = Resources.Load<Material>("Materials/Minecraft/Materials/" + desiredMat);

            if (newMaterial == null)
            {
                Debug.LogError("Failed to set tile material! - " + desiredMat);
            }
            else
            {
                worldCubeArray[arrayLocX, arrayLocY, arrayLocZ].GetComponentInChildren<GenericCube>().SetMaterial(newMaterial);
            }
        }
        else
        {
            GameObject obj = Instantiate(worldCubePref, new Vector3(bottomLeftLocation.x + arrayLocX, bottomLeftLocation.y + arrayLocY, bottomLeftLocation.z + arrayLocZ), Quaternion.identity);
            obj.transform.parent = gridParent.transform;
            worldCubeArray[(int)bottomLeftLocation.x + arrayLocX, (int)bottomLeftLocation.y + arrayLocY, (int)bottomLeftLocation.z + arrayLocZ] = obj;

            Material newMaterial = Resources.Load<Material>("Materials/Minecraft/Materials/" + desiredMat);

            if (newMaterial == null)
            {
                Debug.LogError("Failed to set tile material! - " + desiredMat);
            }
            else
            {
                worldCubeArray[arrayLocX, arrayLocY, arrayLocZ].GetComponentInChildren<GenericCube>().SetMaterial(newMaterial);
            }
        }
    }

    public void CreateObjectHere(int arrayLocX, int arrayLocY, int arrayLocZ, GameObject toSpawn, float transparency = 0.4f)
    {

        GameObject obj = Instantiate(toSpawn, new Vector3(bottomLeftLocation.x + arrayLocX, bottomLeftLocation.y + arrayLocY, bottomLeftLocation.z + arrayLocZ), Quaternion.identity);
        obj.transform.parent = objParent.transform;

        // Make the object transparent
        //var col = obj.GetComponent<MeshRenderer>().material.color;
        //obj.GetComponent<MeshRenderer>().material.color = new Color(1f, 1f, 1f, 0.4f);
        //col.a = transparency;

        // Resize it
        obj.transform.localScale = new Vector3(5, 5, 5);

        // Make it ignore raycasts
        int LayerIgnoreRaycast = LayerMask.NameToLayer("Ignore Raycast");
        obj.layer = LayerIgnoreRaycast;
    }

    public void CreateWallHere(int arrayLocX, int arrayLocY, int arrayLocZ, string desiredMat, int special = 0)
    {
        CreateWorldCubeHere(arrayLocX, arrayLocY, arrayLocZ, desiredMat, special);
        CreateWorldCubeHere(arrayLocX, arrayLocY + 1, arrayLocZ, desiredMat, special);
    }
}
