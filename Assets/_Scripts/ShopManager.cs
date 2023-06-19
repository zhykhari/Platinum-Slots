using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ShopManager : MonoBehaviour
{
    //public AudioSource Selectsound;
    int coins;
  
    private void OnEnable()
    {
        coins = PlayerPrefs.GetInt("NewAllGold", 10000);
    }
 
    public void lastsceneload()
    {
        SceneManager.LoadScene(PlayerPrefs.GetInt("LastScene",0));
    }
    public void Coinpack_1()
    {
        coins += 1000;
        SlotGameManager.amount += 1000;
        PlayerPrefs.SetInt("NewAllGold", coins);
       // Selectsound.Play();
    }
    public void Coinpack_2()
    {
      //  Selectsound.Play();
        SlotGameManager.amount += 3200;
        coins += 3200;
        PlayerPrefs.SetInt("NewAllGold", coins);
    }
    public void Coinpack_3()
    {
     //   Selectsound.Play();
        coins += 8000;
        SlotGameManager.amount += 8000;
        PlayerPrefs.SetInt("NewAllGold", coins);
    }
    public void Coinpack_4()
    {
    //    Selectsound.Play();
        coins += 20000;
        SlotGameManager.amount += 20000;
        PlayerPrefs.SetInt("NewAllGold", coins);
    }
    public void Coinpack_5()
    {
     //   Selectsound.Play();
        coins += 80000;
        SlotGameManager.amount += 80000;
        PlayerPrefs.SetInt("NewAllGold", coins);
    }
    public void Coinpack_6()
    {
      //  Selectsound.Play();
        coins += 200000;
        SlotGameManager.amount += 200000;
        PlayerPrefs.SetInt("NewAllGold", coins);
    }
}
