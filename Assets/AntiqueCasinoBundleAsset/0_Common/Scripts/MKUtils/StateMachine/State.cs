
namespace Mkey
{
    public abstract class State<T>
    {
        protected T stateObject;
        protected StateMachine <T> stateMachine;
        protected bool dLog = true;
        protected State <T> subState;

        protected State (T stateObject, StateMachine<T> stateMachine)
        {
            this.stateObject = stateObject;
            this.stateMachine = stateMachine;
        }

        public virtual void EnterFrom(State<T> oldState)
        {
        }

        public virtual void HandleInput()
        {

        }

        public virtual void LogicUpdate()
        {
           // UnityEngine.Debug.Log(ToString());
        }

        public virtual void PhysicsUpdate()
        {
          
        }

        public virtual void ExitTo(State<T> newState)
        {

        }

        protected void DisplayOnUI( )
        {

        }
    }
}
