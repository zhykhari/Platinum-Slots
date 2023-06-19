using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace Mkey
{
    public class SpinButtonBehavior : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        [SerializeField]
        private Text autoText;
        [SerializeField]
        private TextMesh autoTextMesh;
        [SerializeField]
        private SlotControls slotControls;
        [SerializeField]
        private string autoSpinModeText = "AUTO";
        [SerializeField]
        private string singleSpinModeText = "Hold for AutoSpin";
        [SerializeField]
        private string manualStopModeText = "STOP";

        #region events
        public Action ClickEvent;
        public Action LongPressClickEvent;
        public Action PointerDownEvent;
        public Action LongPointerDownEvent;
        #endregion events

        #region temp vars
        private bool up = true;
        private float downTime = 0;
        private const float longPressTime = 2f;
        private bool longPress = false;
        private WaitForEndOfFrame wef;
        private Button spinButton;
        private SceneButton sceneSpinButton;
        #endregion temp vars

        #region regular
        private void Start()
        {
            wef = new WaitForEndOfFrame();
            if (slotControls)
            {
                slotControls.ChangeAutoSpinModeEvent += (auto) => { SetSpinModeText(slotControls.Auto); };
               // slotControls.TryToSetAutoSpinModeEvent += () => { SetSpinModeText(true); };
            }
            SetSpinModeText(slotControls ? slotControls.Auto : false);
            spinButton = GetComponent<Button>();
            sceneSpinButton = GetComponent<SceneButton>();
        }
        #endregion regular

        #region pointer eventhandlers
        public void OnPointerDown(PointerEventData eventData)
        {
            longPress = false;
            up = false;
            if (!IsInteractable()) return;
            PointerDownEvent?.Invoke();
           
            StopCoroutine(CheckLongPressC());
            downTime = Time.time;
            StartCoroutine(CheckLongPressC());
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            up = true;
            if (!IsInteractable()) return;
            StopCoroutine(CheckLongPressC());
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            up = true;
            if (!IsInteractable()) return;
            StopCoroutine(CheckLongPressC());
            //Debug.Log(gameObject.name + " Was up." + (slotControls ? " SpinMode: auto - " + slotControls.Auto.ToString() : ""));
            if (longPress)
            {
                longPress = false;
                LongPressClickEvent?.Invoke();
            }
            else
            {
                ClickEvent?.Invoke();
            }
            SetSpinModeText(slotControls.Auto);
        }
        #endregion pointer eventhandlers

        private IEnumerator CheckLongPressC()
        {
            bool cancel = false;
            float dTime;
            while (!up && !cancel)
            {
                dTime = Time.time - downTime;
                if (dTime > longPressTime)
                {
                    longPress = true;
                    cancel = true;
                    if (!slotControls.Auto && slotControls.HoldToAutoSpin) SetSpinModeText(true); // set temporary text auto
                    LongPointerDownEvent?.Invoke();
                }
                yield return wef;
            }
        }

        private void SetSpinModeText(bool auto)
        {
            TextExtension.SetText(autoText, (!auto) ? singleSpinModeText : autoSpinModeText);
            TextExtension.SetText(autoTextMesh, (!auto) ? singleSpinModeText : autoSpinModeText);
        }

        private bool IsInteractable()
        {
            if (spinButton) return spinButton.interactable;
            if (sceneSpinButton) return sceneSpinButton.interactable;
            return true;
        }

        public void SetInteractable(bool interactable)
        {
            if (spinButton) spinButton.interactable = interactable;
            if (sceneSpinButton) sceneSpinButton.interactable = interactable;
        }
    }
}