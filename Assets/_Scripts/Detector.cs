using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Detector : MonoBehaviour
{
    public string BlockName;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        BlockName = collision.tag;
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        BlockName = collision.tag;
    }
}
