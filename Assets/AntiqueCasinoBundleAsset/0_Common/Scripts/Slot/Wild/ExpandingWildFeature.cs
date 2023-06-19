using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mkey
{
    public class ExpandingWildFeature : WildFeature
    {
        public ExpandingWildSymbol expandingWildPrefab; //gameobject with sprite and animation

        private List<ExpandingWildSymbol> ewObjects;

        private Dictionary<SlotSymbol, int> dict;

        public override void OnStart()
        {
            base.OnStart();
            dict = new Dictionary<SlotSymbol, int>();
            ewObjects = new List<ExpandingWildSymbol>();
        }

        protected override void StartSpinEventHandler()
        {
            base.StartSpinEventHandler();

            foreach (var item in ewObjects)
            {
                if (item) item.CloseAndDestroy();
            }

            foreach (var item in dict)
            {
                if (item.Key)
                {
					SimpleTween.Cancel(item.Key.gameObject, false);
                    item.Key.SetIcon(controller.slotIcons[item.Value], item.Value); // restore icon ids
                    item.Key.gameObject.SetActive(true);
                }
            }

            ewObjects = new List<ExpandingWildSymbol>();
            dict = new Dictionary<SlotSymbol, int>();
        }

        protected override void EndSpinEventHandler()
        {
            base.EndSpinEventHandler();

            ewObjects = new List<ExpandingWildSymbol>();
            ExpandingWildSymbol g;
            if (expandingWildPrefab)
            {
                foreach (var item in wildReels)
                {
                    foreach (var rC in item.RayCasters)
                    {
                        SlotSymbol s = rC.Symbol;
                        if (s)
                        {
                            dict.Add(s, s.IconID);
                            s.SetIcon(s.Icon, wild_id); // replace id temporary
                            TweenExt.DelayAction(s.gameObject, 0.5f, ()=> { s.gameObject.SetActive(false); }); 
                        }
                    }

                    g = Instantiate(expandingWildPrefab, item.transform);
                    if (g)
                    {
                        g.transform.localPosition = Vector3.zero;
                        ewObjects.Add(g);
                    }
                }
            }
        }

        private void WinEventHandler(SlotSymbol s)
        {

        }
    }
}