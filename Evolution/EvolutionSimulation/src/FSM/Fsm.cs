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
        /// Attempts to execute the current state.
        /// If the action could not be executed returns false.
        /// </summary>
        public bool Execute()
        {
            int cost = 0;
            if(machine.State.canPerformAction(actionPoints))
                cost = machine.State.Action();
            if (cost > 0)   //TODO: Ver si hay que hacer esta comprobacion de si se hace o no accion porque ya esta canPerform
            {
                actionPoints -= cost;
                return true;
            }
            else return false;
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
                    //TODO: Quitar esto, esta pa debugear 
                    Console.WriteLine("Transition: " + t.GetType().Name);
                    
                    machine.Fire(t);
                }
            }
        }

        public void obtainActionPoints(int metabolism)
        {
            actionPoints += metabolism * 10;
        }

        // action points of the creature the fsm belongs to
        public int actionPoints;

        StateMachine<IState, ITransition> machine;
    }
}
