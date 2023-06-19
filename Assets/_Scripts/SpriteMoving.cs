using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteMoving : MonoBehaviour
{
    int RandomValue;
    public int Min,Max;
    public float gap,smallgap;
    Vector2 Pos;
    public SlotGameManager GM;
    public bool istopped;
    public float lastvalue;
    public float ylastpos;
    int onetime = 0;
    void Start()
    {
       
    }
   
    public void Rotating()
    {
        //  GM.coinseffect.SetActive(false);
        if (SlotGameManager.amount >= SlotGameManager.betamount)
        {
            GM.Lines.SetActive(false);
            StartCoroutine(Rotate());
            istopped = false;
            transform.position = new Vector3(transform.position.x, lastvalue, 0);
        }
    }
    IEnumerator Rotate()
    {
        int r = Random.Range(Min, Max);
        for (int i = 0; i <r; i++)
        {
            if (transform.position.y < ylastpos)
            { transform.position = new Vector3(transform.position.x, lastvalue, 0); }

            transform.position = new Vector3(transform.position.x, transform.position.y - gap,0);

            if (i == r - 2)
            {
                for (int j = 0; j < 40; j++)
                {
                    if (transform.position.y < ylastpos)
                    { transform.position = new Vector3(transform.position.x, lastvalue, 0); }

                    transform.position = new Vector3(transform.position.x, transform.position.y - smallgap, 0);
                    yield return new WaitForSeconds(0.01f);
                }
                break;
            }
            yield return new WaitForSeconds(0f);
        }
        onetime = 0;
        istopped = true;
    }    
}
