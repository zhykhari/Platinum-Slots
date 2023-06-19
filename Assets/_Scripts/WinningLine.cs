using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class WinningLine : MonoBehaviour
{
    public Detector[] Ds;
    public SpriteRenderer sp;
    public bool win;
    public GameObject Effect;
    public int streak;
    Animator lineanim;
    public string animationname;
    string Priority, IconName;
    float points;
    private void Start()
    {
        streak = 1;
        lineanim = this.GetComponent<Animator>();
    }
    private void OnEnable()
    {
        win = false;
        streak = 1;
        sp.enabled = false;
        Invoke("checkresult", 0.5f);
       // Debug.Log("checking......");
    }
  
    void checkresult()
    {
        for (int i = 0; i < 1; i++)
        {
            for (int j = i + 1; j < 5; j++)
            {         
                if (Ds[i].BlockName == Ds[j].BlockName|| Ds[j].BlockName == "wild")
                {
                    streak++;
                    IconName = Ds[i].BlockName;
                    if (IconName == "1" || IconName == "2" || IconName == "3")                       
                    {
                        Priority = "Low";
                    }
                    else if (IconName == "4" || IconName == "5" || IconName == "6")
                    {
                        Priority = "Medium";
                    }
                    else if (IconName == "7" || IconName == "8")
                    {
                        Priority = "High";
                    }                
                }
                else
                {
                    break;
                }
            }
        }
        if (streak > 2)
        {
            sp.enabled = true;
           // Effect.SetActive(false);
           // Effect.SetActive(true);
            Debug.Log("win");
            SlotGameManager.win = 1;
            Invoke("newanimation", 3f);
            PointsCalculate();
        }
    }
  void PointsCalculate()
    {
        if (Priority == "Low")
        {
            points = 0.1f * streak;
            SlotGameManager.winamount += points;
            Debug.Log("Low " + points);
        }
        if (Priority == "Medium")
        {
            points = 0.3f * streak;
            SlotGameManager.winamount += points;
            Debug.Log("Medium " + SlotGameManager.winamount);
        }
        if (Priority == "High")
        {
            points = 0.5f * streak;
            SlotGameManager.winamount += points;
            Debug.Log("High " + SlotGameManager.winamount);
        }
    }
    void newanimation()
    {
        lineanim.SetBool(animationname, true);
      //  Invoke("hidesp", 5f);
    }
    void hidesp()
    {
        sp.enabled = false;
    }
}
