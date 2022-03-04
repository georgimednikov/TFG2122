using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvolutionSimulation.FSM
{
    /// <summary>
    /// Class to form a superstate that performes as a FSM with different substates.
    /// </summary>
    class CompoundState : IState
    {
        public CompoundState(string name, Fsm fsm)
        {
            this.name = name;
            stateMachine = fsm;
        }

        /// <summary>
        /// Performs the action of the current substate
        /// </summary>
        public void Action()
        {
            stateMachine.CurrentState.Action();
        }

        /// <summary>
        /// Evaluates transitions of the substates and returns the cost
        /// of the target substate
        /// </summary>
        public virtual int GetCost()
        {
            return stateMachine.EvaluateCost();
        }

        public virtual void OnEntry()
        {
            stateMachine.CurrentState.OnEntry();
        }

        public virtual void OnExit()
        {
            stateMachine.CurrentState.OnExit();
        }
        public override string ToString()
        {
            return name;
        }

        string name;
        public Fsm stateMachine { get; private set; }
    }
}
