using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
    using UnityEditor;
#endif

namespace Mkey
{
    public class SlotControls : MonoBehaviour
    {
        [SerializeField]
        private SlotMenuController menuController;

        #region spin button
        [Header("Spin button options")]
        [Tooltip("In this mode, the endless spin of the slot starts. Press SPIN button to stop. Not work in auto mode and during free spin.")]
        [SerializeField]
        private bool manualStop = false;

        [Tooltip("Hold spin button > 2.0 sec to enable auto spin mode.")]
        [SerializeField]
        private bool holdToAutoSpin = true;
        #endregion spin button

        [Space(8)]
        #region main references


        #endregion main references

        #region default
        [Space(8)]
        [Tooltip("Check if you want to save coins, level, progress, facebook gift flag, sound settings")]
        [SerializeField]
        private bool saveData = false;

        [Tooltip("Default max line bet, min =1")]
        [SerializeField]
        private int maxLineBet = 20;

        [Tooltip("Default line bet at start, min = 1")]
        [SerializeField]
        private int defLineBet = 1;

        [Tooltip("Check if you want to play auto all free spins")]
        [SerializeField]
        private bool autoPlayFreeSpins = true;

        [Tooltip("Default auto spins count, min = 1")]
        [SerializeField]
        private int defAutoSpins = 1;

        [Tooltip("Max value of auto spins, min = 1")]
        [SerializeField]
        private int maxAutoSpins = int.MaxValue;
        #endregion default

        #region output
        [Space(16, order = 0)]
        [SerializeField]
        private Text LineBetSumText;
        [SerializeField]
        private Text TotalBetSumText;
        [SerializeField]
        private Text LinesCountText;
        [SerializeField]
        private Text FreeSpinText;
        [SerializeField]
        private Text SpinText;
        [SerializeField]
        private Text FreeSpinCountText;
        [SerializeField]
        private Text AutoSpinsCountText;
        [SerializeField]
        private Text InfoText;
        [SerializeField]
        private Text WinAmountText;
        #endregion output

        [SerializeField]
        private SpinButtonBehavior spinButton;
        [SerializeField]
        private AutoSpinButtonBehavior autoSpinButton;

        #region features
        [SerializeField]
        private HoldFeature hold;
        #endregion features

        #region references
        [SerializeField]
        private SlotController slot;
        [SerializeField]
        private LinesController linesController;
        #endregion references

        [SerializeField]
        private string coinsFormat = "0,0";

        #region keys
        private static string Prefix { get { return SceneLoader.GetCurrentSceneName() + SceneLoader.GetCurrentSceneBuildIndex(); } }

        private static string SaveAutoSpinsKey { get { return Prefix + "_mk_slot_autospins"; } } // current auto spins
        #endregion keys

        #region temp vars
        //private float levelxp;
        //private float oldLevelxp;
        //private int levelTweenId;
        private SceneButton[] buttons;
        private SlotPlayer MPlayer { get { return SlotPlayer.Instance; } }
        private GuiController MGUI { get { return GuiController.Instance; } }
        //private TweenLongValue balanceTween;
        private TweenLongValue winCoinsTween;
        private TweenLongValue infoCoinsTween;
    
        private SpinButtonBehavior spinButtonBehavior;

        private Dictionary<string, JackPot> jackPotsDict;
        private Dictionary<int, bool> activitySaveDict;
        #endregion temp vars

        #region events
        public Action<long> ChangeTotalBetEvent;
        public Action<long> ChangeLineBetEvent;
        public Action<int, bool> ChangeSelectedLinesEvent;

        public Action<int> ChangeFreeSpinsEvent;
        public Action<int> ChangeAutoSpinsEvent;
        public Action<int, int> ChangeAutoSpinsCounterEvent;
        public Action<bool> ChangeAutoSpinModeEvent;
        public Action TryToSetAutoSpinModeEvent;
        #endregion events

        #region properties
        public bool SaveData
        {
            get { return saveData; }
        }

        public int LineBet
        {
            get; private set;
        }

        public int TotalBet
        {
            get { return LineBet * SelectedLinesCount * HoldMultipler; }
        }

        public int HoldMultipler
        {
            get { return (hold && hold.enabled && hold.gameObject.activeSelf) ? hold.GetMultiplier() : 1; }
        }

        public int SelectedLinesCount
        {
            get; private set;
        }

        public bool AnyLineSelected
        {
            get { return SelectedLinesCount > 0; }
        }

        public int FreeSpins
        {
            get; private set;
        }

        public bool HasFreeSpin
        {
            get { return FreeSpins > 0; }
        }

        public bool AutoPlayFreeSpins
        {
            get { return autoPlayFreeSpins; }
        }

        public bool Auto { get; private set; }

        public int AutoSpinsCounter { get; private set; }

        public HoldFeature Hold { get { return hold; } }

        public bool UseHold
        {
            get { return (hold && hold.enabled && hold.gameObject.activeSelf); }
        }

        public bool ReelsSpin { get { return slot && slot.ReelsSpin; } }

        public bool UseManualStop => (manualStop && !Auto);

        public List<JackPot> jackPots { get; private set; }

        public SpinButtonBehavior SpinButton => spinButton;

        public AutoSpinButtonBehavior AutoSpinButton => autoSpinButton;

        public bool HoldToAutoSpin => holdToAutoSpin;

        #endregion properties

        #region saved properties
        public int MiniJackPot
        {
            get; private set;
        }

        public int MaxiJackPot
        {
            get; private set;
        }

        public int MegaJackPot
        {
            get; private set;
        }

        public int AutoSpinCount
        {
            get; private set;
        }
        #endregion saved properties

        #region regular
        private IEnumerator Start()
        {
            while (!MPlayer)
            {
                yield return new WaitForEndOfFrame();
            }

            buttons = GetComponentsInChildren<SceneButton>();

            jackPotsDict = new Dictionary<string, JackPot>();
            JackPot[] jps = GetComponentsInChildren<JackPot>();
            jackPots = new List<JackPot>();
            if(jps!=null && jps.Length > 0)
            {
                foreach (var item in jps)
                {
                    if(item && item.isActiveAndEnabled && !string.IsNullOrEmpty(item.JpName) && !jackPotsDict.ContainsKey(item.JpName))
                    {
                        jackPotsDict.Add(item.JpName, item);
                        jackPots.Add(item);
                    }
                }
            }


            // set player event handlers
            ChangeFreeSpinsEvent += ChangeFreeSpinsHandler;
            ChangeAutoSpinsEvent += ChangeAutoSpinsHandler;
            ChangeTotalBetEvent += ChangeTotalBetHandler;
            ChangeLineBetEvent += ChangeLineBetHandler;
            ChangeSelectedLinesEvent += ChangeSelectedLinesHandler;

            MPlayer.ChangeWinCoinsEvent += ChangeWinCoinsHandler;

            LoadLineBet();
            if (hold) hold.ChangeBetMultiplierEvent += (hm) => { RefreshBetLines(); };
            if (WinAmountText) winCoinsTween = new TweenLongValue(WinAmountText.gameObject, 0, 0.5f, 2, true, (w) => { if (this && WinAmountText) WinAmountText.text = (w > 0) ? w.ToString(coinsFormat) : "0"; });
            if (InfoText) infoCoinsTween = new TweenLongValue(InfoText.gameObject, 0, 0.5f, 2, true, (w) => { if (this) TextExtension.SetText(InfoText, (w > 0) ? w.ToString(coinsFormat) : "0"); });

            AutoSpinsCounter = 0;
            ChangeAutoSpinsCounterEvent += (r, i) => { if (this && AutoSpinsCountText) AutoSpinsCountText.text = i.ToString(); };
            LoadFreeSpins();
            LoadAutoSpins();
            TextExtension.SetText(InfoText, (SelectedLinesCount > 0) ? "Click to SPIN to start!" : "Select any slot line!");
            ChangeSelectedLinesEvent += (l, b) => { TextExtension.SetText(InfoText, (l > 0) ? "Click to SPIN to start!" : "Select any slot line!"); };
            SetControlActivity(true);
            Refresh();
        }

        void OnDestroy()
        {
            ChangeTotalBetEvent -= ChangeTotalBetHandler;
            ChangeLineBetEvent -= ChangeLineBetHandler;
            ChangeSelectedLinesEvent -= ChangeSelectedLinesHandler;

            // remove player event handlers
            if (MPlayer)
            {
                MPlayer.ChangeWinCoinsEvent -= ChangeWinCoinsHandler;
            }
        }

        private void OnValidate()
        {
            maxLineBet = Math.Max(1, maxLineBet);
            defLineBet = Math.Max(1, defLineBet);
            defLineBet = Mathf.Min(defLineBet, maxLineBet);

            maxAutoSpins = Math.Max(1, maxAutoSpins);
            defAutoSpins = Math.Max(1, defAutoSpins);
            defAutoSpins = Math.Min(defAutoSpins, maxAutoSpins);
        }
        #endregion regular

        /// <summary>
        /// Set all buttons interactble = activity, but startButton = startButtonAcivity
        /// </summary>
        /// <param name="activity"></param>
        /// <param name="spinButtonAcivity"></param>

        #region control activity
        public void SetControlActivity(bool activity)
        {
			//Debug.Log("SetControlActivity(bool activity)" + activity);
            if (menuController)  menuController.SetControlActivity(activity);
            if (linesController) { linesController.SetControlActivity(activity); }

            if (buttons != null)
            {
                foreach (var b in buttons)
                {
                    if (b) b.interactable = activity;
                }
            }

            if (spinButton) spinButton.SetInteractable(activity);
            if (autoSpinButton) autoSpinButton.SetInteractable(activity);
        }

        public void SetControlActivity(bool activity, bool spinButtonAcivity)
        {
			//Debug.Log("SetControlActivity(bool activity, bool spinButtonAcivity)" + activity + " " + spinButtonAcivity);
            SetControlActivity(activity);
            if (spinButton) spinButton.SetInteractable(spinButtonAcivity);
        }

        public void SetControlActivity(bool activity, bool spinButtonAcivity, bool autoSpinButtonAcivity)
        {
			//Debug.Log("SetControlActivity(bool activity, bool spinButtonAcivity, bool autoSpinButtonAcivity)" + activity + " " + spinButtonAcivity + " "+autoSpinButtonAcivity);
            SetControlActivity(activity);
            if (spinButton) spinButton.SetInteractable(spinButtonAcivity);
            if (autoSpinButton) autoSpinButton.SetInteractable(autoSpinButtonAcivity);
        }

        public void SaveControlActivity()
        {
            activitySaveDict = new Dictionary<int, bool>();

            Action<Dictionary<int, bool>, int, bool> addToDict = (dict, key, val) =>
            {
                if (!dict.ContainsKey(key)) dict.Add(key, val);
                else dict[key] = val;
            };

            if (menuController) 
            {
                addToDict(activitySaveDict, menuController.GetInstanceID(), menuController.ControlActivity);
            }
            if (linesController) 
            { 
                addToDict(activitySaveDict, linesController.GetInstanceID(), linesController.ControlActivity);
            }

            if (buttons != null)
            {
                foreach (var b in buttons)
                {
                    if (b) addToDict(activitySaveDict, b.GetInstanceID(), b.interactable);
                }
            }
        }

        public void RestoreControlActivity()
        {
            if (activitySaveDict == null) return;

            if (menuController && activitySaveDict.ContainsKey(menuController.GetInstanceID()))
            { 
                menuController.SetControlActivity(activitySaveDict[menuController.GetInstanceID()]); 
            }

            if (linesController && activitySaveDict.ContainsKey(linesController.GetInstanceID())) 
            { 
                linesController.SetControlActivity(activitySaveDict[linesController.GetInstanceID()]); 
            }

            if (buttons != null)
            {
                foreach (var b in buttons)
                {
                    if (b && activitySaveDict.ContainsKey(b.GetInstanceID())) b.interactable = activitySaveDict[b.GetInstanceID()];
                }
            }

            activitySaveDict = null;
        }
        #endregion control activity

        #region refresh
        /// <summary>
        /// Refresh gui data : Balance,  BetCount, freeSpin
        /// </summary>
        private void Refresh()
        {
            RefreshBetLines();
            RefreshSpins();
            if (WinAmountText) WinAmountText.text = 0.ToString();
        }

        /// <summary>
        /// Refresh gui lines, bet
        /// </summary>
        private void RefreshBetLines()
        {
            if (MPlayer)
            {
                if (LineBetSumText) LineBetSumText.text = LineBet.ToString();
                if (TotalBetSumText) TotalBetSumText.text = TotalBet>=10 ? TotalBet.ToString(coinsFormat) : TotalBet.ToString();
                if (LinesCountText) LinesCountText.text = SelectedLinesCount.ToString();
            }
        }

        /// <summary>
        /// Refresh gui spins
        /// </summary>
        private void RefreshSpins()
        {
            if (AutoSpinsCountText) AutoSpinsCountText.text = AutoSpinCount.ToString();
            if (FreeSpinText) FreeSpinText.text = (FreeSpins > 0) ? "Free" : "";
            if (FreeSpinCountText) FreeSpinCountText.text = (FreeSpins > 0) ? FreeSpins.ToString() : "";
        }
        #endregion refresh

        #region control buttons
        public void LinesPlus_Click()
        {
            AddSelectedLinesCount(1, true);
        }

        public void LinesMinus_Click()
        {
            AddSelectedLinesCount(-1, false);
        }

        public void LineBetPlus_Click()
        {
            AddLineBet(1);
        }

        public void LineBetMinus_Click()
        {
            AddLineBet(-1);
        }

        public void AutoSpinPlus_Click()
        {
            AddAutoSpins(1);
        }

        public void AutoSpinMinus_Click()
        {
            AddAutoSpins(-1);
        }

        public void MaxBet_Click()
        {
            linesController.SelectAllLines(true);
            SetMaxLineBet();
        }

        public void Spin_PointerDown()
        {
            
        }

        //public void Spin_LongPointerDown()
        //{
        //  //  if (!Auto && holdToAutoSpin) TryToSetAutoSpinModeEvent?.Invoke();
        //}

        //public void LongPressSpin_Click()
        //{
        // //   SpinClickEvent?.Invoke(Auto);

        //    //if (holdToAutoSpin)
        //    //{
        //    //    if (Auto)
        //    //    {
        //    //        ResetAutoSpinsMode();
        //    //        return;
        //    //    }

        //    //    SetAutoSpinsMode();
        //    //    slot.TryToRunSlot();
        //    //}
        //    //else
        //    //{
        //    //    if (Auto)
        //    //    {
        //    //        ResetAutoSpinsMode();
        //    //        return;
        //    //    }
        //    //    slot.TryToRunSlot();
        //    //}

        //}

        //public void Spin_Click()
        //{
        //   // SpinClickEvent?.Invoke(Auto);
        //    //if (Auto)
        //    //{
        //    //    ResetAutoSpinsMode();
        //    //    return;
        //    //}
        //}

        //public void AutoSpin_Click()
        //{
        //    //if (Auto) 
        //    //{ 
        //    //    ResetAutoSpinsMode();
        //    //    return; 
        //    //}
        //    //SetAutoSpinsMode();
        //}
        #endregion control buttons

        #region event handlers
        private void ChangeFreeSpinsHandler(int newFreeSpinsCount)
        {
            if (this)
            {
                if (FreeSpinText) FreeSpinText.text = (FreeSpins > 0) ? "Free" : "";
                if (FreeSpinCountText) FreeSpinCountText.text = (newFreeSpinsCount > 0) ? newFreeSpinsCount.ToString() : "";
            }
        }

        private void ChangeAutoSpinsHandler(int newAutoSpinsCount)
        {
            if (this && AutoSpinsCountText) AutoSpinsCountText.text = newAutoSpinsCount.ToString();
        }

        private void ChangeTotalBetHandler(long newTotalBet)
        {
            if (this && TotalBetSumText) TotalBetSumText.text = TotalBet >= 10 ? TotalBet.ToString(coinsFormat) : TotalBet.ToString();
        }

        private void ChangeLineBetHandler(long newLineBet)
        {
            if (this && LineBetSumText) LineBetSumText.text = newLineBet.ToString();

        }

        private void ChangeSelectedLinesHandler(int newCount, bool burn)
        {
            if (this && LinesCountText) LinesCountText.text = newCount.ToString();
        }

        private void ChangeWinCoinsHandler(long newCount)
        {
            if (winCoinsTween != null) winCoinsTween.Tween(newCount, 100);
            if (infoCoinsTween != null) infoCoinsTween.Tween(newCount, 100);
        }
        #endregion event handlers

        #region LineBet
        /// <summary>
        /// Change line bet and save result
        /// </summary>
        /// <param name="count"></param>
        public void AddLineBet(int count)
        {
            SetLineBet(LineBet + count);
        }

        /// <summary>
        /// Set line bet and save result
        /// </summary>
        /// <param name="count"></param>
        public void SetLineBet(int count)
        {
            count = Mathf.Max(1, count);
            count = Mathf.Min(count, maxLineBet);
            bool changed = (LineBet != count);
            LineBet = count;
            if (changed)
            {
                ChangeLineBetEvent?.Invoke(LineBet);
                ChangeTotalBetEvent?.Invoke(TotalBet);
            }
        }

        /// <summary>
        /// Load default line bet
        /// </summary>
        private void LoadLineBet()
        {
            SetLineBet(defLineBet);
        }

        internal void SetMaxLineBet()
        {
            SetLineBet(maxLineBet);
        }

        /// <summary>
        /// If has money for bet, dec money, and return true
        /// </summary>
        /// <returns></returns>
        internal bool ApplyBet()
        {
            if (MPlayer.HasMoneyForBet(TotalBet))
            {
                MPlayer.AddCoins(-TotalBet);
                return true;
            }
            else
            {
                return false;
            }
        }


        #endregion LineBet

        #region lines
        internal void AddSelectedLinesCount(int count, bool burn)
        {
            SetSelectedLinesCount(SelectedLinesCount + count, burn);
        }

        internal void SetSelectedLinesCount(int count, bool burn)
        {
            count = Mathf.Max(1, count);
            count = Mathf.Min(linesController.LinesCount, count);

            bool changed = (SelectedLinesCount != count);
            SelectedLinesCount = count;
            if (changed)
            {
                ChangeSelectedLinesEvent?.Invoke(count, burn);
                ChangeTotalBetEvent?.Invoke(TotalBet);
            }
        }
        #endregion lines

        #region FreeSpins
        /// <summary>
        /// Change free spins count and save result
        /// </summary>
        /// <param name="count"></param>
        public void AddFreeSpins(int count)
        {
            SetFreeSpinsCount(FreeSpins + count);
        }

        /// <summary>
        /// Set Free spins count and save result
        /// </summary>
        /// <param name="count"></param>
        public void SetFreeSpinsCount(int count)
        {
            count = Mathf.Max(0, count);
            bool changed = (FreeSpins != count);
            FreeSpins = count;
            if (changed) ChangeFreeSpinsEvent?.Invoke(FreeSpins);
        }

        /// <summary>
        /// Load default free spins count
        /// </summary>
        private void LoadFreeSpins()
        {
            SetFreeSpinsCount(0);
        }

        /// <summary>
        /// If has free spins, dec free spin and return true.
        /// </summary>
        /// <returns></returns>
        internal bool ApplyFreeSpin()
        {
            if (HasFreeSpin)
            {
                AddFreeSpins(-1);
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion FreeSpins

        #region AutoSpins
        /// <summary>
        /// Change auto spins cout and save result
        /// </summary>
        /// <param name="count"></param>
        public void AddAutoSpins(int count)
        {
            SetAutoSpinsCount(AutoSpinCount + count);
        }

        /// <summary>
        /// Set level and save result
        /// </summary>
        /// <param name="count"></param>
        public void SetAutoSpinsCount(int count)
        {
            count = Mathf.Max(1, count);
            count = Mathf.Min(count, maxAutoSpins);
            bool changed = (AutoSpinCount != count);
            AutoSpinCount = count;
            if (SaveData && changed)
            {
                string key = SaveAutoSpinsKey;
                PlayerPrefs.SetInt(key, AutoSpinCount);
            }
            if (changed) ChangeAutoSpinsEvent?.Invoke(AutoSpinCount);
        }

        /// <summary>
        /// Load serialized auto spins count or set default auto spins count
        /// </summary>
        private void LoadAutoSpins()
        {
            SetAutoSpinsCount(defAutoSpins);
        }

        public void IncAutoSpinsCounter()
        {
            SetAutoSpinsCounter(AutoSpinsCounter + 1);
        }

        public void SetAutoSpinsCounter(int count)
        {
            count = Mathf.Max(0, count);
            bool changed = (count != AutoSpinsCounter);
            AutoSpinsCounter = count;
            if (changed) ChangeAutoSpinsCounterEvent?.Invoke(AutoSpinsCounter, AutoSpinCount);
        }

        public void ResetAutoSpinsMode()
        {
            Auto = false;
            ChangeAutoSpinModeEvent?.Invoke(false);
        }

        public void SetAutoSpinsMode()
        {
            SetAutoSpinsCounter(0);
            Auto = true;
            ChangeAutoSpinModeEvent?.Invoke(true);
        }
        #endregion AutoSpins

        public void SetDefaultData()
        {
            foreach (var item in jackPotsDict)
            {
                if (item.Value) item.Value.ResetAmount();
            }

            SetLineBet(defLineBet);
            SetAutoSpinsCount(defAutoSpins);
        }

        internal void JPWinCancel()
        {
            foreach (var item in jackPots)
            {
                item.WinCancel();
            }
        }

        internal void SetSpinButtonText(string text)
        {
            TextExtension.SetText(SpinText, text);
        }

        #region utils
        private string GetMoneyName(int count)
        {
            if (count > 1) return "coins";
            else return "coin";
        }

        /// <summary>
        /// Add new value to dictionary or replace existing value
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="key"></param>
        /// <param name="val"></param>

        #endregion utils
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(SlotControls))]
    public class SlotControlsEditor : Editor
    {
        private bool test = false;
        Color lineBgColor;
        Sprite normal;
        Sprite pressed;

        Font buttonTextFont;
        Material buttonTextMaterial;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            SlotControls t = (SlotControls)target;

            if (!EditorApplication.isPlaying)
            {
                if (test = EditorGUILayout.Foldout(test, "Development"))
                {
                    lineBgColor = EditorGUILayout.ColorField("New Color", lineBgColor);

                    EditorGUILayout.BeginHorizontal("box");
                    if (GUILayout.Button("set_color bg"))
                    {
                        LineBehavior [] lbs = t.GetComponentsInChildren<LineBehavior>(true);
                        foreach (var item in lbs)
                        {
                            item.lineInfoBGColor = lineBgColor;
                        }

                    }

                    if (GUILayout.Button("rebuild handles"))
                    {
                        LineCreator[] lbs = t.GetComponentsInChildren<LineCreator>(true);
                        foreach (var item in lbs)
                        {
                            item.SetInitial();
                            EditorUtility.SetDirty(item);
                        }

                    }

                    if (GUILayout.Button("clean raycasters"))
                    {
                        LineBehavior[] lbs = t.GetComponentsInChildren<LineBehavior>(true);
                        foreach (var item in lbs)
                        {
                            List<RayCaster> rcsL = new List<RayCaster>(item.rayCasters);
                            rcsL.RemoveAll((rc) => { return !rc; });
                            item.rayCasters = rcsL.ToArray();
                            EditorUtility.SetDirty(item);
                        }

                    }
                    EditorGUILayout.EndHorizontal();
               
                    EditorGUILayout.BeginVertical();
                    normal = (Sprite)EditorGUILayout.ObjectField("normal sprite button", (UnityEngine.Object)normal, typeof(Sprite));
                    pressed = (Sprite)EditorGUILayout.ObjectField("pressed sprite button", (UnityEngine.Object)pressed, typeof(Sprite));
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.BeginHorizontal("box");
                    if (GUILayout.Button("Set new button sprites"))
                    {
                        LineButtonBehavior[] lbs = t.GetComponentsInChildren<LineButtonBehavior>(true);
                        foreach (var item in lbs)
                        {
                            item.SetSprites(normal, pressed);
                        }

                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginVertical();
                    buttonTextFont = (Font)EditorGUILayout.ObjectField("button text font", (UnityEngine.Object)buttonTextFont, typeof(Font));
                    if (buttonTextFont) buttonTextMaterial = (Material)EditorGUILayout.ObjectField("button text material", (UnityEngine.Object)buttonTextMaterial, typeof(Material));
                    EditorGUILayout.EndVertical();

                    EditorGUILayout.BeginHorizontal("box");

                    if (buttonTextFont)
                    {
                        if (GUILayout.Button("Set new button text font"))
                        {
                            LineButtonBehavior[] lbs = t.GetComponentsInChildren<LineButtonBehavior>(true);
                            foreach (var item in lbs)
                            {
                                TextMesh tM = item.GetComponentInChildren<TextMesh>();
                                if (tM)
                                {
                                    tM.font = buttonTextFont;
                                }
                            }
                        }

                        if (buttonTextMaterial && GUILayout.Button("Set new button text material"))
                        {
                            LineButtonBehavior[] lbs = t.GetComponentsInChildren<LineButtonBehavior>(true);
                            foreach (var item in lbs)
                            {
                                MeshRenderer mR = item.GetComponentInChildren<MeshRenderer>();
                                if (mR)
                                {
                                    mR.material = buttonTextMaterial;
                                }
                            }

                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
        }
    }
#endif
}