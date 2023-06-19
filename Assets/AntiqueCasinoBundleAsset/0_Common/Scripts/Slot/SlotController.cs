using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine.Events;

namespace Mkey
{
    public enum WinLineFlashing {All, Sequenced, None}
    public enum JackPotIncType { Const, Percent } // add const value or percent of start value
    public enum SlotSpinType {Free, Auto, Normal, EndLess }

    public class SlotController : MonoBehaviour
    {
        [SerializeField]
        private string machineID;

        #region main reference
        [SerializeField]
        public SlotControls controls;
        [SerializeField]
        public WinController winController;
        [SerializeField]
        private AudioClip spinSound;
        [SerializeField]
        private AudioClip looseSound;
        [SerializeField]
        private AudioClip winCoinsSound;
        [SerializeField]
        private AudioClip winFreeSpinSound;
        #endregion main reference

        #region icons

        [SerializeField, ArrayElementTitle("iconSprite"), NonReorderable]
        public SlotIcon[] slotIcons;

        [Space(8)]
        [SerializeField]
        public WinSymbolBehavior[] winSymbolBehaviors;
        #endregion icons

        #region payTable
        public List<PayLine> payTable;
        internal List<PayLine> payTableFull; // extended  if useWild
        #endregion payTable

        #region special major
        public int scatter_id;
        public int wild_id;
        public WildFeature wildFeature;
        public bool useWild;
        public bool useScatter;
        #endregion special major

        #region scatter paytable
        public List<ScatterPay> scatterPayTable;
        #endregion scatter paytable

        #region prefabs
        public GameObject tilePrefab;
        public GameObject particlesStars;
        [SerializeField]
        private WarningMessController BigWinPrefab;
        [SerializeField]
        private WarningMessController FreeGamesPUPrefab;
        #endregion prefabs

        #region slotGroups
        public SlotGroupBehavior[] slotGroupsBeh;
        #endregion slotGroups

        #region tweenTargets
        public Transform bottomJumpTarget;
        public Transform topJumpTarget;
        #endregion tweenTargets

        #region spin options
        [SerializeField]
        private EaseAnim inRotType = EaseAnim.EaseLinear; // in rotation part
        [SerializeField]
        [Tooltip("Time in rotation part, 0-1 sec")]
        private float inRotTime = 0.3f;
        [SerializeField]
        [Tooltip("In rotation part angle, 0-10 deg")]
        private float inRotAngle = 7;

        [Space(16, order = 0)]
        [SerializeField]
        private EaseAnim outRotType = EaseAnim.EaseLinear;   // out rotation part
        [SerializeField]
        [Tooltip("Time out rotation part, 0-1 sec")]
        private float outRotTime = 0.3f;
        [SerializeField]
        [Tooltip("Out rotation part angle, 0-10 deg")]
        private float outRotAngle = 7;

        [Space(16, order = 0)]
        [SerializeField]
        private EaseAnim mainRotateType = EaseAnim.EaseLinear;   // main rotation part
        [SerializeField]
        [Tooltip("Time main rotation part, sec")]
        private float mainRotateTime = 4f;
        [Tooltip("min 0% - max 20%, change rotateTime")]
        [SerializeField]
        private int mainRotateTimeRandomize = 10;
        #endregion spin options

        #region options
        public WinLineFlashing winLineFlashing = WinLineFlashing.Sequenced;
        public bool winSymbolParticles = true;
        public RNGType RandomGenerator = RNGType.Unity;
        [SerializeField]
        [Tooltip("Multiply win coins by bet multiplier")]
        public bool useLineBetMultiplier = true;
        [SerializeField]
        [Tooltip("Multiply win spins by bet multiplier")]
        public bool useLineBetFreeSpinMultiplier = true;
        [SerializeField]
        [Tooltip("Debug to console predicted symbols")]
        private bool debugPredictSymbols = false;
        #endregion options 

        #region levelprogress
        [SerializeField]
        [Tooltip("Multiply level progress by bet multiplier")]
        public bool useLineBetProgressMultiplier = true;
        [SerializeField]
        [Tooltip("Player level progress for loose spin")]
        public float loseSpinLevelProgress = 0.5f;
        [SerializeField]
        [Tooltip("Player level progress for win spin per win line")]
        public float winSpinLevelProgress = 2.0f;
        #endregion level progress

        public bool ReelsSpin { get; private set; }
        public bool IsFreeSpin { get; set; }

        #region temp vars
        private int slotTilesCount = 30;
        private WaitForSeconds wfs1_0;
        private WaitForSeconds wfs0_2;
        private WaitForSeconds wfs0_1;
        private RNG rng; // random numbers generator

        private uint spinCount = 0;

        private bool playFreeSpins = false;

        private bool startFreeGames = false;
        private bool endFreeGames = false;
        private bool logStatistic = true;
        private SoundMaster MSound { get { return SoundMaster.Instance; } }
        private SlotPlayer MPlayer { get { return SlotPlayer.Instance; } }
        private GuiController MGUI {get { return GuiController.Instance; } }
        private List<List<Triple>> tripleCombos;

        #endregion temp vars

        #region events
        public Action StartSpinEvent;
        public Action EndSpinEvent;
        public Action BeginWinCalcEvent;
        public Action <bool> EndWinCalcEvent; // <win>
        public Action AnyWinEvent;
        public Action <double> WinCoinsEvent;
        public Action<int> FreeSpinWinEvent;
        public UnityEvent StartFreeGamesEvent;
        public UnityEvent EndFreeGamesEvent;
        public UnityEvent AnyWinShowEvent;
        #endregion events

        #region dev
        public string payTableJsonString;
        #endregion dev

        #region state machine
        private StateMachine<SlotController> stateMachine;
        public IddleState iddleState;
        public LooseState looseState;
        public WinState winState;
        public FreeInputWinState freeInputWinState;
        public MiniGameState miniGameState;
        public SpinState spinState;
        public PreSpinState preSpinState;
        #endregion state machine

        public string MachineID => machineID;

        public static SlotController CurrentSlot { get; private set; }

        public bool useWildInFirstPosition = false;

        #region regular
        private void OnValidate()
        {
            Validate();
        }

        void Validate()
        {
            mainRotateTimeRandomize = (int)Mathf.Clamp(mainRotateTimeRandomize, 0, 20);

            inRotTime = Mathf.Clamp(inRotTime, 0, 1f);
            inRotAngle = Mathf.Clamp(inRotAngle, 0, 10);

            outRotTime = Mathf.Clamp(outRotTime, 0, 1f);
            outRotAngle = Mathf.Clamp(outRotAngle, 0, 10);

            if (scatterPayTable != null)
            {
                foreach (var item in scatterPayTable)
                {
                    if (item!=null)
                    {
                        item.payMult = Mathf.Max(1, item.payMult);
                    }
                }
            }
        }
      
        void Start()
        {
            wfs1_0 = new WaitForSeconds(1.0f);
            wfs0_2 = new WaitForSeconds(0.2f);
            wfs0_1 = new WaitForSeconds(0.1f);

            // create reels
            int slotsGrCount = slotGroupsBeh.Length;
            ReelData[] reelsData = new ReelData[slotsGrCount];
            ReelData reelData;
            int i = 0;
            foreach (SlotGroupBehavior sGB in slotGroupsBeh)
            {
                reelData = new ReelData(sGB.symbOrder);
                reelsData[i++] = reelData;
                sGB.CreateSlotCylinder(slotIcons, slotTilesCount, tilePrefab);
            }

            CreateFullPaytable();
            rng = new RNG(RNGType.Unity, reelsData);
            CurrentSlot = this;

            #region state machine
            stateMachine = new StateMachine<SlotController>();
            iddleState = new IddleState(this, stateMachine);

            winState = new WinState(this, stateMachine);
            freeInputWinState = new FreeInputWinState(this, stateMachine);
            looseState = new LooseState(this, stateMachine);
            miniGameState = new MiniGameState(this, stateMachine);
            spinState = new SpinState(this, stateMachine);
            preSpinState = new PreSpinState(this, stateMachine);
            stateMachine.Initialize(iddleState);
            #endregion state machine

            if(useWild && wildFeature) wildFeature.InitStart();

            StartSpinEvent += () => GameEvents.SpinEvent?.Invoke(MachineID);
            AnyWinEvent += () => GameEvents.WinEvent?.Invoke(MachineID);
            WinCoinsEvent += (coins) => GameEvents.WinCoinsEvent?.Invoke(MachineID, (int)coins);
        }

        void Update()
        {
            rng.Update();
            #region state machine
            stateMachine.CurrentState.LogicUpdate();
            #endregion state machine
        }

        private void OnDestroy()
        {
           
        }
        #endregion regular
       
        internal void StopSlot()
        {
            if (!ReelsSpin) return;
            for (int i = 0; i < slotGroupsBeh.Length; i++)
            {
                int r = randsCache[i];
                slotGroupsBeh[i].SetNextOrder(r);
            }
        }

        internal void ForceStopSlot()
        {
            if (!ReelsSpin) return;
            for (int i = 0; i < slotGroupsBeh.Length; i++)
            {
                slotGroupsBeh[i].ForceStop();
            }
        }

        public void RunSlot()
        {
			StopAllCoroutines();

           // Debug.Log("RunSlot");
            StartCoroutine(RunSlotsAsync(RotateSlots));
        }

        public void RunSlotEndLess()
        {
			StopAllCoroutines();
			
            Debug.Log("RunSlotEndLess");
            StartCoroutine(RunSlotsAsync(RotateSlotsEndless));
        }

        List<JackPot> jackPotWin;
        long jackPotsWinCoins;
        bool bigWin;

        private IEnumerator RunSlotsAsync(Action <Action> rotAction)
        {
            StartSpinEvent?.Invoke();
            //Debug.Log("StartSpinEvent?.Invoke() : " + spinCount);
            jackPotsWinCoins = 0;

            ReelsSpin = true;
            if (controls.Auto && !IsFreeSpin) controls.IncAutoSpinsCounter();
            Debug.Log("Spins count from game start: " + (++spinCount));

            MPlayer.SetWinCoinsCount(0);

            //1 ---------------start preparation-------------------------------
            winController.HideAllLines();

            //1a ------------- sound -----------------------------------------
            MSound.StopAllClip(false); // stop all clips without background musik
            MSound.PlayClip(0f, true, spinSound);

            //2 --------start rotating ----------------------------------------
            bool fullRotated = false;
            rotAction(() => { MSound.StopClips(spinSound); fullRotated = true; });
            while (!fullRotated) yield return wfs0_2;  // wait 
            yield return new WaitForSeconds(0.2f);
            EndSpinEvent?.Invoke();
            //Debug.Log("EndSpinEvent?.Invoke(): " + spinCount);


            //3 --------check result-------------------------------------------
            BeginWinCalcEvent?.Invoke();
            winController.SearchWinSymbols();

            bool hasLineWin = false;
            bool hasScatterWin = false;
            bigWin = false;
            jackPotWin = new List<JackPot>();


            if (winController.HasAnyWinn(ref hasLineWin, ref hasScatterWin, ref jackPotWin))
            {
				//Debug.Log("has win: " + spinCount);
                AnyWinEvent?.Invoke();
                double jpCoins = 0;
                foreach (var item in jackPotWin)
                {
                    if (item.Amount > 0)
                    {
                        jpCoins += item.Amount;
                    }
                }

                int lCoins = winController.GetLineWinCoins();
                int sCoins = winController.GetScatterWinCoins();
                int payMultiplier = winController.GetPayMultiplier();
                lCoins *= payMultiplier;
                sCoins *= payMultiplier;
                if (useLineBetMultiplier) 
                {
                    lCoins *= controls.LineBet;
                    sCoins *= controls.LineBet; 
                }
                double summCoins = jpCoins + lCoins + sCoins;
                if (summCoins > 0) WinCoinsEvent?.Invoke(summCoins);


                int winSpins = winController.GetWinSpins();
                int freeSpinsMultiplier = winController.GetFreeSpinsMultiplier();
                winSpins *= freeSpinsMultiplier;
                int winLinesCount = winController.GetWinLinesCount();
                if (useLineBetFreeSpinMultiplier) winSpins *= controls.LineBet;
                if (winSpins > 0) FreeSpinWinEvent?.Invoke(winSpins);

                SlotStatistic.Add(new StatisticData(controls.TotalBet, summCoins, IsFreeSpin, winController.GetWinLines(), winController.scatterWin, jackPotWin));

                EndWinCalcEvent?.Invoke(true); // go to win state
				//Debug.Log("end win calc: " + spinCount);
            }
            else // lose
            {
				//Debug.Log("loose : " + spinCount);
                SlotStatistic.Add(new StatisticData(controls.TotalBet, IsFreeSpin));
                EndWinCalcEvent?.Invoke(false); // go to loose state
				//Debug.Log("end win calc : " + spinCount);
            }
        }

        private IEnumerator WinShowC(Action completeCallBack)
        {
			//Debug.Log("win show start: " + spinCount);
            //3a ------ any win show event -------
            AnyWinShowEvent?.Invoke();
            yield return new WaitWhile(() => SlotEvents.Instance && SlotEvents.Instance.MiniGameStarted);
            yield return new WaitWhile(() => !MGUI.HasNoPopUp);

            //3b0 ---- show particles, line flasing  -----------
            winController.WinEffectsShow(winLineFlashing == WinLineFlashing.All, winSymbolParticles);

            //3b1 --------- check Jack pot -------------
            foreach (var item in jackPotWin)
            {
                yield return new WaitWhile(() => !MGUI.HasNoPopUp);

                if (item.Amount > 0)
                {
                    jackPotsWinCoins += item.Amount;
                    MPlayer.SetWinCoinsCount(jackPotsWinCoins);
                    MPlayer.AddCoins(item.Amount);

                    if (controls.HasFreeSpin || controls.Auto)
                    {
                        item.WinShow();
                        yield return new WaitForSeconds(5.0f); // delay
                        item.WinCancel();
                    }
                    else
                    {
                        item.WinShow();
                        yield return new WaitForSeconds(3.0f);// delay
                    }
                    item.SetCount(0); // reset jack pot amount
                }
            }

            yield return new WaitWhile(() => SlotEvents.Instance && SlotEvents.Instance.MiniGameStarted);
            yield return new WaitWhile(() => !MGUI.HasNoPopUp);

            //3c0 -----------------calc coins -------------------
            int winCoins = winController.GetWinCoins();
            int payMultiplier = winController.GetPayMultiplier();
            winCoins *= payMultiplier;
            if (useLineBetMultiplier) winCoins *= controls.LineBet;
            MPlayer.SetWinCoinsCount(jackPotsWinCoins + winCoins);
            MPlayer.AddCoins(winCoins);
            yield return new WaitWhile(() => SlotEvents.Instance && SlotEvents.Instance.MiniGameStarted);
            yield return new WaitWhile(() => !MGUI.HasNoPopUp);

            //3c1 ----------- calc free spins ----------------
            int winSpins = winController.GetWinSpins();
            int freeSpinsMultiplier = winController.GetFreeSpinsMultiplier();
            winSpins *= freeSpinsMultiplier;
            int winLinesCount = winController.GetWinLinesCount();
            if (useLineBetFreeSpinMultiplier) winSpins *= controls.LineBet;

            if (winCoins > 0)
            {
                bigWin = (winCoins >= MPlayer.MinWin && MPlayer.UseBigWinCongratulation);
                if (!bigWin) MSound.PlayClip(0, winCoinsSound);
                else
                {
                    yield return new WaitWhile(() => !MGUI.HasNoPopUp); 
                    MGUI.ShowMessage(BigWinPrefab, " ", winCoins.ToString(), 3f, null);
                }
                yield return new WaitWhile(() => SlotEvents.Instance && SlotEvents.Instance.MiniGameStarted);
                yield return new WaitWhile(() => !MGUI.HasNoPopUp);
            }

            if (winSpins > 0) MSound.PlayClip((winCoins > 0 || jackPotsWinCoins > 0) ? 1.5f : 0, winFreeSpinSound);
            controls.AddFreeSpins(winSpins);
            yield return new WaitWhile(() => SlotEvents.Instance && SlotEvents.Instance.MiniGameStarted);
            yield return new WaitWhile(() => !MGUI.HasNoPopUp);

            playFreeSpins = (controls.AutoPlayFreeSpins && controls.HasFreeSpin);
            startFreeGames = !IsFreeSpin && playFreeSpins;
            endFreeGames = IsFreeSpin && !playFreeSpins;

            if (endFreeGames) EndFreeGamesEvent?.Invoke();
            yield return new WaitWhile(() => SlotEvents.Instance && SlotEvents.Instance.MiniGameStarted);
            yield return new WaitWhile(() => !MGUI.HasNoPopUp);


            //3d0 ----- invoke scatter win event -----------
            if (winController.scatterWin != null && winController.scatterWin.WinEvent != null) 
            {
                yield return new WaitForSeconds(1.0f);
                winController.scatterWin.WinEvent.Invoke();
                yield return new WaitWhile(() => SlotEvents.Instance && SlotEvents.Instance.MiniGameStarted);
                yield return new WaitWhile(() => !MGUI.HasNoPopUp);
            }

            // 3d1 -------- add levelprogress --------------
            MPlayer.AddLevelProgress((useLineBetProgressMultiplier) ? winSpinLevelProgress * winLinesCount * controls.LineBet : winSpinLevelProgress * winLinesCount); // for each win line
            yield return new WaitWhile(() => SlotEvents.Instance && SlotEvents.Instance.MiniGameStarted);
            yield return new WaitWhile(() => !MGUI.HasNoPopUp);

            // 3d2 ------------ start line events ----------
            winController.StartLineEvents();
            yield return new WaitWhile(() => SlotEvents.Instance && SlotEvents.Instance.MiniGameStarted);
            yield return new WaitWhile(() => !MGUI.HasNoPopUp);

            // start free games events
            if (startFreeGames) 
            {
                MGUI.ShowMessage(FreeGamesPUPrefab, "", winSpins.ToString(), 1000000, () => { });
                StartFreeGamesEvent?.Invoke();
                yield return new WaitWhile(() => SlotEvents.Instance && SlotEvents.Instance.MiniGameStarted);
                yield return new WaitWhile(() => MGUI.GetPopUpWindowByDescription(FreeGamesPUPrefab ? FreeGamesPUPrefab.description : "freespinswin")); // wait while window exist
            }

            //3e ---- ENABLE player interaction -----------
            ReelsSpin = false;
            MSound.PlayCurrentMusic();

            if (controls.Auto && controls.AutoSpinsCounter >= controls.AutoSpinCount)
            {
                controls.ResetAutoSpinsMode();
            }
            if (logStatistic) SlotStatistic.PrintStatistic();
            completeCallBack?.Invoke();
			//Debug.Log("win show end: " + spinCount);
        }

        public void WinShow(Action completeCallBack)
        {
            StartCoroutine(WinShowC(completeCallBack));
        }

        private IEnumerator FreeInputWinShowC(Action <bool> completeCallBack)
        {
            bool showEnd = false;
            winController.WinSymbolShow(winLineFlashing,
                   (windata) => //linewin
                       {
                           //event can be interrupted by player
                           if (windata != null) Debug.Log("lineWin : " + windata.ToString());
                   },
                   () => //scatter win
                       {
                           //event can be interrupted by player
                       },
                   () => //jack pot 
                       {
                           //event can be interrupted by player
                       },
                   () =>
                   {
                       showEnd = true;
                   }
                   );

            yield return new WaitWhile(()=>!showEnd);

            completeCallBack?.Invoke(controls.Auto || playFreeSpins);
        }

        public void FreeInputWinShow(Action <bool> completeCallBack)
        {
            StartCoroutine(FreeInputWinShowC(completeCallBack));
        }

        private IEnumerator LooseShowC(Action <bool> completeCallBack)
        {
            //Debug.Log("Lose showC: " + spinCount);
            MSound.PlayClip(0, looseSound);
            MPlayer.AddLevelProgress(loseSpinLevelProgress);
            playFreeSpins = (controls.AutoPlayFreeSpins && controls.HasFreeSpin);

            ReelsSpin = false;
            MSound.PlayCurrentMusic();

            yield return new WaitWhile(() => !MGUI.HasNoPopUp);  

            if (controls.Auto && controls.AutoSpinsCounter >= controls.AutoSpinCount)
            {
                controls.ResetAutoSpinsMode();
            }

            yield return new WaitForSeconds(1.0f);
            if (logStatistic) SlotStatistic.PrintStatistic();
            completeCallBack?.Invoke(controls.Auto || playFreeSpins);
			//Debug.Log("end Lose showC: " + spinCount);
        }

        public void LooseShow(Action <bool> completeCallBack)
        {
            StartCoroutine(LooseShowC(completeCallBack));
        }

        private List<int> randsCache;

        private void RotateSlots(Action rotCallBack)
        {
            ParallelTween pT = new ParallelTween();
            List<int> rands = new List<int>(rng.GetRandSymbols()); //next symbols for reel (bottom raycaster)
            randsCache = new List<int>(rands);

            //hold feature
            HoldFeature hold = controls.Hold;
            bool[] holdReels = null;
            if (controls.UseHold && hold && hold.Length == rands.Count)
            {
                holdReels = hold.GetHoldReels();
                for (int i = 0; i < rands.Count; i++)
                {
                    rands[i] = (holdReels[i]) ? slotGroupsBeh[i].CurrOrderPosition : rands[i]; // hold position
                }
            }

            #region prediction visible symbols on reels
            if (debugPredictSymbols)
            for (int i = 0; i < rands.Count; i++)
            {
                    Debug.Log("------- Reel: " + i +" ------- (down up)");
                    for (int r = 0; r < slotGroupsBeh[i].RayCasters.Length; r++)
                    {
                        int sO = (int)Mathf.Repeat(rands[i] + r, slotGroupsBeh[i].symbOrder.Count);
                        int sID =  slotGroupsBeh[i].symbOrder[sO]; 
                        string sName = slotIcons[sID].iconSprite.name; 
                        Debug.Log("NextSymb ID: " + sID + " ;name : " + sName);
                    }
            }
            #endregion prediction

            for (int i = 0; i < slotGroupsBeh.Length; i++)
            {
                int n = i;
                int r = rands[i];

                if (holdReels == null || (holdReels != null && !holdReels[i]))
                {
                    pT.Add((callBack) =>
                    {
                        slotGroupsBeh[n].NextRotateCylinderEase(mainRotateType, inRotType, outRotType,
                            mainRotateTime, mainRotateTimeRandomize / 100f,
                            inRotTime, outRotTime, inRotAngle, outRotAngle,
                            r, callBack);
                    });
                }
            }

            pT.Start(rotCallBack);
        }

        private void RotateSlotsEndless(Action rotCallBack)
        {
            ParallelTween pT = new ParallelTween();
            List<int> rands = new List<int>(rng.GetRandSymbols()); //next symbols for reel (bottom raycaster)
            randsCache = new List<int>(rands);
            List<int> randsEL = new List<int>(rands);

            //hold feature
            HoldFeature hold = controls.Hold;
            bool[] holdReels = null;
            if (controls.UseHold && hold && hold.Length == rands.Count)
            {
                holdReels = hold.GetHoldReels();
                for (int i = 0; i < rands.Count; i++)
                {
                    rands[i] = (holdReels[i]) ? slotGroupsBeh[i].CurrOrderPosition : randsCache[i]; // hold position
                    randsEL[i] = (holdReels[i]) ? slotGroupsBeh[i].CurrOrderPosition : -1;
                }
            }
            else
            {
                for (int i = 0; i < rands.Count; i++)
                    randsEL[i] =  -1;
            }

            #region prediction visible symbols on reels
            if (debugPredictSymbols)
                for (int i = 0; i < rands.Count; i++)
                {
                    Debug.Log("------- Reel: " + i + " ------- (down up)");
                    for (int r = 0; r < slotGroupsBeh[i].RayCasters.Length; r++)
                    {
                        int sO = (int)Mathf.Repeat(rands[i] + r, slotGroupsBeh[i].symbOrder.Count);
                        int sID = slotGroupsBeh[i].symbOrder[sO];
                        string sName = slotIcons[sID].iconSprite.name;
                        Debug.Log("NextSymb ID: " + sID + " ;name : " + sName);
                    }
                }
            #endregion prediction



            for (int i = 0; i < slotGroupsBeh.Length; i++)
            {
                int n = i;
                int r = randsEL[i];

                if (holdReels == null || (holdReels != null && !holdReels[i]))
                {
                    pT.Add((callBack) =>
                    {
                        slotGroupsBeh[n].NextRotateCylinderEase(mainRotateType, inRotType, outRotType,
                            mainRotateTime, mainRotateTimeRandomize / 100f,
                            inRotTime, outRotTime, inRotAngle, outRotAngle,
                            r, callBack);
                    });
                }
            }

            pT.Start(rotCallBack);
        }


        /// <summary>
        /// Calculate propabilities
        /// </summary>
        /// <returns></returns>
        public string[,] CreatePropabilityTable()
        {
            List<string> rowList = new List<string>();
            string[] iconNames = GetIconNames(false);
            int length = slotGroupsBeh.Length;
            string[,] table = new string[length + 1, iconNames.Length + 1];

            rowList.Add("reel / icon");
            rowList.AddRange(iconNames);
            SetRow(table, rowList, 0, 0);

            for (int i = 1; i <= length; i++)
            {
                table[i, 0] = "reel #" + i.ToString();
                SetRow(table, new List<float>(slotGroupsBeh[i - 1].GetReelSymbHitPropabilities(slotIcons)), 1, i);
            }
            return table;
        }

        /// <summary>
        /// Calculate propabilities
        /// </summary>
        /// <returns></returns>
        public string[,] CreatePayTable(out float sumPayOut, out float sumPayoutFreeSpins)
        {
            List<string> row = new List<string>();
            List<float[]> reelSymbHitPropabilities = new List<float[]>();
            string[] iconNames = GetIconNames(false);

            sumPayOut = 0;
            CreateFullPaytable();
            int rCount = payTableFull.Count + 1;
            int cCount = slotGroupsBeh.Length + 3;
            string[,] table = new string[rCount, cCount];
            row.Add("PayLine / reel");
            for (int i = 0; i < slotGroupsBeh.Length; i++)
            {
                row.Add("reel #" + (i + 1).ToString());
            }
            row.Add("Payout");
            row.Add("Payout, %");
            SetRow(table, row, 0, 0);

            PayLine pL;
            List<PayLine> freeSpinsPL = new List<PayLine>();  // paylines with free spins

            for (int i = 0; i < payTableFull.Count; i++) 
            {
                pL = payTableFull[i];
                table[i + 1, 0] = "Payline #" + (i + 1).ToString();
                table[i + 1, cCount - 2] = pL.pay.ToString();
                float pOut = pL.GetPayOutProb(this);
                sumPayOut += pOut;
                table[i + 1, cCount - 1] = pOut.ToString("F6");
                SetRow(table, new List<string>(pL.Names(slotIcons, slotGroupsBeh.Length)), 1, i + 1);
                if (pL.freeSpins > 0) freeSpinsPL.Add(pL);
            }

            Debug.Log("sum (without free spins) % = " + sumPayOut);

            sumPayoutFreeSpins = sumPayOut;
            foreach (var item in freeSpinsPL)
            {
                sumPayoutFreeSpins += sumPayOut * item.GetProbability(this) * item.freeSpins;
            }

            Debug.Log("sum (with free spins) % = " + sumPayoutFreeSpins);
            Debug.Log("sum (with expanding wild) % = " + sumPayoutFreeSpins);
            return table;
        }

        private void SetRow<T>(string[,] table, List<T> row, int beginColumn, int rowNumber)
        {
            if (rowNumber >= table.GetLongLength(0)) return;

            for (int i = 0; i < row.Count; i++)
            {
               // Debug.Log("sr"+i);
                if (i + beginColumn < table.GetLongLength(1)) table[rowNumber, i + beginColumn] = row[i].ToString();
            }
        }

        public string[] GetIconNames(bool addAny)
        {
            if (slotIcons == null || slotIcons.Length == 0) return null;
            int length = (addAny) ? slotIcons.Length + 1 : slotIcons.Length;
            string[] sName = new string[length];
            if (addAny) sName[0] = "any";
            int addN = (addAny) ? 1 : 0;
            for (int i = addN; i < length; i++)
            {
                if (slotIcons[i - addN] != null && slotIcons[i - addN].iconSprite != null)
                {
                    sName[i] = slotIcons[i - addN].iconSprite.name;
                }
                else
                {
                    sName[i] = (i - addN).ToString();
                }
            }
            return sName;
        }

        internal WinSymbolBehavior GetWinPrefab(string tag)
        {
            if (winSymbolBehaviors == null || winSymbolBehaviors.Length == 0) return null;
            foreach (var item in winSymbolBehaviors)
            {
                if (item.WinTag.Contains(tag))
                {
                    return item;
                }
            }
            return null;
        }

        private void CreateFullPaytable()
        {
            payTableFull = new List<PayLine>();
            for (int j = 0; j < payTable.Count; j++)
            {
                payTable[j].ClampLine(slotGroupsBeh.Length);
                payTableFull.Add(payTable[j]);
                if (useWild) payTableFull.AddRange(payTable[j].GetWildLines(this));
            }
        }

        #region calculate
        public void CreatTripleCombos()
        {
            Measure("triples time", () => {
                List<List<int>> triplesComboNumbers;  //0 0 0 0 0; 0 0 0 0 1 .... 24 24 24 24 24
                triplesComboNumbers = new List<List<int>>();
                ComboCounterT cct = new ComboCounterT(slotGroupsBeh);
                List<int> combo = cct.combo;
                //Debug.Log(combo[0] + " : " + combo[1] + " : " + combo[2] + " : " + combo[3] + " : " + combo[4]);
                triplesComboNumbers.Add(new List<int>(combo));

                int i = 0;
                while (cct.NextCombo())
                {
                    combo = cct.combo;
                    //if(i<100)  Debug.Log(combo[0] + " : " + combo[1] + " : " + combo[2] + " : " + combo[3] + " : " + combo[4]);
                    triplesComboNumbers.Add(new List<int>(combo));
                    i++;
                }
                tripleCombos = new List<List<Triple>>();
                Debug.Log(triplesComboNumbers.Count);
                List<Triple> trList;
                Triple tr = null;
                foreach (var item in triplesComboNumbers)
                {
                    trList = new List<Triple>();
                    for (int t = 0; t < item.Count; t++)
                    {
                        tr = slotGroupsBeh[t].triples[item[t]];
                        trList.Add(tr);
                    }
                    tripleCombos.Add(trList);
                }

                Debug.Log("tripleCombos.Count " + tripleCombos.Count);
            });
            // Debug.Log(tr.ToString());
        }

        /// <summary>
        /// Calc win for triple
        /// </summary>
        /// <param name="trList"></param>
        public void TestWin()
        {
            Measure("test time", () =>
            {
                double sumPayOUt = 0;
                int sumFreeSpins = 0;
                double sumBets = 0;
                LineBehavior[] lbs = FindObjectsOfType<LineBehavior>();
                //Debug.Log("lines count: " + lbs.Length);
                int linesCount = lbs.Length;
                int i = 0;
                int wins = 0;
                double totalBet = linesCount * linebet;
                Debug.Log("totalBet: " + totalBet);
                for (int w = 0; w < 10000; w++)
                {
                    int r = UnityEngine.Random.Range(0, tripleCombos.Count);
                    var item = tripleCombos[r];
                    if (sumFreeSpins > 0) { sumFreeSpins--; }
                    else
                    {
                        sumBets += (totalBet);
                    }
                    int freeSpins = 0;
                    int pay = 0;
                    int payMult = 1;

                    int freeSpinsScat = 0;
                    int payScat = 0;
                    int payMultScat = 1;

                    CalcWin(item, lbs, ref freeSpins, ref pay, ref payMult, ref freeSpinsScat, ref payScat, ref payMultScat);
                    sumPayOUt += ((double)pay * linebet);
                    sumPayOUt += ((double)payScat * totalBet);
                    sumFreeSpins += freeSpins;
                    if (pay > 0 || payScat > 0 || freeSpins > 0) wins++;
                    i++;
                }


                //foreach (var item in tripleCombos)
                //{
                //    i++;
                //    if (sumFreeSpins > 0) {  sumFreeSpins--; }
                //   else {
                //        sumBets += (linesCount);
                //        }
                //    int freeSpins = 0;
                //    int pay = 0;
                //    int payMult = 1;
                //    CalcWin(item,lbs, ref freeSpins, ref pay, ref payMult);
                //    sumPayOUt += pay;
                //    sumFreeSpins += freeSpins;
                //    if (i > 1000000) break;
                //    if (pay > 0) wins++;
                //}
                Debug.Log("calcs: " + i + " ;payout: " + sumPayOUt + " ; sumBets: " + sumBets + "; wins: " + wins + " ;pOUt,%" + ((float)sumPayOUt / (float)sumBets * 100f));
            });
        }

        private double linebet = 0.004;
        /// <summary>
        /// Calc win for triple
        /// </summary>
        /// <param name="trList"></param>
        public void CalcWin()
        {
            Measure("calc time", () =>
            {
                LineBehavior[] lbs = FindObjectsOfType<LineBehavior>();
                winController.InitCalc();
                Debug.Log("lines count: " + lbs.Length);
                int linesCount = lbs.Length;
                int i = 0;
                int wins = 0;
                double pOut = 0;
                double comboProb = (1f / (double)tripleCombos.Count) / (double)linesCount;
                double comboProbScat = (1f / (double)tripleCombos.Count);
                int length = tripleCombos.Count;
                for (i = 0; i < length; i++)
                {
                    var item = tripleCombos[i];
                    int freeSpins = 0;
                    int pay = 0;
                    int payMult = 1;

                    int freeSpinsScat = 0;
                    int payScat = 0;
                    int payMultScat = 1;

                    CalcWin(item, lbs, ref freeSpins, ref pay, ref payMult, ref freeSpinsScat, ref payScat, ref payMultScat);
                    payMult *= payMultScat;
                    pay *= payMult;

                    pOut += ((double)pay * comboProb + (double)payScat * comboProbScat);

                    if (pay > 0 || payScat > 0 || freeSpins > 0)
                    {
                        wins++;
                        //  Debug.Log(pay + " : " + (pay * comboProb));
                    }
                }

                Debug.Log("calcs: " + i + " ; wins: " + wins + " ;payout %: " + (pOut * 100f));
            });
        }

        /// <summary>
        /// Calc win for triple
        /// </summary>
        /// <param name="trList"></param>
        public void CalcWin(List<Triple> trList, LineBehavior[] lbs, ref int freeSpins, ref int pay, ref int payMult, ref int freeSpinsScat, ref int payScat, ref int payMultScat)
        {
            SetTriples(trList);
            winController.SearchWinCalc();
            freeSpins = winController.GetLineWinSpinsCalc();
            pay = winController.GetLineWinCoinsCalc();
            payMult = winController.GetLinePayMultiplierCalc();

            freeSpinsScat = winController.GetScatterWinSpinsCalc();
            payScat = winController.GetScatterWinCoinsCalc();
            payMultScat = winController.GetScatterPayMultiplierCalc();
        }

        public void SetTriples(List<Triple> trList)
        {
            RayCaster[] rs;
            for (int i = 0; i < slotGroupsBeh.Length; i++)
            {
                rs = slotGroupsBeh[i].RayCasters;
                rs[0].ID = trList[i].ordering[2];
                rs[1].ID = trList[i].ordering[1];
                rs[2].ID = trList[i].ordering[0];
            }
        }

        public static void Measure(string message, Action measProc)
        {
            System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();//https://msdn.microsoft.com/ru-ru/library/system.diagnostics.stopwatch%28v=vs.110%29.aspx
            stopWatch.Start();
            if (measProc != null) { measProc(); }
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
            UnityEngine.Debug.Log(message + "- elapsed time: " + elapsedTime);
        }
        #endregion calculate

        #region dev
        public void RebuildLines()
        {
            foreach (var item in payTable)
            {
                int[] line = new int[slotGroupsBeh.Length];

                for (int i = 0; i < line.Length; i++)
                {
                    if (i < item.line.Length) line[i] = item.line[i];
                    else line[i] = -1;
                }
                item.line = line;
            }
        }

        public string PaytableToJsonString()
        {
            string res = "";
            ListWrapper<PayLine> lW = new ListWrapper<PayLine>(payTable);
            res = JsonUtility.ToJson(lW);
            return res;
        }

        public void SetPayTableFromJson()
        {
            Debug.Log("Json viewer - " + "http://jsonviewer.stack.hu/");
            Debug.Log("old paytable json: "  + PaytableToJsonString());

            if(string.IsNullOrEmpty(payTableJsonString))
            {
                Debug.Log("payTableJsonString : empty");
                return;
            }

            ListWrapper<PayLine> lWPB = JsonUtility.FromJson<ListWrapper<PayLine>>(payTableJsonString);
            if(lWPB!=null && lWPB.list!=null && lWPB.list.Count > 0)
            {
                payTable = lWPB.list;
            }
        }
        #endregion dev

        #region test
        public void Test(int testSpinsCount)
        {
            controls.SetAutoSpinsCount(testSpinsCount);
            controls.AutoSpinButton.ClickEvent?.Invoke();
            Debug.Log("Start Test"+"Ballance: " + MPlayer.Coins + " ;bet: " + controls.TotalBet);
            Time.timeScale = 100;
        }
        #endregion test
    }

    [Serializable]
    public class ReelData
    {
        public List<int> symbOrder;
        public ReelData(List<int> symbOrder)
        {
            this.symbOrder = symbOrder;
        }
        public int Length
        {
            get { return (symbOrder == null) ? 0 : symbOrder.Count; }
        }
        public int GetSymbolAtPos(int position)
        {
            return (symbOrder == null || position >= symbOrder.Count) ? 0 : symbOrder.Count;
        }
    }

    [Serializable]
    //Helper class for creating pay table
    public class PayTable
    {
        public int reelsCount;
        public List<PayLine> payLines;
        public void Rebuild()
        {
            if (payLines != null)
            {
                foreach (var item in payLines)
                {
                    if (item != null)
                    {
                        item.RebuildLine();
                    }
                }
            }
        }
    }

    [Serializable]
    public class PayLine
    {
        private const int maxLength = 5;
        public int[] line;
        public int pay;
        public int freeSpins;
        public bool showEvent = false;
        public UnityEvent LineEvent;
        [Tooltip("Payouts multiplier, default value = 1")]
        public int payMult = 1; // payout multiplier
        [Tooltip("Free Spins multiplier, default value = 1")]
        public int freeSpinsMult = 1; // payout multiplier

        bool useWildInFirstPosition = false;

        public PayLine()
        {
            line = new int[maxLength];
            for (int i = 0; i < line.Length; i++)
            {
                line[i] = -1;
            }
        }

        public PayLine(PayLine pLine)
        {
            if (pLine.line != null)
            {
                line = pLine.line;
                RebuildLine();
                pay = pLine.pay;
                freeSpins = pLine.freeSpins;
                LineEvent = pLine.LineEvent;
                payMult = pLine.payMult;
            }
            else
            {
                RebuildLine();
            }
        }

        public PayLine(int[] newLine, int pay, int freeSpins)
        {
            if (newLine != null)
            {
                this.line = newLine;
                this.pay = pay;
                this.freeSpins = freeSpins;
            }
            RebuildLine();
        }

        public string ToString(Sprite[] sprites, int length)
        {
            string res = "";
            if (line == null) return res;
            for (int i = 0; i < line.Length; i++)
            {
                if (i < length)
                {
                    if (line[i] >= 0)
                        res += sprites[line[i]].name;
                    else
                    {
                        res += "any";
                    }
                    if (i < line.Length - 1) res += ";";
                }
            }
            return res;
        }

        public string[] Names(SlotIcon[] sprites, int length)
        {
            if (line == null) return null;
            List<string> res = new List<string>();
            for (int i = 0; i < line.Length; i++)
            {
                if (i < length)
                {
                    if (line[i] >= 0)
                        res.Add((sprites[line[i]] != null && sprites[line[i]].iconSprite != null) ? sprites[line[i]].iconSprite.name : "failed");
                    else
                    {
                        res.Add("any");
                    }
                }
            }
            return res.ToArray();
        }

        public float GetPayOutProb(SlotController sC)
        {
            return GetProbability(sC) * 100f * pay;
        }

        public float GetProbability(SlotController sC)
        {
            float res = 0;
            if (!sC) return res;
            if (line == null || sC.slotGroupsBeh == null || sC.slotGroupsBeh.Length > line.Length) return res;
            float[] rP = sC.slotGroupsBeh[0].GetReelSymbHitPropabilities(sC.slotIcons);

            //avoid "any" symbol error in first position
            res = (line[0] >= 0) ? rP[line[0]] : 1; //  res = rP[line[0]];

            for (int i = 1; i < sC.slotGroupsBeh.Length; i++)
            {
                if (line[i] >= 0) // any.ID = -1
                {
                    rP = sC.slotGroupsBeh[i].GetReelSymbHitPropabilities(sC.slotIcons);
                    res *= rP[line[i]];
                }
                else
                {
                    // break;
                }
            }
            return res;
        }

        /// <summary>
        /// Create and return additional lines for this line with wild symbol,  only if symbol can be substitute with wild
        /// </summary>
        /// <returns></returns>
        public List<PayLine> GetWildLines(SlotController sC)
        {
            int workLength = sC.slotGroupsBeh.Length;
            List<PayLine> res = new List<PayLine>();
            if (!sC) return res; // return empty list
            if (!sC.useWild) return res; // return empty list

            int wild_id = sC.wild_id;
            useWildInFirstPosition = sC.useWildInFirstPosition;
            List<int> wPoss = GetPositionsForWild(wild_id, sC);
            int maxWildsCount = (useWildInFirstPosition) ? wPoss.Count - 1 : wPoss.Count;
            int minWildsCount = 1;
            ComboCounter cC = new ComboCounter(wPoss);
            while (cC.NextCombo())
            {
                List<int> combo = cC.combo; // 
                int comboSum = combo.Sum(); // count of wilds in combo

                if (comboSum >= minWildsCount && comboSum <= maxWildsCount)
                {
                    PayLine p = new PayLine(this);
                    for (int i = 0; i < wPoss.Count; i++)
                    {
                        int pos = wPoss[i];
                        if (combo[i] == 1)
                        {
                            p.line[pos] = wild_id;
                        }
                    }
                    if (!p.IsEqual(this, workLength) && !ContainEqualLine(res, p, workLength)) res.Add(p);
                }
            }

            return res;
        }

        private bool IsEqual(PayLine pLine, int workLength)
        {
            if (pLine == null) return false;
            if (pLine.line == null) return false;
            if (line.Length != pLine.line.Length) return false;
            for (int i = 0; i < line.Length; i++)
            {
                if (line[i] != pLine.line[i]) return false;
            }
            return true;
        }

        private bool ContainEqualLine(List<PayLine> pList, PayLine pLine, int workLength)
        {
            if (pList == null) return false;
            if (pLine == null) return false;
            if (pLine.line == null) return false;

            foreach (var item in pList)
            {
                if (item.IsEqual(pLine, workLength)) return true;
            }
            return false;
        }

        /// <summary>
        /// return list position on line for wild symbols (0 - line.length -1)  
        /// </summary>
        /// <param name="wild_id"></param>
        /// <param name="sC"></param>
        /// <returns></returns>
        private List<int> GetPositionsForWild(int wild_id, SlotController sC)
        {
            List<int> wPoss = new List<int>();
            int counter = 0;
            int length = sC.slotGroupsBeh.Length;

            for (int i = 0; i < line.Length; i++)
            {
                if (i < length)
                {
                    if (line[i] != -1 && line[i] != wild_id)
                    {
                        if (!useWildInFirstPosition && counter == 0) // don't use first
                        {
                            counter++;
                        }
                        else
                        {
                            if (sC.slotIcons[line[i]].useWildSubstitute) wPoss.Add(i);
                            counter++;
                        }
                    }
                }
            }
            return wPoss;
        }

        public void RebuildLine()
        {
          // if (line.Length == maxLength) return;
            int[] lineT = new int[maxLength];
            for (int i = 0; i < maxLength; i++)
            {
                if (line != null && i < line.Length) lineT[i] = line[i];
                else lineT[i] = -1;
            }
            line = lineT;
        }

        public void ClampLine(int workLength)
        {
            RebuildLine();
            for (int i = 0; i < maxLength; i++)
            {
                if (i >= workLength) line[i] = -1;
            }
        }
    }

    [Serializable]
    public class ScatterPay
    {
        public int scattersCount;
        public int pay;
        public int freeSpins;
        public int payMult = 1;
        public int freeSpinsMult = 1;
        public UnityEvent WinEvent;

        public ScatterPay()
        {
            payMult = 1;
            freeSpinsMult = 1;
            scattersCount = 3;
            pay = 0;
            freeSpins = 0;
        }
    }

    static class ClassExt
    {
        public enum FieldAllign { Left, Right, Center}

        /// <summary>
        /// Return formatted string; (F2, N5, e, r, p, X, D12, C)
        /// </summary>
        /// <param name="fNumber"></param>
        /// <param name="format"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        public static string ToString(this float fNumber, string format, int field)
        {
            string form = "{0," + field.ToString() +":"+ format + "}";
            string res = String.Format(form, fNumber);
            return res;
        }

        /// <summary>
        /// Return formatted string; (F2, N5, e, r, p, X, D12, C)
        /// </summary>
        /// <param name="fNumber"></param>
        /// <param name="format"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        public static string ToString(this string s, int field)
        {
            string form = "{0," + field.ToString() +"}";
            string res = String.Format(form, s);
            return res;
        }

        /// <summary>
        /// Return formatted string; (F2, N5, e, r, p, X, D12, C)
        /// </summary>
        /// <param name="fNumber"></param>
        /// <param name="format"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        public static string ToString(this string s, int field, FieldAllign fAllign)
        {
            int length = s.Length;
            if (length >= field)
            {
                string form = "{0," + field.ToString() + "}";
                return String.Format(form, s);
            }
            else
            {
                if (fAllign == FieldAllign.Center)
                {
                    int lCount = (field - length) / 2;
                    int rCount = field - length - lCount;
                    string lSp = new string('*', lCount);
                    string rSp = new string('*', rCount);
                    return (lSp + s + rSp);
                }
                else if (fAllign == FieldAllign.Left)
                {
                    int lCount = (field - length);
                    string lSp = new string('*', lCount);
                    return (s+lSp);
                }
                else
                {
                    string form = "{0," + field.ToString() + "}";
                    return  String.Format(form, s);
                }
            }
        }

        private static string ToStrings<T>(T[] a)
        {
            string res = "";
            for (int i = 0; i < a.Length; i++)
            {
                res += a[i].ToString();
                res += " ";
            }
            return res;
        }

        private static string ToStrings(float[] a, string format, int field)
        {
            string res = "";
            for (int i = 0; i < a.Length; i++)
            {
                res += a[i].ToString(format, field);
                res += " ";
            }
            return res;
        }

        private static string ToStrings(string[] a, int field, ClassExt.FieldAllign allign)
        {
            string res = "";
            for (int i = 0; i < a.Length; i++)
            {
                res += a[i].ToString(field, allign);
                res += " ";
            }
            return res;
        }

        private static float[] Mul(float[] a, float[] b)
        {
            if (a.Length != b.Length) return null;
            float[] res = new float[a.Length];
            for (int i = 0; i < res.Length; i++)
            {
                res[i] = a[i] * b[i];
            }
            return res;
        }
    }

    /// <summary>
    /// Helper class to make combinations from symbols with wild
    /// </summary>
    public class ComboCounter
    {
        public List<int> combo;
        public List<int> positions;

        List<byte> counterSizes;

        public ComboCounter(List<int> positions)
        {
            this.positions = positions;
            counterSizes = GetComboCountsForSymbols();
            combo = new List<int>(counterSizes.Count);

            for (int i = 0; i < counterSizes.Count; i++) // create in counter first combination
            {
                combo.Add(0);
            }
        }

        /// <summary>
        /// get list with counts of combinations for each position
        /// </summary>
        /// <returns></returns>
        private List<byte> GetComboCountsForSymbols()
        {
            List<byte> res = new List<byte>();
            foreach (var item in positions)
            {
                res.Add((byte)(1)); // wild or symbol (0 or 1)
            }
            return res;
        }

        private bool Next()
        {
            for (int i = counterSizes.Count - 1; i >= 0; i--)
            {
                if (combo[i] < counterSizes[i])
                {
                    combo[i]++;
                    if (i != counterSizes.Count - 1) // reset low "bytes"
                    {
                        for (int j = i + 1; j < counterSizes.Count; j++)
                        {
                            combo[j] = 0;
                        }
                    }
                    return true;
                }
            }
            return false;
        }

        public bool NextCombo()
        {
            if (Next())
            {
                return true;
            }
            return false;
        }
    }

    public class ComboCounterT
    {
        public List<int> combo; // combination with 5 numbers
        public List<int> positions;

        List<byte> counterSizes;

        public ComboCounterT(SlotGroupBehavior[] sgb)
        {
            counterSizes = new List<byte>();
            for (int i = 0; i < sgb.Length; i++)
            {
                counterSizes.Add((byte)sgb[i].triples.Count);
            }

            combo = new List<int>(counterSizes.Count);

            for (int i = 0; i < counterSizes.Count; i++) // create in counter first combination
            {
                combo.Add(0);
            }
        }

        private bool Next()
        {
            for (int i = counterSizes.Count - 1; i >= 0; i--)
            {
                if (combo[i] < counterSizes[i] - 1)
                {
                    combo[i]++;
                    if (i != counterSizes.Count - 1) // reset low "bytes"
                    {
                        for (int j = i + 1; j < counterSizes.Count; j++)
                        {
                            combo[j] = 0;
                        }
                    }
                    return true;
                }
            }
            return false;
        }

        public bool NextCombo()
        {
            if (Next())
            {
                return true;
            }
            return false;
        }
    }


}

