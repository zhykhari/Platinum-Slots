using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SlotGameManager : MonoBehaviour
{
    public GameObject MoreCoinseffect, MoreCoinseffect2,SlotUnlockPanel, Lines, Bigwin, Autospinpanel, ShopPanel,PayoutPanel;
    public Text amountext, bettext,winamounttext,winamounttext2,leveltext;
    int levelno;
    public static float winamount, amount, betamount;
    public static int win = 0;
    public Image LevelBar;
    public SpriteMoving[] sp;
    public int MinBet, thisSceneNo, BetLimit;
    public AudioSource bttnsound, Spinsound,winsound,slotunlock,coinssound,levelupsound,BigWinSound;
    public Button[] betbttns;
    float levelbarvalue;
    public int noofspin = 0;
    public int nextlevelunlockvalue;
    public static int freespins = 0;
    public static int autorotate=0;
    public GameObject[] alllines,Unlocked;
    int previouslineNo;
    public static int adcounter = 0;
    //public AdsManager Ads;
    public GameObject RewardAdButton;
    bool autoSpinstate = false;
    #region startF
    void Start()
    {
        previouslineNo = 0;
        autorotate = 0;
        
        levelno = PlayerPrefs.GetInt("Level", 1);
        levelbarvalue = PlayerPrefs.GetFloat("LevelBar" + levelno, 0f);

        LevelBar.fillAmount = levelbarvalue;
        betamount = MinBet;
        amount = PlayerPrefs.GetInt("NewAllGold", 10000);
        leveltext.text = "" + levelno;


    }
    private void OnEnable()
    {
        freespins = 0;
        betbttns[0].interactable = true;
        betbttns[1].interactable = true;
        betbttns[2].interactable = true;
        betbttns[3].interactable = true;
    }
    #endregion
    void Update()
    {
        if (amount < 100)
        {
            RewardAdButton.SetActive(true);
        }
        else
        {
            RewardAdButton.SetActive(false);
        }            
        PlayerPrefs.SetInt("NewAllGold", (int)amount);
        bettext.text = "" + betamount;
        amountext.text = "" + amount;
       
        if (amount < 0) { amount = 0; }
        if (sp[0].istopped&& sp[1].istopped && sp[2].istopped && sp[3].istopped && sp[4].istopped)
        {
            Spinsound.Stop();
            sp[0].istopped = false;
            Invoke("Active", 0.2f);
            LevelBar.fillAmount += 0.03f;
            PlayerPrefs.SetFloat("LevelBar" + levelno, LevelBar.fillAmount);
            sp[0].istopped = false;
            if (LevelBar.fillAmount > 0.96f)
            {
                Invoke("unlocknewlevel", 2f);
                            
            }
        }
        if (win == 1)
        {
            winsound.Play();
            Invoke("winningamount", 1);
            Invoke("CoinsSpwan",1);
            Invoke("CancelInvokes", 3);
            Invoke("MoreCoins", 0.5f);
            win = 0;
        }
    }
    void winningamount()
    {
        Debug.Log(winamount);
        if (winamount > 5f)
        {
            Bigwin.SetActive(true);
            BigWinSound.Play();
        }
        amount += (betamount * (winamount)) + betamount;
    }
    public void Spin()
    {
        if (amount >= betamount)
        {
            adcounter++;
            if (autorotate == 1)
            {
                freespins--;
                for (int i = 0; i < 5; i++)
                {
                    sp[i].Rotating();
                }
            }
            noofspin++;
            Spinsound.Play();
            bttnsound.Play();
            CancelInvoke();
            winamounttext.text = "";
            betbttns[0].interactable = false;
            betbttns[1].interactable = false;
            betbttns[2].interactable = false;
            betbttns[3].interactable = false;
            amount -= betamount;
            PlayerPrefs.SetFloat("NewAllGold", amount);
            alllines[previouslineNo].SetActive(false);
            BonusRoundChecking.bonusonetime = 0;
            Autospinpanel.SetActive(false);
        }
    }
    public void autospin_panel()
    {
        autoSpinstate = !autoSpinstate;
        Autospinpanel.SetActive(autoSpinstate);
    }
    public void AutoSpin(int spins)
    {
        if (amount >= betamount)
        {
            autorotate = 1;
            freespins = spins;
            Spin();
            Autospinpanel.SetActive(false);
        }
    }
    public void Add()
    {
        bttnsound.Play();

        if (amount > betamount)
        {
            betamount += 100;
            if (betamount > amount) { betamount = 100; }
            if (betamount > 500) { betamount = 100; }
            return;
        }
        if (betamount > amount-1) { betamount = 100; }        

    }
    public void MaxBet()
    {
        bttnsound.Play();

        if (amount > BetLimit)
        {
            betamount = BetLimit;
        }
    }
   void unlocknewlevel()
    {
        LevelBar.fillAmount = 0f;
        levelno++;
        PlayerPrefs.SetInt("Level", levelno);
        leveltext.text = "" + levelno;
        if (levelno < 5)
        {
            Unlocked[levelno-1].SetActive(true);
            SlotUnlockPanel.SetActive(true);
            levelupsound.Play();
        }
        StartCoroutine("unlockpanelfalse");
    }
    void Active()
    {
        winamount = 0;
        winamounttext2.text =0+"";
        Lines.SetActive(true);
        Invoke("ActiveButtons", 0.2f);
    }
    IEnumerator unlockpanelfalse()
    {
        yield return new WaitForSeconds(4f);
        Unlocked[levelno-1].SetActive(true);
        SlotUnlockPanel.SetActive(false);
    }
    void ActiveButtons()
    {
        if (winamount < 1&&freespins<1)
        {
            betbttns[0].interactable = true;
            betbttns[1].interactable = true;
            betbttns[2].interactable = true;
            betbttns[3].interactable = true;
        }
        
        StartCoroutine("unlockpanelfalse");
        if (freespins > 0)
        {
            Invoke("Spin",3);           
            autorotate = 1;
        }
        else { autorotate = 0; }
    }
    public void CoinsSpwan()
    {
        coinssound.Play(); 
        winamounttext.GetComponent<Animator>().enabled = true;
        float amountwon = (betamount *(winamount))+betamount;
        if (amountwon > 0)
        {
            winamounttext.text = amountwon.ToString("0") + "\n WIN";
            winamounttext2.text = amountwon.ToString("0") +"";
        }
        Invoke("CoinsSpwan", 0.1f);      
    }
    public void CancelInvokes()
    {
        winamounttext.GetComponent<Animator>().enabled = false;
        winamounttext.text = "";
        Bigwin.SetActive(false);
        if (adcounter > 10)
        {
            //Ads.ShowInterstitial();
        }
            CancelInvoke();
      //  coinseffect.SetActive(false);
        if (freespins < 1)
        {
            betbttns[0].interactable = true;
            betbttns[1].interactable = true;
            betbttns[2].interactable = true;
            betbttns[3].interactable = true;
        }
    }
    void MoreCoins()
    {
        MoreCoinseffect.SetActive(false);
        MoreCoinseffect.SetActive(true);
        Invoke("MoreCoins2", 1f);       
    }
    void MoreCoins2()
    {
        MoreCoinseffect2.SetActive(false);
        MoreCoinseffect2.SetActive(true);
    }
    public void Patternlines(int lineno)
    {
        alllines[previouslineNo].SetActive(false);
        alllines[lineno].SetActive(true);
        previouslineNo = lineno;
        bttnsound.Play();
    }
    public void shopOnOff(bool value)
    {
        bttnsound.Play();
        ShopPanel.SetActive(value);
    }
    public void PayoutOnOff(bool value)
    {
        bttnsound.Play();
        PayoutPanel.SetActive(value);
    }
    
  
}
