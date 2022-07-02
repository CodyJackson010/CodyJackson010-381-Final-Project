using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    int sceneToPick;
    public GameObject fadeRef;
    public GameObject backingRef;

    public void PlayGame()
    {
        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        fadeRef.GetComponent<LevelFade>().FadeOut();
    }

    public void QuitGame()
    {
        Debug.Log("Quit");
        Application.Quit();
    }

    public void EncounterA()
    {
        PlayerPrefs.SetInt("encounter", 1);
    }

    public void EncounterB()
    {
        PlayerPrefs.SetInt("encounter", 2);
    }

    public void EncounterC()
    {
        PlayerPrefs.SetInt("encounter", 3);
    }

    public void EncounterD()
    {
        PlayerPrefs.SetInt("encounter", 4);
    }

}
