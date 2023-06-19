using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Mkey
{
    public class JackPotWin : MonoBehaviour
    {
        [SerializeField]
        private AudioClip coinsClip;

        #region temp vars
        private LampsController[] lamps;
        private CoinProcAnim[] coinsFountains;
        private JackPot jp;
        private SoundMaster MSound { get { return SoundMaster.Instance; } }
        private ColorFlasher f;
        #endregion temp vars

        #region regular
        private void Start()
        {
            jp = GetComponentInParent<JackPot>();
            if (!jp) return;

            if (jp.WinRenderers!=null)
            {
                foreach (var item in jp.WinRenderers)
                {
                    if (item) item.enabled = true;
                } 
            }


            lamps = jp.Lamps;
            coinsFountains = jp.CoinsFoutains;

            if (lamps != null)
            {
                foreach (var item in lamps)
                {
                    if (item) item.lampFlash = LampsFlash.Sequence;
                }
            }

            StartCoroutine(FountainC());

            f = new ColorFlasher(gameObject, new TextMesh[] {jp.titleTextMesh, jp.amountTextMesh }, new Text[] {jp.titleText, jp.amountText }, new SpriteRenderer[] { }, new Image[] { }, 1f);
            f.FlashingAlpha();
        }

        private void OnDestroy()
        {
            if (jp && jp.WinRenderers!=null)
            {
                foreach (var item in jp.WinRenderers)
                {
                    if (item) item.enabled = false;
                }
            }

            StopCoroutine(FountainC());

            if (lamps != null)
            {
                foreach (var item in lamps)
                {
                    if (item) item.lampFlash = LampsFlash.NoneDisabled;
                }
            }
            if (f!=null) f.Cancel();
        }
        #endregion regular

        private IEnumerator FountainC()
        {
            if (coinsFountains != null)
            {

                while (true)
                {
                    foreach (var item in coinsFountains)
                    {
                        if (item) item.Jump();
                        if (coinsClip) MSound.PlayClip(0.2f, coinsClip);
                        yield return new WaitForSeconds(0.5f);
                    }
                    yield return new WaitForSeconds(2);
                }
            }
        }
    }
}
