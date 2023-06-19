using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Mkey
{
	public class MiniGameState : State <SlotController>
	{
        #region temp vars

        #endregion temp vars

        public MiniGameState(SlotController slot, StateMachine<SlotController> stateMachine) : base(slot, stateMachine)
        {

        }

        public override void EnterFrom(State<SlotController> oldState)
        {
            base.EnterFrom(oldState);
           // horizontalInput = verticalInput = 0.0f;
        }

        public override void ExitTo(State<SlotController> newState)
        {
            base.ExitTo(newState);
          //  character.ResetMoveParams();
        }

        public override void HandleInput()
        {
            base.HandleInput();
          //  verticalInput = Input.GetAxis("Vertical");
          //  horizontalInput = Input.GetAxis("Horizontal");
        }

        public override void PhysicsUpdate()
        {
            base.PhysicsUpdate();
          //  character.Move(verticalInput * speed, horizontalInput * rotationSpeed);
        }
    }
}
