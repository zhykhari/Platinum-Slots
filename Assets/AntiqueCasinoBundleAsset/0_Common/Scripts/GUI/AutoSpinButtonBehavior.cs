using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace Mkey
{
    public class AutoSpinButtonBehavior : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        [SerializeField]
        private SlotControls slotControls;

        #region events
        public Action ClickEvent;
        #endregion events

        #region temp vars
        private bool up = true;
        private const float longPressTime = 2f;
        private WaitForEndOfFrame wef;
        private Button autoSpinButton;
        private SceneButton sceneAutoSpinButton;
        private bool auto = false;
        #endregion temp vars

        #region regular
        private void Start()
        {
            wef = new WaitForEndOfFrame();
            if (slotControls)
            {
                slotControls.ChangeAutoSpinModeEvent += (auto) => { SetPressed(auto); };
                auto = slotControls.Auto;
            }
            autoSpinButton = GetComponent<Button>();
            sceneAutoSpinButton = GetComponent<SceneButton>();
            SetPressed(auto);
            Debug.Log("SetPressed(auto) " +auto);
        }

        private void OnGUI()
        {
            if(slotControls && (auto != slotControls.Auto))
            {
                auto = slotControls.Auto;
              //  SetPressed(auto);
            }
        }
        #endregion regular

        #region pointer eventhandlers
        public void OnPointerDown(PointerEventData eventData)
        {
            up = false;
            if (!IsInteractable()) return;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            up = true;
            if (!IsInteractable()) return;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            up = true;
            if (!IsInteractable()) return;
            Debug.Log(gameObject.name + " Was up." + (slotControls ? " SpinMode: auto - " + slotControls.Auto.ToString() : ""));
            ClickEvent?.Invoke();
        }
        #endregion pointer eventhandlers

        private void SetPressed(bool pressed)
        {
            if (autoSpinButton && pressed) autoSpinButton.SetPressed();
            else if (autoSpinButton)  autoSpinButton.Release();

            if (sceneAutoSpinButton && pressed) sceneAutoSpinButton.SetPressed();
            else if (sceneAutoSpinButton) sceneAutoSpinButton.Release();
        }

        private bool IsInteractable()
        {
            if (autoSpinButton) return autoSpinButton.interactable;
            if (sceneAutoSpinButton) return sceneAutoSpinButton.interactable;
            return true;
        }

        public void SetInteractable(bool interactable)
        {
            if (autoSpinButton) autoSpinButton.interactable = interactable;
            if (sceneAutoSpinButton) sceneAutoSpinButton.interactable = interactable;
        }
    }
}