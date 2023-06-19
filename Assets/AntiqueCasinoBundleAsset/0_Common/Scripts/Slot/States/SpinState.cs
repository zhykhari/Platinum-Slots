using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Mkey
{
	public class SpinState : State<SlotController>
	{
        #region substates
        private NormalSpinState normalSpinState;
        private FreeSpinState freeSpinState;
        private AutoSpinState autoSpinState;
        private EndLessSpinState endLessSpinState;
        #endregion substates

        #region temp vars
        private SlotControls controls;
        #endregion temp vars

        public SpinState(SlotController slot, StateMachine<SlotController> stateMachine) : base(slot, stateMachine)
        {
            controls = slot.controls;
            normalSpinState = new NormalSpinState(slot, stateMachine);
            freeSpinState = new FreeSpinState(slot, stateMachine);
            autoSpinState = new AutoSpinState(slot, stateMachine);
            endLessSpinState = new EndLessSpinState(slot, stateMachine);
        }

        public override void EnterFrom(State<SlotController> oldState)
        {
            if (dLog) Debug.Log(ToString() + " - Enter From: " + oldState);
            base.EnterFrom(oldState);

            if (controls.HasFreeSpin) subState = freeSpinState;
            else if (stateObject.controls.Auto) subState = autoSpinState;
            else if (stateObject.controls.UseManualStop) subState = endLessSpinState;
            else subState = normalSpinState;
            if (dLog) Debug.Log("Spin Substate: " +  subState.ToString());
            subState.EnterFrom(oldState);
        }

        public override void ExitTo(State<SlotController> newState)
        {
            base.ExitTo(newState);
            subState.ExitTo(newState);
            subState = null;
        }

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
            return (subState!=null) ? subState.ToString() : "SpinState";
        }
    }
}
