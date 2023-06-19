using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Mkey
{
    public class PreSpinState : State<SlotController>
    {
        #region temp vars
        private SlotControls controls;
        private SpinButtonBehavior SpinButton => controls.SpinButton;
        private AutoSpinButtonBehavior AutoSpinButton => controls.AutoSpinButton;
        private GuiController MGUI { get { return GuiController.Instance; } }
        private SlotPlayer MPlayer { get { return SlotPlayer.Instance; } }
        #endregion temp vars

        public PreSpinState(SlotController slot, StateMachine<SlotController> stateMachine) : base(slot, stateMachine)
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

            controls.SetControlActivity(false, false);
            stateObject.winController.WinEffectsShow(false, false);
            stateObject.winController.WinShowCancel();
            stateObject.winController.ResetWin();
            stateObject.IsFreeSpin = false;
            controls.JPWinCancel();

            if (!controls.AnyLineSelected)
            {
                MGUI.ShowMessage(null, "Please select a any line.", 1.5f, null);
                stateMachine.ChangeState(stateObject.iddleState);
                return;
            }

            if (controls.TotalBet > MPlayer.Coins)
            {
                MGUI.ShowMessage(null, "You have no money.", 1.5f, null);
                if (controls.Auto) controls.ResetAutoSpinsMode();
                stateMachine.ChangeState(stateObject.iddleState);
                return;
            }

            if (dLog) Debug.Log(ToString() + "  - go to spin state");

            stateMachine.ChangeState(stateObject.spinState);
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
            return "PreSpinState";
        }
    }
}
