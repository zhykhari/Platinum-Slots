﻿using UnityEngine;
using System;
namespace Mkey
{
    public class SlotSymbol : MonoBehaviour
    {
        [SerializeField]
        private SpriteRenderer sR;
        [SerializeField]
        private IconSpriteDeformerMesh deformer;
        [SerializeField]
        private bool useDeformer;
        [SerializeField]
        private float speed = 0;
        
        #region properties
        public SlotIcon Icon { get; private set; }
        public int IconID;//{ get; private set; }
        public SlotGroupBehavior Reel { get; private set; }
        private SlotSymbol NextSymbol { get; set; }
        #endregion properties

        #region temp vars
        private int defaultSortingOrder = 10;
        private int defaultSortingLayerID = 0;// ID of the default sorting layer is always 0.
        private GameObject particles;
        private Vector3 pos;
        private Vector3 oldPos;
        private SlotController Slot { get; set; }
        private WinSymbolBehavior wsb;
        #endregion temp vars

        #region regular
        private void Start()
        {
            pos = transform.position;
            oldPos = transform.position;
            Slot = GetComponentInParent<SlotController>();
            Reel = GetComponentInParent<SlotGroupBehavior>();
        }

        private void Update()
        {
            pos = transform.position;
            speed = (pos - oldPos).magnitude/ Time.deltaTime;
            oldPos = pos;
            SetIcon(speed > 10);
        }

        private void OnDestroy()
        {
            DestroyWinObject();
        }

        private void OnDisable()
        {
            DestroyWinObject();
        }
        #endregion regular

        public Action <SlotSymbol> WinShowEvent;
        public Action<SlotSymbol> WinShowCancelEvent;

        internal void SetIcon(SlotIcon icon, int iconID)
        {
            IconID = iconID;
            Icon = icon;
            SetIcon(speed>0);
        }

        private void SetIcon(bool blur)
        {
            if (Icon == null) return;
            if (sR)
            {
                sR.sprite =(blur && Icon.iconBlur) ? Icon.iconBlur : Icon.iconSprite;
            }
            else if (deformer)
            {
                deformer.SetTexture((blur && Icon.iconBlur) ? Icon.iconBlur.texture : Icon.iconSprite.texture );
            }
        }

        #region win animation
        internal void ShowParticles(bool activity, GameObject particlesPrefab)
        {
            if (activity)
            {
                if (particlesPrefab)
                {
                    if (particles == null)
                    {
                        particles = Instantiate(particlesPrefab, transform.position, transform.rotation);
                        particles.transform.parent = transform.parent;
                        particles.transform.localScale = transform.localScale;
                    }
                }
            }
            else
            {
                if (particles)
                {
                    GameObject p = particles;
                    particles = null;
                    Destroy(p);
                }
            }
        }

        internal void ShowWinPrefab(string tag)
        {
            DestroyWinObject();

            // get prefab by tag from icon or from slotcontroller
            WinSymbolBehavior wsbPrefab =  Icon.GetWinPrefab(tag);
            if (!wsbPrefab) wsbPrefab =Slot.GetWinPrefab(tag);
            if (!wsbPrefab) return;

            wsb = Instantiate(wsbPrefab);
            wsb.transform.parent = transform;
            wsb.transform.localScale = Vector3.one;
            wsb.transform.localPosition = Vector3.zero;
            wsb.transform.localEulerAngles = Vector3.zero;
            WinShowEvent?.Invoke(this);
        }

        internal void DestroyWinObject()
        {
            ShowSymbol();
            if (wsb)
            {
                WinSymbolBehavior wsbTemp = wsb;
                wsb = null;
                Destroy(wsbTemp.gameObject);
            }
            WinShowCancelEvent?.Invoke(this);
        }

        internal Sprite GetSprite()
        {
            return (Icon != null) ? Icon.iconSprite : null;
        }

        internal int GetSortingOrder()
        {
            if (sR) return sR.sortingOrder;
            if (deformer) return deformer.SortingOrder;
            return defaultSortingOrder;
        }

        internal int GetSortingLayerID()
        {
            if (sR) return sR.sortingLayerID;
            if (deformer) return deformer.SortingLayerID;
            return defaultSortingLayerID; 
        }

        internal void HideSymbol()
        {
            if (sR) sR.enabled = false;
            else 
            {
                MeshRenderer mR = GetComponent<MeshRenderer>();
                if (mR) mR.enabled = false;
            }
        }

        internal void ShowSymbol()
        {
            if (sR) sR.enabled = true;
            else
            {
                MeshRenderer mR = GetComponent<MeshRenderer>();
                if (mR) mR.enabled = true;
            }
        }
        #endregion win animation

        #region alias
        public SlotSymbol GetAlias()
        {
            return GetComponentInChildren<SlotSymbol>();
        }

        public SlotSymbol SetAlias(SlotSymbol prefab)
        {
            return Instantiate(prefab, transform);
        }
        #endregion alias

        private void StartSpinEventHandler()
        {

        }

        private void EndSpinEventHandler()
        {

        }

        private void LandOnReelEventHandler()
        {

        }

        public override string ToString()
        {
            return (sR && sR.sprite) ? sR.sprite.name : "null sprite";
        }

    }
}
