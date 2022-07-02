using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelFade : MonoBehaviour
{
    public Animator animator;
    public GameObject loadingRef;

    public void FadeOut()
    {
        animator.SetTrigger("FadeOut");
    }

    public void XOnFadeComplete()
    {
        loadingRef.SetActive(true);

        int pick = SceneManager.GetActiveScene().buildIndex;

        if(pick == 0)
        {
            SceneManager.LoadScene(1);
        }
        else
        {
            SceneManager.LoadScene(0);
        }
    }
}
