using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BonusRoundChecking : MonoBehaviour
{
    public Detector[] Ds;
    public GameObject  MainPanel,Bonusimage;
    public GameObject[] BonusPanels;
    int streak;
    public static int bonusonetime;
    private void OnEnable()
    {
        streak = 0;
        Invoke("checkresult", 0.5f);
    }
    void checkresult()
    {
        for (int j = 0 ; j < 15; j++)
        {
            if (Ds[j].BlockName == "bonus")
            {
                streak++;
            }
        }
        if (streak > 2&&bonusonetime==0)
        {
            Debug.Log("Bonussssssss");
            Bonusimage.SetActive(false);
            Bonusimage.SetActive(true);
            SlotGameManager.freespins = 0;
            Invoke("BonusActive", 4);
            bonusonetime++;
        }
    }
    void BonusActive()
    {       
        BonusPanels[Random.Range(0, 3)].SetActive(true);
        MainPanel.SetActive(false);
    }
}
