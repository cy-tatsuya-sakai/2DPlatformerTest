using System.Collections;
using UnityEngine;

namespace Lib.State
{
    /// <summary>
    /// 有限ステートマシン
    /// </summary>
    public class FiniteStateMachine
    {
        public enum Case
        {
            Enter,
            Exec,
            Exit
        };


        public delegate void State(Case c);

        public State Curr { get; private set; }
        public State Prev { get; private set; }
        public State Next { get; private set; }

        public FiniteStateMachine(State state)
        {
            ChangeState(state);
        }

        public void Update()
        {
            ChangeNext();
            Curr?.Invoke(Case.Exec);
        }

        public void Exit()
        {
            Curr?.Invoke(Case.Exit);
            Curr = Prev = Next =null;
        }

        public void ChangeState(State state)
        {
            Next = state;
        }

        private void ChangeNext()
        {
            if(Next == null) { return; }
            if(Next == Curr) { return; }

            Curr?.Invoke(Case.Exit);
            Next?.Invoke(Case.Enter);
            Prev = Curr;
            Curr = Next;
            Next = null;
        }
    }
}
