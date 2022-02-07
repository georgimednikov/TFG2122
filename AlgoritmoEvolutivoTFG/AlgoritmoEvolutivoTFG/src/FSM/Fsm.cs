using System;
using Stateless;

namespace EvolutionSimulation.FSM
{

    /// <summary>
    /// State Machine implementation. It uses the Stateless library
    /// </summary>
    public class Fsm
    {
        public Fsm(IState initalState)
        {
            machine = new StateMachine<IState, ITransition>(initalState);
        }

        /// <summary>
        /// Adds a transition from the original state to the destiny state
        /// </summary>
        public void AddTransition(IState og, ITransition t, IState dest)
        {
            machine.Configure(og)
                .Permit(t, dest); //?
        }

        public void Execute()
        {
            machine.State.Action();
        }
        public void Evaluate()
        {
            foreach (ITransition t in machine.GetPermittedTriggers())
            {
                if (t.Evaluate())
                {
                    machine.Fire(t);
                    break;
                }
            }
        }
        StateMachine<IState, ITransition> machine;
    }
}
