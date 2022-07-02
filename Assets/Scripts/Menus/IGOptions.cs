using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IGOptions : MonoBehaviour
{
    /// <summary>
    /// In Game Options Menu
    /// </summary>

    public static bool gameIsPaused = false;
    public static bool quitMenu = false;
    public static bool restartMenu = false;
    public static bool optionsMenu = false;

    public static bool victoryMenu = false;
    public static bool defeatMenu = false;

    public bool otherUsed = false;

    public GameObject pauseMenuUI;
    public GameObject quitMenuUI;
    public GameObject optionsMenuUI;
    public GameObject restartMenuUI;

    public GameObject victoryMenuUI;
    public GameObject defeatMenuUI;

    // References to things that use the escape button
    public GameObject measurementTool;

    public GameObject fadeRef;

    public static IGOptions inst;
    public void Awake()
    {
        inst = this;
    }

    // Update is called once per frame
    void Update()
    {
        otherUsed = CheckOtherLevels();

        if (Input.GetKeyDown(KeyCode.Escape) && UIManager.inst.escapeLevel == 0)
        {
            if (optionsMenu)
            {
                LoadOptionsMenu();
            }
            else if(restartMenu)
            {
                HandleRestart();
            }
            else if (quitMenu)
            {
                QuitFunc();
            }
            else
            {
                if (gameIsPaused)
                {
                    Resume();
                }
                else
                {
                    Pause();
                }
            }
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        quitMenuUI.SetActive(false);
        Time.timeScale = 1f;
        gameIsPaused = false;
    }

    void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f; // Freeze the game
        gameIsPaused = true;
    }

    public void LoadOptionsMenu()
    {
        if (optionsMenu)
        {
            optionsMenuUI.SetActive(false);
            pauseMenuUI.SetActive(true);
            optionsMenu = false;
        }
        else
        {
            optionsMenuUI.SetActive(true);
            pauseMenuUI.SetActive(false);
            optionsMenu = true;
        }
    }

    public void HandleRestart()
    {
        if (restartMenu)
        {
            restartMenuUI.SetActive(false);
            pauseMenuUI.SetActive(true);
            restartMenu = false;
        }
        else
        {
            restartMenuUI.SetActive(true);
            pauseMenuUI.SetActive(false);
            restartMenu = true;
        }
    }

    public void QuitFunc()
    {
        if (quitMenu)
        {
            quitMenuUI.SetActive(false);
            quitMenu = false;
        }
        else
        {
            quitMenuUI.SetActive(true);
            quitMenu = true;
        }
    }

    public void QuitToMainMenu()
    {
        //fadeRef.GetComponent<LevelFade>().FadeOut();

        quitMenuUI.SetActive(false);
        quitMenu = false;
        optionsMenuUI.SetActive(false);
        optionsMenu = false;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1); // Goes to Main Menu
    }

    public void QuitToDesktop()
    {
        Application.Quit();
    }

    public bool CheckOtherLevels() // Check if any other thing is using escape first
    {
        if (measurementTool.GetComponent<MeasurementTool>().showLine)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void DisplayVictory()
    {
        SoundManager.inst.StopAmbient();
        SoundManager.inst.PlayGlobalSound(20);

        victoryMenuUI.SetActive(true);
        quitMenuUI.SetActive(true);
    }

    public void DisplayDefeat()
    {
        SoundManager.inst.StopAmbient();
        SoundManager.inst.PlayGlobalSound(12);

        defeatMenuUI.SetActive(true);
        quitMenuUI.SetActive(true);
    }
}
