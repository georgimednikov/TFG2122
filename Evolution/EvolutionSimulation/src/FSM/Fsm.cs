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
            machine = new StateMachine<IState, ITransition>(initalState, FiringMode.Queued);
        }

        /// <summary>
        /// Adds a transition from the original state to the destiny state
        /// </summary>
        public void AddTransition(IState og, ITransition t, IState dest)
        {
            machine.Configure(og)
                .Permit(t, dest);
        }

        public IState GetState()
        {
            return machine.State;
        }

        /// <summary>
        /// Adds the state sub inside of super
        /// </summary>
        public void AddSubstate(IState super, IState sub)
        {
            machine.Configure(sub)
                .SubstateOf(super);
        }

        /// <summary>
        /// Executes the current state
        /// </summary>
        public void Execute()
        {
            while(machine.State.Action()) Evaluate();
        }

        /// <summary>
        /// Triggers whichever transitions return true on its Evaluate, if there is an available transition
        /// </summary>
        public void Evaluate()
        {
            foreach (ITransition t in machine.GetPermittedTriggers())
            {
                if (machine.CanFire(t) && t.Evaluate())
                {
                    //TO DO: Quitar esto, esta pa debugear 
                    Console.WriteLine("Transition: " + t.GetType().Name);
                    
                    machine.Fire(t);
                }
            }
        }
        StateMachine<IState, ITransition> machine;
    }
}
