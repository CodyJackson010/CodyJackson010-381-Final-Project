using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MenuScript
{
    /*
    [MenuItem("Tools/Assign Tile Material")]
    public static void AssignTileMaterial()
    {
        GameObject[] tiles = GameObject.FindGameObjectsWithTag("Tile"); // Name of Tag
        Material material = Resources.Load<Material>("Tile"); // Name of Material (MUST MATCH)

        foreach(GameObject t in tiles)
        {
            t.GetComponent<Renderer>().material = material;
        }
    }
    
    [MenuItem("Tools/Assign Tile Script")]
    public static void AssignTileScript()
    {
        GameObject[] tiles = GameObject.FindGameObjectsWithTag("Tile"); // Name of Tag

        foreach (GameObject t in tiles)
        {
            if(t.GetComponent<Tile>() != null) // Only add if it doesn't already have it
            {
                t.AddComponent<Tile>();
            }
        }
    }
    */
    /*
    [MenuItem("Tools/Remove Tile Script")]
    public static void RemoveTileScript()
    {
        GameObject[] tiles = GameObject.FindGameObjectsWithTag("Tile"); // Name of Tag

        foreach (GameObject t in tiles)
        {
            UnityEngine.Object.Destroy(t.GetComponent<Tile>());
        }
    }
    */
}
