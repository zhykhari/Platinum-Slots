using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Mkey
{
	public class FreeSpinState : State<SlotController>
	{
        #region temp vars
        private SlotControls controls;
        private SpinButtonBehavior SpinButton => controls.SpinButton;
        private AutoSpinButtonBehavior AutoSpinButton => controls.AutoSpinButton;
        #endregion temp vars

        public FreeSpinState(SlotController slot, StateMachine<SlotController> stateMachine) : base(slot, stateMachine)
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

            controls.SetControlActivity(false, controls.Auto);
            if (dLog) Debug.Log(ToString() + "  - run free spin");
            stateObject.IsFreeSpin = true;
            controls.ApplyFreeSpin();
            stateObject.RunSlot();
            controls.SetSpinButtonText(controls.FreeSpins.ToString());
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
            controls.ResetAutoSpinsMode();
            // and diisable input
            controls.SetControlActivity(false, false);
        }

        public void Spin_Click()
        {
            controls.ResetAutoSpinsMode();
            // and diisable input
            controls.SetControlActivity(false, false);
        }

        public void AutoSpin_Click()
        {
            controls.ResetAutoSpinsMode();
            // and diisable input
            controls.SetControlActivity(false, false);
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

        public override string ToString()
        {
            return "FreeSpinState";
        }
    }
}
