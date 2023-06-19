using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class Bonus2Manager : MonoBehaviour
{
    public GameObject[] BoxOpen, BoxClose, BoxGold;
    public Button[] buttons;
    public Text infotext;
    public GameObject wintext,BonusPanel,MainPanel;
    int r;
    int count;
  
    private void OnEnable()
    {
        count = 0;
        StartCoroutine(Playboxes());
    }
    IEnumerator Playboxes()
    {
        yield return new WaitForSeconds(0.8f);
        r = Random.Range(0, 10);
        for (int j = 0; j < 10; j++)
        {
            BoxClose[j].SetActive(false);
            if (r != j)
            {
                BoxOpen[j].SetActive(true);
            }
            else
            {
                BoxGold[j].SetActive(true);
            }
        }
        yield return new WaitForSeconds(0.5f);
        for (int k = 0; k < 10; k++)
        {
            BoxClose[k].SetActive(true);
            BoxOpen[k].SetActive(false);
            BoxGold[k].SetActive(false);
        }
        count++;
        if (count < 5)
        {
            StartCoroutine(Playboxes());
        }
        if (count == 5)
        {
            buttonsactive();
        }
    }
    void buttonsactive()
    {
        for (int k = 0; k < 10; k++)
        {
            buttons[k].interactable = true;
        }
    }
    void buttonsInActive()
    {
        for (int k = 0; k < 10; k++)
        {
            buttons[k].interactable = false;
        }
    }
    public void guess(int n)
    {
        if (n == r)
        {
            infotext.text = "Well Done!";
            BoxGold[r].SetActive(true);
            BoxClose[r].SetActive(false);
            wintext.SetActive(true);
            SlotGameManager.amount += 500;
            Debug.Log("winnnnnnnnn");
            buttonsInActive();
            Invoke("Bonusfalse", 3f);
        }
        else
        {
            infotext.text = "Wrong Box";          
            BoxOpen[n].SetActive(true);
            BoxGold[r].SetActive(true);
            BoxClose[n].SetActive(false);
            BoxClose[r].SetActive(false);
            Debug.Log("loseeee");
            buttonsInActive();
            Invoke("Bonusfalse", 3f);
        }
    }
    void Bonusfalse()
    {
        SceneManager.LoadScene(0);
        infotext.text = "Tap the Box with the Gold In";
        BoxGold[r].SetActive(false);
        wintext.SetActive(false);
      //  MainPanel.SetActive(true);
       // BonusPanel.SetActive(false);
    }
}
