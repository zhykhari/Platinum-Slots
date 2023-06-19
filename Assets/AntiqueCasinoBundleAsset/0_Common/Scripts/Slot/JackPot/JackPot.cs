using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
    using UnityEditor;
    using UnityEditor.SceneManagement;
#endif

namespace Mkey
{
    public class JackPot : MonoBehaviour
    {
        [SerializeField]
        private string jpName;
 
        #region main references
        [SerializeField]
        private SlotController slot;
        [SerializeField]
        private SlotControls controls;
        [SerializeField]
        private LampsController[] lamps;
        [SerializeField]
        private CoinProcAnim[] coinsFountains;
        [SerializeField]
        private SpriteRenderer[] winRenderers;
        #endregion main references

        #region settings
        public int jp_symbol_id = -1;
        [Tooltip("Jackpot amount at game start")]
        [SerializeField]
        private long startAmount;
        [SerializeField]
        private JackPotIncType incType;
        [Tooltip("Increase jackpot amount value, after each spin")]
        [SerializeField]
        private long incValue = 1;
        [Tooltip("Count identical symbols on screen")]
        public int symbolsCount = 7;
        #endregion settings

        #region output
        [SerializeField]
        public Text titleText;
        [SerializeField]
        public TextMesh titleTextMesh;
        [SerializeField]
        public Text amountText;
        [SerializeField]
        public TextMesh amountTextMesh;
        [SerializeField]
        private string coinsFormat = "0,0";
        #endregion output

        #region win prefabs
        [SerializeField]
        private WarningMessController jackPotWinPuPrefab;
        [SerializeField]
        private GameObject jackPotWinPrefab;
        #endregion win prefabs

        #region properties
        public LampsController[] Lamps { get { return lamps; } }
        public CoinProcAnim[] CoinsFoutains { get { return coinsFountains; } }
        public SpriteRenderer[] WinRenderers { get { return winRenderers; } }
        public string JpName => jpName;

        public List<SlotSymbol> WinSymbols { get; private set; }
        public bool HaveWin => (WinSymbols != null && WinSymbols.Count == symbolsCount);

        public long Amount { get; private set; }
        public bool SaveData { get { return controls && controls.SaveData; } }
        private string Prefix { get { return MachineID; } }

        private string MachineID { get { return slot ? slot.MachineID : ""; } }

        private string SaveKey { get { return Prefix + "_mk_slot_jackpot_" + (string.IsNullOrEmpty(jpName) ? name : jpName); } }
        #endregion properties

        #region events
        public Action<long> ChangeEvent;
        public Action<long> LoadEvent;
        #endregion events


        #region temp vars
        private GameObject jackPotWinGO;
        private WarningMessController jackPotWinPu;
        private GuiController MGUI { get { return GuiController.Instance; } }
    
        private Dictionary<int, List<SlotSymbol>> sD;
        private Action<GameObject, float, Action> waitAction = (g, time, callBack) => { SimpleTween.Value(g, 0, 1, time).AddCompleteCallBack(callBack); };
        #endregion temp vars

        #region regular
        private void Start()
        {
            Validate();
            Load();
            ChangeEvent += RefreshOutput;
            LoadEvent += RefreshOutput;
            RefreshOutput(Amount);
            if (slot) slot.EndSpinEvent += Increase;
        }

        private void OnValidate()
        {
            Validate();
        }

        private void OnDestroy()
        {

        }
        #endregion regular

        void Validate()
        {
            incValue = (incValue < 0) ? 0 : incValue;
            symbolsCount = Mathf.Max(1, symbolsCount);
        }

        private void Increase()
        {
            Add((incType == JackPotIncType.Const) ? incValue : (long)(startAmount * (double)incValue / 100f));
        }

        /// <summary>
        /// Add mini jack pot and save result
        /// </summary>
        /// <param name="count"></param>
        public void Add(long count)
        {
            SetCount(Amount + count);
        }

        /// <summary>
        /// Set mini jackpot and save result
        /// </summary>
        /// <param name="count"></param>
        public void SetCount(long count)
        {
            count = Math.Max(startAmount, count);
            bool changed = (Amount != count);
            Amount = count;
            if (SaveData && changed)
            {
                string key = SaveKey;
                PlayerPrefsExtension.SetLong(key, Amount);
            }
            if (changed) 
            { 
                ChangeEvent?.Invoke(Amount);
                GameEvents.ChangeJackpotEvent?.Invoke(MachineID, JpName, Amount);
            }
        }

        /// <summary>
        /// Load serialized jackpot amount or set defaults
        /// </summary>
        public void Load()
        {
            if (SaveData)
            {
                string key = SaveKey;
                Debug.Log("Load jp: " + SaveKey);
                long count = PlayerPrefsExtension.GetLong(key, startAmount);
                long countClamp = Math.Max(startAmount, count);
                Amount = countClamp;
                bool changed = (Amount != count);
                if (changed)
                {
                    PlayerPrefsExtension.SetLong(key, Amount);
                }
            }
            else
            {
                Amount = startAmount;
            }
            LoadEvent?.Invoke(Amount);
        }

        public void ResetAmount()
        {
            SetCount(startAmount);
        }

        internal void WinShow()
        {
            if (jackPotWinGO) Destroy(jackPotWinGO);
            if (jackPotWinPu) Destroy(jackPotWinPu.gameObject);

            if (jackPotWinPrefab) jackPotWinGO = Instantiate(jackPotWinPrefab, transform);
            if (jackPotWinPuPrefab) jackPotWinPu = MGUI.ShowMessage(jackPotWinPuPrefab, "", Amount.ToString(), 5f, null);
        }

        internal void WinCancel()
        {
            if (jackPotWinPu) jackPotWinPu.CloseWindow();
            if (jackPotWinGO) Destroy(jackPotWinGO);
        }

        internal void WinSymbolsPlay(string winTag, float winShowTime, Action completeCallBack)
        {
            if (WinSymbols != null && WinSymbols.Count > 0)
            {
                ParallelTween pT = new ParallelTween();
                foreach (var item in WinSymbols)
                {
                    pT.Add((callBack) =>
                    {
                        item.ShowWinPrefab(winTag);
                        waitAction(item.gameObject, winShowTime, callBack);
                    });
                }
                pT.Start(completeCallBack);
            }
            else
            {
                completeCallBack?.Invoke();
            }
        }

        internal void WinSymbolsPlayCancel()
        {
            if (WinSymbols != null)
                foreach (var item in WinSymbols)
                {
                    item.DestroyWinObject();
                }
        }

        private void RefreshOutput(long newAmount)
        {
            if (this && amountText) amountText.text = newAmount.ToString(coinsFormat);
            if (this && amountTextMesh) amountTextMesh.text = newAmount.ToString(coinsFormat);
        }

        /// <summary>
        /// Instantiate particles for each winning symbol
        /// </summary>
        internal void ShowWinSymbolsParticles(bool activate)
        {
            if (WinSymbols != null) WinSymbols.ForEach((wS) => { wS.ShowParticles(activate, slot.particlesStars); });
        }

        internal void ResetWin()
        {
            WinSymbols = null;
        }

        public void SearchWin()
        {
            WinSymbols = null;
            if (!isActiveAndEnabled) return ;
            sD = new Dictionary<int, List<SlotSymbol>>();

            // create symbols dictionary
            foreach (var item in slot.slotGroupsBeh)
            {
                RayCaster[] rCs = item.RayCasters;
                foreach (var rc in rCs)
                {
                    SlotSymbol s = rc.Symbol;
                    int sID = s.IconID;
                    if (sD.ContainsKey(sID))
                    {
                        sD[sID].Add(s);
                    }
                    else
                    {
                        sD.Add(sID, new List<SlotSymbol> { s });
                    }
                }
            }

            // search jackPot id if symbol is any
            if (jp_symbol_id == -1)
            {
                foreach (var item in sD)
                {
                    int sCount = item.Value.Count;
                    int id = item.Key;
                    if (sCount == symbolsCount)
                    {
                        WinSymbols = sD[id];
                        return;
                    }
                }
            }

            else
            {
                int id = jp_symbol_id;
                if (sD.ContainsKey(id))
                {
                    int sCount = sD[id].Count;
                    if (sCount == symbolsCount)
                    {
                        WinSymbols = sD[id];
                        return;
                    }
                }
            }
        }

        public static long GetJPAmount(string machine_id, string jp_name, long defaultValue)
        {
            string key = machine_id + "_mk_slot_jackpot_" + jp_name;
            Debug.Log("get jp: " + key);
            long amount = PlayerPrefsExtension.GetLong(key, defaultValue);
            long amountClamp = Math.Max(defaultValue, amount);
            return amountClamp;
        }
    }


#if UNITY_EDITOR
    [CustomEditor(typeof(JackPot))]
    public class JackpotEditor : Editor
    {
        private SlotController slotController;
        private JackPot jackPot;

        private static bool showMainRef = false;
        private static bool showOutput = false;
        private static bool showWinPrefabs = false;
        private static bool showSettings = false;
       
        public override void OnInspectorGUI()
        {
            jackPot = (JackPot)target;
            serializedObject.Update();

            EditorExt.ShowPropertiesBox(serializedObject,  new string[] {"jpName"},  true);

            if (!slotController)
            {
                slotController = jackPot.GetComponentInParent<SlotController>();
            }

            if (!slotController || !jackPot)
            {
                DrawDefaultInspector();
                return;
            }

            string[] sChoise = slotController.GetIconNames(true);

            EditorExt.ShowPropertiesBoxFoldOut("Settings", ref showSettings, ()=>
            {
                EditorGUILayout.BeginHorizontal();
                if (sChoise != null || sChoise.Length > 0)
                {
                    int choiseIndex = jackPot.jp_symbol_id + 1;
                    int oldIndex = choiseIndex;
                    EditorGUILayout.LabelField("Select Jackpot symbol ");
                    EditorExt.ShowChoiseLO(sChoise, jackPot, ref choiseIndex);
                    jackPot.jp_symbol_id = choiseIndex - 1;
                }
                EditorGUILayout.EndHorizontal();

                EditorExt.ShowProperties(serializedObject, new string[] { "symbolsCount", "incValue", "incType", "startAmount" }, false);
            });

            EditorExt.ShowPropertiesBoxFoldOut(serializedObject, "Hierarchy references:", new string[] { "slot", "controls" , "lamps", "coinsFountains", "winRenderers" }, ref showMainRef, true);
            EditorExt.ShowPropertiesBoxFoldOut(serializedObject, "Output:", new string[] { "titleText", "titleTextMesh", "amountText", "amountTextMesh", "coinsFormat" }, ref showOutput, true);
            EditorExt.ShowPropertiesBoxFoldOut(serializedObject, "Win prefabs:", new string[] { "jackPotWinPuPrefab", "jackPotWinPrefab" }, ref showWinPrefabs, true);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Log"))
            {
                if (!EditorApplication.isPlaying) jackPot.Load();
                Debug.Log(jackPot.JpName + ": " + jackPot.Amount);
            }

            if (GUILayout.Button("+100"))
            {
                if (!EditorApplication.isPlaying) jackPot.Load();
                jackPot.Add(100);
                Debug.Log(jackPot.JpName + ": " + jackPot.Amount);
            }
            EditorGUILayout.EndHorizontal();
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
