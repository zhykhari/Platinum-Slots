using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Mkey
{
    public class WildFeature : MonoBehaviour
    {
        #region temp vars
        protected SlotController controller;
        protected List<SlotGroupBehavior> wildReels;
        protected List<SlotSymbol>wildSymbols;
        protected int wild_id =-1;
        protected SlotIcon wildIcon;
        #endregion temp vars

        public void InitStart() // avoid to use multiple features per slot
        {
            OnStart();
        }

        public virtual void OnStart()
        {
            controller = GetComponentInParent<SlotController>();
            if (controller)
            {
                controller.StartSpinEvent += StartSpinEventHandler;
                controller.EndSpinEvent += EndSpinEventHandler;
                wild_id = controller.wild_id;
                wildIcon = controller.slotIcons[wild_id];
            }
            wildSymbols = new List<SlotSymbol>();
            wildReels = new List<SlotGroupBehavior>();
        }

        protected virtual void StartSpinEventHandler()
        {
            //Debug.Log(name + "start spin event");
        }

        protected virtual void EndSpinEventHandler()
        {
            GetWildsOnReel();
            //Debug.Log(name + " - end spin event");
            //Debug.Log("wildSymbols.Count: " + wildSymbols.Count);
        }

        private void GetWildsOnReel()
        {
            wildSymbols = new List<SlotSymbol>();
            wildReels = new List<SlotGroupBehavior>();

            List<SlotSymbol> wildSymbolsTemp = new List<SlotSymbol>();
            foreach (var item in controller.slotGroupsBeh)
            {
                if (!item.HasSymbolInAnyRayCaster(wild_id, ref wildSymbolsTemp))
                {
                }
                else
                {
                    wildSymbols.AddRange(wildSymbolsTemp);
                }
            }

            foreach (var item in wildSymbols)
            {
                if (!wildReels.Contains(item.Reel)) wildReels.Add(item.Reel);
            }
        }
    }
}