using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Mkey
{
	public class TimeGiftGUIController : MonoBehaviour
	{
        [SerializeField]
        private Button  giftButton;
        [SerializeField]
        private GameObject timerGroup;
        [SerializeField]
        private Text timerText;
        [SerializeField]
        private  GUIFlyer guiFlyerPrefab;
        #region temp vars
        private TimeGiftController TGC { get { return TimeGiftController.Instance; } }
        #endregion temp vars

        #region regular
        private IEnumerator Start()
        {
            yield return new WaitWhile(() => !TGC);
            yield return new WaitForEndOfFrame();

            TGC.TickRestDaysHourMinSecEvent += RefreshTimerText;
            TGC.RaiseGiftEvent += TimePassedEventHandler;

            if (giftButton) giftButton.gameObject.SetActive(TGC && TGC.HaveGift);
            if (timerGroup) timerGroup.SetActive(TGC && !TGC.HaveGift);
        }

        private void OnDestroy()
        {
            if (TGC) 
            {
                TGC.TickRestDaysHourMinSecEvent -= RefreshTimerText;
                TGC.RaiseGiftEvent -= TimePassedEventHandler;
            }
        }
        #endregion regular

        private void RefreshTimerText(int days, int hours, int minutes, float seconds)
        {
            if (timerText && TGC)
            {
                    timerText.text = TGC.RestMinutes.ToString("00") + ":" + TGC.RestSeconds.ToString("00");
            }
        }

        private void TimePassedEventHandler(bool haveGift)
        {
            if (giftButton) giftButton.gameObject.SetActive(TGC && haveGift);
            if (timerGroup) timerGroup.SetActive(TGC && !haveGift);
        }

        public void ApplyReward()
        {
            int coins=0;
            if (TGC) TGC.ApplyReward(out coins);
            if (guiFlyerPrefab) guiFlyerPrefab.CreateFlyer(transform, coins.ToString());
        }
    }
}
