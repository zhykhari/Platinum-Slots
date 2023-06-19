using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;

/*
    26.11.2021
 */
namespace Mkey
{
    public class DayNightTracker : MonoBehaviour
    {
        public UnityEvent DayEvent;
        public UnityEvent NightEvent;

        public DateTime dateTime;

        private float checkPeriod = 1f;
        private float nextCheck = 0f;
        private bool isDay = true;

        #region regular
        private void Start()
        {
            if (IsDay())
            {
                DayEvent?.Invoke();
            }
            else
            {
                NightEvent?.Invoke();
            }
        }

        void Update()
        {
            if (nextCheck < Time.time)
            {
                nextCheck = Time.time + checkPeriod;
                CheckTime();
            }
        }
        #endregion regular

        private void CheckTime()
        {
            bool _isDay = IsDay();
          
            if(isDay && !_isDay) 
            {
                NightEvent?.Invoke();
            }
            else if (!isDay && _isDay)
            {
                DayEvent?.Invoke();
            }
            isDay = _isDay;
        }

        private bool IsDay()
        {
            dateTime = DateTime.Now;
            int hour = dateTime.Hour; // 0 - 23
            bool nightTime = (hour >= 0 && hour < 5);

            if (nightTime)  // check night
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}