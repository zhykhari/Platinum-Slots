using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    12.08.2020 - first
 */
namespace Mkey
{
    public class GameObjectToggle : MonoBehaviour
    {
        public void Toggle()
        {
          if(gameObject) gameObject.SetActive(!isActiveAndEnabled);
        }
    }
}