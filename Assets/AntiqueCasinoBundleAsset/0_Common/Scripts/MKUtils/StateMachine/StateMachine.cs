using UnityEngine;

namespace Mkey
{
    public class StateMachine<T>
    {
        public State<T> CurrentState { get; private set; }

        public void Initialize(State<T> startingState)
        {
            CurrentState = startingState;
            startingState.EnterFrom(null);
        }

        public void ChangeState(State<T> newState)
        {
          //  Debug.Log("ChangeState; oldstate: " + CurrentState + "; newState: " + newState);
            State<T> old = CurrentState;
            CurrentState.ExitTo(newState);
            CurrentState = newState;
            newState.EnterFrom(old);
        }
    }
}
/*
 https://www.raywenderlich.com/6034380-state-pattern-using-unity#toc-anchor-005
 https://ru.wikipedia.org/wiki/%D0%9A%D0%BE%D0%BD%D0%B5%D1%87%D0%BD%D1%8B%D0%B9_%D0%B0%D0%B2%D1%82%D0%BE%D0%BC%D0%B0%D1%82
 https://gamedev.ru/code/articles/finite_state_machine
 https://habr.com/ru/company/ruvds/blog/346908/
 */
