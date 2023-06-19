using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mkey
{
    public class WinController : MonoBehaviour
    {
        [SerializeField]
        private LinesController linesController;
        [Tooltip("Win prefab tag")]
        [SerializeField]
        private string winTag = "spritescale";
        [Tooltip("Time in sec for each winning line to show winsymbols")]
        [SerializeField]
        private float lineWinShowTime = 5f;

        public WinData scatterWin { get; private set; }

        #region temp vars
        private List<PayLine> payTable;
        private List<ScatterPay> scatterPayTable;
        private List<SlotSymbol> scatterWinSymbols;
        private SlotGroupBehavior[] slotGroupsBeh;
        private int scatter_id;
        private bool useScatter;
        private GameObject particlesPrefab;
        private Transform topJumpTarget;
        private Transform bottomJumpTarget;
        private SlotController slot;
        private int contID;
        private TweenSeq contTS;
        private TweenSeq onceTS;
        private SlotControls controls;
        private WinLineFlashing winLineFlashing;
        #endregion temp vars

        #region regular 
        void Start()
        {
            InitCalc();
        }

        private void OnDestroy()
        {
            WinShowCancel();
        }

        private void OnValidate()
        {
            lineWinShowTime = (lineWinShowTime < 3) ? 3 : lineWinShowTime;
        }

        internal void InitCalc()
        {
            slot = GetComponent<SlotController>();
            controls = slot.controls;
            payTable = slot.payTableFull;
            slotGroupsBeh = slot.slotGroupsBeh;
            scatter_id = slot.scatter_id;
            useScatter = slot.useScatter;
            particlesPrefab = slot.particlesStars;
            topJumpTarget = slot.topJumpTarget;
            bottomJumpTarget = slot.bottomJumpTarget;
            scatterPayTable = slot.scatterPayTable;
        }
        #endregion regular 

        #region win animation
        /// <summary>
        /// Show symbols particles and lines glowing
        /// </summary>
        internal void WinEffectsShow(bool flashingLines, bool showSymbolParticles)
        {
            HideAllLines();

            foreach (var lb in linesController.Lines)
            {
                //if (lb.IsWinningLine)
                //{
                //    lb.SetLineVisible(flashingLines);
                //    lb.LineFlashing(flashingLines);
                //}
                lb.ShowWinSymbolsParticles(showSymbolParticles);
            }

            if (useScatter && scatterWinSymbols != null && scatterWinSymbols.Count > 0)
            {
                foreach (var item in scatterWinSymbols)
                {
                    item.ShowParticles(showSymbolParticles, slot.particlesStars);
                }
            }
            
            controls.jackPots.ForEach((jp) => { jp.ShowWinSymbolsParticles(showSymbolParticles); });
        }

        /// <summary>
        /// Show win symbols 
        /// </summary>
        internal void WinSymbolShow(WinLineFlashing flashLine, Action<WinData> lineWinCallBack, Action scatterWinCallBack, Action jackPotWinCallBack, Action completeCallBack)
        {
            winLineFlashing = flashLine;
            WinSymbolShowContinuous(lineWinCallBack, scatterWinCallBack, jackPotWinCallBack, completeCallBack);
        }

        /// <summary>
        /// Hide selected lines
        /// </summary>
        public void HideAllLines()
        {
            linesController.HideAllLines();
        }

        /// <summary>
        /// Reset win data
        /// </summary>
        internal void ResetWin()
        {
            foreach (LineBehavior lb in linesController.Lines)
            {
                lb.ResetLineWinning();
            }

            scatterWinSymbols = null;
            controls.jackPots.ForEach((jp)=>{ jp.ResetWin(); });
            scatterWin = null;
        }

        internal void WinShowCancel()
        {
            //Debug.Log("cancel");
            if (onceTS!=null) onceTS.Break();
            if (contTS != null) contTS.Break();
            SimpleTween.Cancel(contID, false);

            if (linesController && linesController.Lines != null)
            {
                foreach (LineBehavior lb in linesController.Lines)
                {
                    lb.LineWinCancel();
                }
            }

            if (useScatter && scatterWinSymbols != null)
                foreach (var item in scatterWinSymbols)
                {
                    item.DestroyWinObject();
                }

            controls.jackPots.ForEach((jp) => { jp.WinSymbolsPlayCancel(); });
        }

        /// <summary>
        /// Show won symbols once
        /// </summary>
        private void WinSymbolShowOnce(Action<WinData> lineWinCallBack, Action scatterWinCallBack, Action jackPotWinCallBack, Action completeCallBack)
        {
            if (onceTS != null) onceTS.Break();

            onceTS = new TweenSeq();

            lineWinShowTime = (lineWinShowTime < 3) ? 3 : lineWinShowTime;

            //show linewins
            if (winLineFlashing == WinLineFlashing.Sequenced)
            {
                foreach (LineBehavior lB in linesController.Lines)
                {
                    if (lB.IsWinningLine)

                        onceTS.Add((callBack) =>
                        {
                            lB.LineFlashing(true);
                            lB.SetLineVisible(true);

                            lB.LineWinPlay(winTag, lineWinShowTime,
                                    (windata) =>
                                    {
                                        lB.LineFlashing(false);
                                        lB.SetLineVisible(false);
                                        lineWinCallBack?.Invoke(windata);
                                        callBack();
                                    });
                        });
                }
            }

            else if (winLineFlashing == WinLineFlashing.All)
            {
                ParallelTween pT = new ParallelTween();
                foreach (LineBehavior lB in linesController.Lines)
                {
                    if (lB.IsWinningLine)

                        pT.Add((callBack) =>
                        {
                            lB.LineFlashing(true);
                            lB.SetLineVisible(true);

                            lB.LineWinPlay(winTag, lineWinShowTime,
                                    (windata) =>
                                    {
                                        lB.LineFlashing(false);
                                        lB.SetLineVisible(false);
                                        lineWinCallBack?.Invoke(windata);
                                        callBack();
                                    });
                        });
                }
                onceTS.Add((callBack) =>
                {
                    pT.Start(() =>
                    {
                        callBack();
                    });
                });
            }

            else if (winLineFlashing == WinLineFlashing.None)
            {
                ParallelTween pT = new ParallelTween();
                foreach (LineBehavior lB in linesController.Lines)
                {
                    if (lB.IsWinningLine)

                        pT.Add((callBack) =>
                        {
                            lB.LineWinPlay(winTag, lineWinShowTime,
                                    (windata) =>
                                    {
                                        lineWinCallBack?.Invoke(windata);
                                        callBack();
                                    });
                        });
                }
                onceTS.Add((callBack) =>
                {
                    pT.Start(() =>
                    {
                        callBack();
                    });
                });
            }

            //show jackPot wins
            foreach (var item in controls.jackPots)
            {
                onceTS.Add((callBack) => { item.WinSymbolsPlay(winTag, lineWinShowTime, callBack); });
            }
            onceTS.Add((callBack) => { jackPotWinCallBack?.Invoke(); callBack?.Invoke(); });



            //show scatterwin
            if (useScatter && scatterWinSymbols != null && scatterWinSymbols.Count > 0)
            {
                ParallelTween pT = new ParallelTween();
                foreach (var item in scatterWinSymbols)
                {
                    pT.Add((callBack) =>
                    {
                        item.ShowWinPrefab(winTag);
                        TweenExt.DelayAction(item.gameObject, lineWinShowTime, callBack);
                    });
                }
                onceTS.Add((callBack) =>
                {
                    pT.Start(() =>
                    {
                        scatterWinCallBack?.Invoke();
                        callBack();
                    });
                });
            }

            onceTS.Add((callBack) =>
            {
                completeCallBack?.Invoke();
                callBack();
            });

            onceTS.Start();
        }

        /// <summary>
        /// Show won symbols continuous
        /// </summary>
        private void WinSymbolShowContinuous(Action<WinData> lineWinCallBack, Action scatterWinCallBack, Action jackPotWinCallBack, Action completeCallBack)
        {
            contTS = new TweenSeq();

            contTS.Add((callBack) =>
            {
                foreach (LineBehavior lb in linesController.Lines)
                {
                    lb.LineWinCancel();
                }
                if (useScatter && scatterWinSymbols != null)
                    foreach (var item in scatterWinSymbols)
                    {
                        item.DestroyWinObject();
                    }

                foreach (var item in controls.jackPots)
                {
                    item.WinCancel();
                }

                WinSymbolShowOnce(null, null, null, callBack);
            });

            WinSymbolShowOnce(lineWinCallBack, scatterWinCallBack, jackPotWinCallBack, () =>
                  {
                    //  Debug.Log("once complete");
                      completeCallBack?.Invoke();
                      contTS.StartCycle();
                  });
        }
        #endregion win animation

        #region get win
        /// <summary>
        /// Return true if slot has any winning
        /// </summary>
        internal bool HasAnyWinn(ref bool hasLineWin, ref bool hasScatterWin, ref List<JackPot> jackPotWin)
        {
            hasLineWin = false;
            hasScatterWin = false;

            foreach (LineBehavior lB in linesController.Lines)
            {
                if (lB.IsWinningLine)
                {
                    hasLineWin = true;
                    break;
                }
            }
            if (useScatter && HasScatterWin())
            {
                hasScatterWin = true;
            }

            jackPotWin = GetJackPotWin();
            return (hasLineWin || hasScatterWin || jackPotWin.Count > 0);
        }

        /// <summary>
        /// Search win symbols (paylines, scatter)
        /// </summary>
        internal void SearchWinSymbols()
        {
            foreach (var lb in linesController.Lines)
            {
                if (lb.IsSelected)
                {
                    lb.FindWin(payTable);
                }
            }

            // search scatters
            scatterWinSymbols = new List<SlotSymbol>();
            List<SlotSymbol> scatterSymbolsTemp = new List<SlotSymbol>();
            scatterWin = null;
            foreach (var item in slotGroupsBeh)
            {
                if (!item.HasSymbolInAnyRayCaster(scatter_id, ref scatterSymbolsTemp))
                {

                }
                else
                {
                    scatterWinSymbols.AddRange(scatterSymbolsTemp);
                }
            }

            if (useScatter)
                foreach (var item in scatterPayTable)
                {
                    if (item.scattersCount > 0 && item.scattersCount == scatterWinSymbols.Count)
                    {
                        scatterWin = new WinData(scatterWinSymbols, item.freeSpins, item.pay, item.payMult, item.freeSpinsMult, item.WinEvent);
                    }
                }
            if (scatterWin == null) scatterWinSymbols = new List<SlotSymbol>();
        }

        private bool HasScatterWin()
        {
            return scatterWin != null;
        }

        private List<JackPot> GetJackPotWin()
        {
            List<JackPot> wJP = new List<JackPot>();
            foreach (var item in controls.jackPots)
            {
                item.SearchWin();
                if (item.HaveWin) wJP.Add(item);
            }
            return wJP;
        }

        /// <summary>
        /// Return line win coins + sctater win coins, without jackpot
        /// </summary>
        /// <returns></returns>
        public int GetWinCoins()
        {
            return GetLineWinCoins() + GetScatterWinCoins();
        }

        public int GetLineWinCoins()
        {
            int res = 0;
            foreach (LineBehavior lB in linesController.Lines)
            {
                if (lB.IsWinningLine)
                {
                    res += lB.win.Pay;
                }
            }
            return res;
        }

        public List<LineBehavior> GetWinLines()
        {
            List<LineBehavior>  res = new List<LineBehavior>();
            foreach (LineBehavior lB in linesController.Lines)
            {
                if (lB.IsWinningLine)
                {
                    res.Add(lB);
                }
            }
            return res;
        }

        public int GetScatterWinCoins()
        {
            if (scatterWin != null) return  scatterWin.Pay;
            return 0;
        }

        /// <summary>
        /// Return line win spins + sctater win spins
        /// </summary>
        /// <returns></returns>
        public int GetWinSpins()
        {
            int res = 0;
            foreach (LineBehavior lB in linesController.Lines)
            {
                if (lB.IsWinningLine)
                {
                    res += lB.win.FreeSpins;
                }
            }

            if (scatterWin != null) res += scatterWin.FreeSpins;
            return res;
        }

        /// <summary>
        /// Return product of lines payMultiplier, sctater payMultiplier
        /// </summary>
        /// <returns></returns>
        public int GetPayMultiplier()
        {
            int res = 1;
            foreach (LineBehavior lB in linesController.Lines)
            {
                if (lB.IsWinningLine && lB.win.PayMult > 0)
                {
                    res *= lB.win.PayMult;
                }
            }

            if (scatterWin != null && scatterWin.PayMult > 0) res *= scatterWin.PayMult;
            return res;
        }

        /// <summary>
        /// Return product of lines free spins multipliers, scatter free spins multiplier
        /// </summary>
        /// <returns></returns>
        public int GetFreeSpinsMultiplier()
        {
            int res = 1;
            foreach (LineBehavior lB in linesController.Lines)
            {
                if (lB.IsWinningLine && lB.win.FreeSpinsMult != 0)
                {
                    res *= lB.win.FreeSpinsMult;
                }
            }

            if (scatterWin != null && scatterWin.FreeSpinsMult > 0) res *= scatterWin.FreeSpinsMult;
            return res;
        }

        public int GetWinLinesCount()
        {
            int res = 0;
            foreach (LineBehavior lB in linesController.Lines)
            {
                if (lB.IsWinningLine)
                {
                    res++;
                }
            }
            return res;
        }

        public void StartLineEvents()
        {
            foreach (LineBehavior lB in linesController.Lines)
            {
                if (lB.IsWinningLine)
                {
                    lB.win.WinEvent?.Invoke();
                }
            }
        }
        #endregion get win

        #region calc
        public WinDataCalc scatterWinCalc;

        /// <summary>
        /// calc line wins and scatter wins
        /// </summary>
        /// 
        internal void SearchWinCalc()
        {
            foreach (LineBehavior lB in linesController.Lines)
            {
                if (lB.gameObject.activeSelf)
                {
                    lB.FindWinCalc(payTable);
                }
            }

            // search scatters
            int scatterWinS = 0;
            int scatterSymbolsTemp = 0;
            scatterWinCalc = null;
            foreach (var item in slotGroupsBeh)
            {
                scatterSymbolsTemp = 0;
                for (int i = 0; i < item.RayCasters.Length; i++)
                {
                    if (item.RayCasters[i].ID == scatter_id)
                    {
                        scatterSymbolsTemp++;
                    }
                }
                scatterWinS += scatterSymbolsTemp;
            }

            if (useScatter)
                foreach (var item in scatterPayTable)
                {
                    if (item.scattersCount > 0 && item.scattersCount == scatterWinS)
                    {
                        scatterWinCalc = new WinDataCalc(scatterWinS, item.freeSpins, item.pay, item.payMult);
                        //Debug.Log("scatters: " + item.scattersCount);
                    }
                }
        }

        /// <summary>
        /// Return line win coins + sctater win coins, without jackpot
        /// </summary>
        /// <returns></returns>
        public int GetLineWinCoinsCalc()
        {
            int res = 0;
            foreach (LineBehavior lB in linesController.Lines)
            {
                if (lB.IsWinningLineCalc)
                {
                    res += lB.winCalc.Pay;
                }
            }
            return res;
        }

        /// <summary>
        /// Return line win coins + sctater win coins, without jackpot
        /// </summary>
        /// <returns></returns>
        public int GetScatterWinCoinsCalc()
        {
            int res = 0;
            if (scatterWinCalc != null) res += scatterWinCalc.Pay;
            return res;
        }

        /// <summary>
        /// Return line win spins + sctater win spins
        /// </summary>
        /// <returns></returns>
        public int GetLineWinSpinsCalc()
        {
            int res = 0;
            foreach (LineBehavior lB in linesController.Lines)
            {
                if (lB.IsWinningLineCalc)
                {
                    res += lB.winCalc.FreeSpins;
                }
            }
            return res;
        }

        /// <summary>
        /// Return line win spins + sctater win spins
        /// </summary>
        /// <returns></returns>
        public int GetScatterWinSpinsCalc()
        {
            int res = 0;
            if (scatterWinCalc != null) res += scatterWinCalc.FreeSpins;
            return res;
        }

        /// <summary>
        /// Return product of lines payMultiplier, sctater payMultiplier
        /// </summary>
        /// <returns></returns>
        public int GetLinePayMultiplierCalc()
        {
            int res = 1;
            foreach (LineBehavior lB in linesController.Lines)
            {
                if (lB.IsWinningLineCalc && lB.winCalc.PayMult != 0)
                {
                    res *= lB.winCalc.PayMult;
                }
            }
            return res;
        }

        /// <summary>
        /// Return product of lines payMultiplier, sctater payMultiplier
        /// </summary>
        /// <returns></returns>
        public int GetScatterPayMultiplierCalc()
        {
            int res = 1;
            if (scatterWinCalc != null && scatterWinCalc.PayMult != 0) res *= scatterWinCalc.PayMult;
            return res;
        }

        public int GetWinLinesCountCalc()
        {
            int res = 0;
            foreach (LineBehavior lB in linesController.Lines)
            {
                if (lB.IsWinningLineCalc)
                {
                    res++;
                }
            }
            return res;
        }
        #endregion calc


    }
}
