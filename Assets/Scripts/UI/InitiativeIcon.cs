using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InitiativeIcon : MonoBehaviour
{
    public Image mainBacking;
    public Image pfp;

    public Image healthMain;
    public Image healthBar;

    public Image isActiveArrow;
    public bool isActive = false;

    public TMP_Text idDisplay;
    public TMP_Text nameDisplay;

    public bool isPlayer;
    public int currentHealth;
    public int maxHP;
    public string newHPText;
    public string tooltip;
    public Character me;

    public Color playerColor = new Color(0.4f, 0.6f, 1f);
    public Color enemyColor = new Color(0.6f, 0f, 0f);

    void Start()
    {
        if (isPlayer)
        {
            // Display HP if player
            currentHealth = me.hitPoints;
            maxHP = me.maxHealth;
            healthBar.fillAmount = currentHealth / maxHP;
            newHPText = currentHealth.ToString() + "/" + maxHP.ToString();

            mainBacking.color = playerColor;
        }
        else
        {
            mainBacking.color = enemyColor;
        }

        if (newHPText != null)
        {
            tooltip = me.characterName + " (" + newHPText + ")";
        }
        else
        {
            tooltip = me.characterName;
        }

        idDisplay.SetText(me.initiative.ToString());
        nameDisplay.SetText(me.characterName.ToString());
        pfp.sprite = me.characterPfp;
        this.GetComponent<TooltipGo>().tipToShow = tooltip;
    }

    void Update()
    {
        if (isPlayer)
        {
            healthMain.enabled = true;
            healthBar.enabled = true;

            // Display HP if player
            currentHealth = me.hitPoints;
            maxHP = me.maxHealth;
            healthBar.fillAmount = (float)currentHealth / (float)maxHP;
            newHPText = currentHealth.ToString() + "/" + maxHP.ToString();

            tooltip = me.characterName + " (" + newHPText + ")";
            this.GetComponent<TooltipGo>().tipToShow = tooltip;
        }
        else
        {
            healthMain.enabled = false;
            healthBar.enabled = false;
        }

        if (isActive)
        {
            isActiveArrow.enabled = true;
            nameDisplay.enabled = true;
        }
        else
        {
            isActiveArrow.enabled = false;
            nameDisplay.enabled = false;
        }

    }

    public void Destroy()
    {
        Destroy(this.gameObject);
    }

}
