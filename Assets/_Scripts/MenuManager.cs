using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public GameObject[] levellocked;
    public Button[] bttns;
    int levelno;
    float levelbarvalue;
    public Image LevelBar;
    public Text leveltext, amountext;
    float amount;
    public GameObject HomePanel,ShopPanel;
    public AudioSource bttnsound;

    void Start()
    {
        amount = PlayerPrefs.GetInt("NewAllGold", 10000);
        levelno = PlayerPrefs.GetInt("Level", 1);
        levelbarvalue = PlayerPrefs.GetFloat("LevelBar" + levelno, 0.0f);
        leveltext.text = "" + levelno;
        LevelBar.fillAmount = levelbarvalue;
        if (levelno < 5)
        {
            for(int i = 1; i < levelno; i++)
            {
                bttns[i].interactable = true;
                levellocked[i].SetActive(false);
            }
        }
        else
        {
            for (int i = 1; i < 4; i++)
            {
                bttns[i].interactable = true;
                levellocked[i].SetActive(false);
            }
        }
    }
    private void Update()
    {
        amount = PlayerPrefs.GetInt("NewAllGold", 10000);
        amountext.text = "" + amount;
    }
    public void shopOnOff(bool value)
    {
        amount = PlayerPrefs.GetInt("NewAllGold", 10000);
        bttnsound.Play();
        ShopPanel.SetActive(value);
    }
    public void levelsactive()
    {
        HomePanel.SetActive(false);
    }
}
