using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Mkey
{
	public class NormalSpinState : State<SlotController>
	{
        #region temp vars
        private SlotControls controls;
        private SpinButtonBehavior SpinButton => controls.SpinButton;
        private AutoSpinButtonBehavior AutoSpinButton => controls.AutoSpinButton;
        private GuiController MGUI { get { return GuiController.Instance; } }
        #endregion temp vars

        public NormalSpinState(SlotController slot, StateMachine<SlotController> stateMachine) : base(slot, stateMachine)
        {
            controls = slot.controls;
            stateObject.EndWinCalcEvent += (win) => { if (win) stateMachine.ChangeState(stateObject.winState);  else stateMachine.ChangeState(stateObject.looseState); };
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

            controls.SetSpinButtonText("SPIN");
            controls.SetControlActivity(false, false);
            if (dLog) Debug.Log(ToString() + "  - run normal spin");
            controls.ApplyBet();
            stateObject.RunSlot();
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
          
        }

        public void Spin_Click()
        {

        }

        public void AutoSpin_Click()
        {

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
            return "NormalSpinState";
        }
    }
}
