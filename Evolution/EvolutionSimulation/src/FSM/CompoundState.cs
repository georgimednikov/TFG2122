using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvolutionSimulation.FSM
{
    class CompoundState : IState
    {
        public CompoundState(string name, IState initialState)
        {
            this.name = name;
            stateMachine = new Fsm(initialState);
        }
        public int Action()
        {
            int prevActionPoints = stateMachine.ActionPoints;
            stateMachine.Evaluate();
            if (stateMachine.Execute())
                return prevActionPoints - stateMachine.ActionPoints;
            return 0;
        }

        public bool canPerformAction(int actionPoints)
        {
            return stateMachine.CurrentState.canPerformAction(actionPoints);
        }

        public override string ToString()
        {
            return name;
        }

        string name;
        Fsm stateMachine;
    }
}
