using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Mkey
{
	public class FreeInputWinState : State <SlotController>
	{
        #region temp vars
        private SlotControls controls;
        private SpinButtonBehavior SpinButton => controls.SpinButton;
        private AutoSpinButtonBehavior AutoSpinButton => controls.AutoSpinButton;
        #endregion temp vars

        public FreeInputWinState(SlotController slot, StateMachine<SlotController> stateMachine) : base(slot, stateMachine)
        {
            controls = slot.controls;
        }

        public override void EnterFrom(State<SlotController> oldState)
        {
            if (dLog) Debug.Log(ToString() + " - Enter From: " + oldState);
            base.EnterFrom(oldState);
            if (SpinButton)
            {
                SpinButton.LongPressClickEvent += LongPressSpin_Click;
                SpinButton.ClickEvent += Spin_Click;
                SpinButton.PointerDownEvent += Spin_PointerDown;
                SpinButton.LongPointerDownEvent += Spin_LongPointerDown;
            }
            if (AutoSpinButton)
            {
                AutoSpinButton.ClickEvent += AutoSpin_Click;
            }

            controls.SetSpinButtonText(controls.FreeSpins > 0 ? controls.FreeSpins.ToString() : "SPIN");
            SetInputActivity();
            stateObject.FreeInputWinShow((needSpin)=> { if (needSpin) stateMachine.ChangeState(stateObject.preSpinState); });
        }

        public override void ExitTo(State<SlotController> newState)
        {
            base.ExitTo(newState);
            if (SpinButton)
            {
                SpinButton.LongPressClickEvent -= LongPressSpin_Click;
                SpinButton.ClickEvent -= Spin_Click;
                SpinButton.PointerDownEvent -= Spin_PointerDown;
                SpinButton.LongPointerDownEvent -= Spin_LongPointerDown;
            }
            if (AutoSpinButton)
            {
                AutoSpinButton.ClickEvent -= AutoSpin_Click;
            }
        }

        #region input
        public void Spin_PointerDown()
        {

        }

        public void Spin_LongPointerDown()
        {

        }

        public void LongPressSpin_Click()
        {
            if (controls.Auto)
            {
                controls.ResetAutoSpinsMode();
                SetInputActivity();
            }
            else if (controls.HoldToAutoSpin)
            {
                controls.SetAutoSpinsMode();
                stateMachine.ChangeState(stateObject.preSpinState);
            }
        }

        public void Spin_Click()
        {
            if (controls.Auto)
            {
                controls.ResetAutoSpinsMode();
                SetInputActivity();
            }
            else
            {
                stateMachine.ChangeState(stateObject.preSpinState);
            }
        }

        public void AutoSpin_Click()
        {
            if (controls.Auto)
            {
                controls.ResetAutoSpinsMode();
                SetInputActivity();
            }
            else
            {
                controls.SetAutoSpinsMode();
                stateMachine.ChangeState(stateObject.preSpinState);
            }
        }
        #endregion input

        #region old
        public override void HandleInput()
        {
            base.HandleInput();
        }

        public override void PhysicsUpdate()
        {
            base.PhysicsUpdate();
        }
        #endregion old

        private void SetInputActivity()
        {
            if (controls.HasFreeSpin && controls.AutoPlayFreeSpins) controls.SetControlActivity(false, false);
            else if (controls.HasFreeSpin && !controls.AutoPlayFreeSpins) controls.SetControlActivity(false, true);
            else if (controls.Auto)
            {
                controls.SetControlActivity(false, true);
            }
            else controls.SetControlActivity(true, true);
        }

        public override string ToString()
        {
            return "FreeInputWinState";
        }
    }
}
