using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Mkey
{
    public class TimeGiftController : MonoBehaviour
    {
        private string giftTimerName = "gift_timer";
        [Tooltip("Time span until the next gift, minutes")]
        [SerializeField]
        private int giftTime = 20; 
        [Tooltip("If check, count the time between games")]
        [SerializeField]
        private bool countGlobalTime = false;

        [SerializeField]
        private int minCoins = 10;
        [SerializeField]
        private int maxCoins = 20;


        #region temp vars
        private GlobalTimer gTimer;
        private bool debug = true;
        public static TimeGiftController Instance;
        #endregion temp vars

        #region properties
        public bool IsWork { get; private set; }
        public float RestMinutes { get; private set; }
        public float RestSeconds { get; private set; }
        public float RestDays { get; private set; }
        public float RestHours { get; private set; }
        public bool HaveGift { get; private set; }
        #endregion properties

        #region events
        public Action <int, int, int, float> TickRestDaysHourMinSecEvent;
        public Action <bool> RaiseGiftEvent;
        #endregion events

        #region regular
        void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        void Start()
        {
            HaveGift = false;
            if (!countGlobalTime  && !IsWork)
            {
                StartNewTimer();
            }
            else if (countGlobalTime  && !IsWork)
            {
                if (GlobalTimer.Exist(giftTimerName)) StartExistingTimer();
                else StartNewTimer();
            }
        }

        void OnDestroy()
        {

        }

        void Update()
        {
            if (IsWork)
                gTimer.Update();
        }
        #endregion regular

        #region timerhandlers
        private void TickRestDaysHourMinSecHandler(int d, int h, int m, float s)
        {
            RestDays = d;
            RestHours = h;
            RestMinutes = m;
            RestSeconds = s;
            TickRestDaysHourMinSecEvent?.Invoke(d, h, m, s);
        }

        private void TimePassedHandler(double initTime, double realTime)
        {
            HaveGift = true;
            Debug.Log("Time Passed event : " + initTime + " : " + realTime);
            RaiseGiftEvent?.Invoke(HaveGift);
        }
        #endregion timerhandlers

        private void StartNewTimer()
        {
            if (debug) Debug.Log("start new");
            TimeSpan ts = new TimeSpan(0, giftTime, 0);
            gTimer = new GlobalTimer(giftTimerName, ts.Days, ts.Hours, ts.Minutes, ts.Seconds);
            gTimer.TickRestDaysHourMinSecEvent += TickRestDaysHourMinSecHandler;
            gTimer.TimePassedEvent += TimePassedHandler;
            IsWork = true;
        }

        private void StartExistingTimer()
        {
            gTimer = new GlobalTimer(giftTimerName);
            gTimer.TickRestDaysHourMinSecEvent += TickRestDaysHourMinSecHandler;
            gTimer.TimePassedEvent += TimePassedHandler;
            IsWork = true;
        }

        public void Restart()
        {
            if (HaveGift)
            {
                HaveGift = false;
                RaiseGiftEvent?.Invoke(HaveGift);
            }
            StartNewTimer();
        }

        public void ApplyReward(out int coins)
        {
            coins = UnityEngine.Random.Range(minCoins, maxCoins);
            if (SlotPlayer.Instance) SlotPlayer.Instance.AddCoins(coins);
            Restart();
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(TimeGiftController))]
    public class TimeGiftControllerEditor : Editor
    {
        private bool test = true;
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            if (EditorApplication.isPlaying)
            {
                if (test = EditorGUILayout.Foldout(test, "Test"))
                {
                    EditorGUILayout.BeginHorizontal("box");
                    if (GUILayout.Button("Restart"))
                    {
                        TimeGiftController t = (TimeGiftController)target;
                        t.Restart();
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
        }
    }
#endif
}