using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiceRoll : MonoBehaviour
{
    /// <summary>
    /// Call this function to roll a specific type die anywhere.
    /// </summary>
    /// <returns> Returns the result of the die roll. </returns>

    public static DiceRoll inst;
    public void Awake()
    {
        inst = this;
    }

    public int RollD20(){
        int lowerBound = 1;
        int upperBound = 20;

        int finalRoll = Random.Range(lowerBound, upperBound);

        return finalRoll;
    }
    public int RollD10(){
        int lowerBound = 1;
        int upperBound = 10;

        int finalRoll = Random.Range(lowerBound, upperBound);

        return finalRoll;
    }
    public int RollD6(){
        int lowerBound = 1;
        int upperBound = 6;

        int finalRoll = Random.Range(lowerBound, upperBound);

        return finalRoll;
    }
    public int RollD100(){
        int lowerBound = 1;
        int upperBound = 100;

        int finalRoll = Random.Range(lowerBound, upperBound);

        return finalRoll;
    }
    public int RollD12(){
        int lowerBound = 1;
        int upperBound = 12;

        int finalRoll = Random.Range(lowerBound, upperBound);

        return finalRoll;
    }
    public int RollD8(){
        int lowerBound = 1;
        int upperBound = 8;

        int finalRoll = Random.Range(lowerBound, upperBound);

        return finalRoll;
    }
    public int RollD4(){
        int lowerBound = 1;
        int upperBound = 4;

        int finalRoll = Random.Range(lowerBound, upperBound);

        return finalRoll;
    }

    public int RollDX(int newUpper) // Custom roll
    {
        int lowerBound = 1;
        int upperBound = newUpper;

        int finalRoll = Random.Range(lowerBound, newUpper);

        return finalRoll;
    }
}
