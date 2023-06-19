using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mkey
{
    public class ExpandingWildSymbol : MonoBehaviour
    {
        public GameObject winGameObjectPrefab;

        #region temp vars
        private SlotGroupBehavior sGB;
        private List<SlotSymbol> wilds;
        private GameObject winGameObject;
        private SpriteRenderer sR;
        private Vector3 sourceScale = Vector3.one;
        #endregion temp vars

        #region regular
        private void Awake()
        {
            sourceScale = transform.localScale;
            transform.localScale = Vector3.zero;
        }

        private void Start()
        {
            SimpleTween.Value(gameObject, Vector3.zero, sourceScale, 0.5f).SetOnUpdate((Vector3 val)=> 
            {
                if (this) transform.localScale = val;
            }).SetEase(EaseAnim.EaseOutBounce);

            sGB = GetComponentInParent<SlotGroupBehavior>();
            wilds = new List<SlotSymbol>();
            sR = GetComponent<SpriteRenderer>();

            foreach (var item in sGB.RayCasters)
            {
                wilds.Add(item.Symbol);
                item.Symbol.WinShowEvent += WinShowEventHandler;
                item.Symbol.WinShowCancelEvent += WinShowCancelEventHandler;
            }
        }

        private void OnDestroy()
        {
            foreach (var item in wilds)
            {
                if (item) item.WinShowEvent -= WinShowEventHandler;
                if (item) item.WinShowCancelEvent -= WinShowCancelEventHandler;
            }
            SimpleTween.Cancel(gameObject, false);
        }
        #endregion regular

        public void CloseAndDestroy()
        {
            SimpleTween.Value(gameObject, sourceScale, Vector3.zero, 0.3f).SetOnUpdate((Vector3 val) =>
            {
                if (this) transform.localScale = val;
            }).AddCompleteCallBack(()=> { Destroy(gameObject); });
        }

        private void WinShowEventHandler(SlotSymbol slotSymbol)
        {
            GameObject tGO = winGameObject;
            winGameObject = null;
            if (tGO) Destroy(tGO);

            if (winGameObjectPrefab) winGameObject = Instantiate(winGameObjectPrefab, transform);
            if (sR) sR.enabled = false;
        }

        private void WinShowCancelEventHandler(SlotSymbol slotSymbol)
        {
            GameObject tGO = winGameObject;
            winGameObject = null;
            if (tGO) Destroy(tGO);
            if (sR) sR.enabled = true;
        }
    }
}