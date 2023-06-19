using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Mkey
{
	public class IddleState : State <SlotController>
	{
        #region temp vars
        private SlotControls controls;
        private SpinButtonBehavior SpinButton => controls.SpinButton;
        private AutoSpinButtonBehavior AutoSpinButton => controls.AutoSpinButton;
        #endregion temp vars

        public IddleState(SlotController slot, StateMachine<SlotController> stateMachine) : base(slot, stateMachine)
        {
            controls = slot.controls;
        }

        public override void EnterFrom(State<SlotController> oldState)
        {
            if (dLog) Debug.Log(ToString() + " - enter from: " + oldState);
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

            controls.SetControlActivity(true, true);
            controls.SetSpinButtonText("SPIN");
        }

        public override void ExitTo(State<SlotController> newState)
        {
            if (dLog) Debug.Log(ToString() + " - Exit To: " + newState);
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
            if (controls.HoldToAutoSpin)
            {
                controls.SetAutoSpinsMode();
            }
            stateMachine.ChangeState(stateObject.preSpinState);
        }

        public void Spin_Click()
        {
            Debug.Log(ToString() + " - Spin Click");
            stateMachine.ChangeState(stateObject.preSpinState);
        }

        public void AutoSpin_Click()
        {
            Debug.Log(ToString() + " - Auto Spin Click");
            controls.SetAutoSpinsMode();
            stateMachine.ChangeState(stateObject.preSpinState);
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
            return "IddleState";
        }
    }
}

/*
  1) music
  2) gui info
 */ 