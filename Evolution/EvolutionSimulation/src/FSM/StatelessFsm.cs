using System;
using Stateless;
using Stateless.Graph;

namespace EvolutionSimulation.FSM
{

    /// <summary>
    /// State Machine implementation. It uses the Stateless library
    /// </summary>
    public class StatelessFsm
    {
        public StatelessFsm(IState initalState)
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
        /// Adds a transition to the same state
        /// </summary>
        public void AddReentry(IState state, ITransition sub)
        {
            machine.Configure(state)
                .PermitReentry(sub);
        }


        /// <summary>
        /// Specify an action that will execute when transitioning to the state
        /// </summary>
        public void OnEntry(IState state, Action action)
        {
            machine.Configure(state)
                .OnEntry(action);
        }

        /// <summary>
        /// Specify an action that will execute when activating the state
        /// </summary>
        public void OnActivate(IState state, Action action)
        {
            machine.Configure(state)
                .OnActivate(action);
        }

        /// <summary>
        /// Sets an initial transition to trigger when entering a
        /// super state to one of its substates.
        /// </summary>
        public void InitalTransition(IState superState, IState subState)
        {
            machine.Configure(superState)
                .InitialTransition(subState);
        }

        /// <summary>
        /// Returns a string representation of the state machine in the DOT graph language
        /// It can then be visualized by pasting the output in http://www.webgraphviz.com/
        /// </summary>
        public string ExportToDotGraph()
        {
            return UmlDotGraph.Format(machine.GetInfo());
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

        public void ObtainActionPoints(int metabolism)
        {
            actionPoints += metabolism * 10;
        }

        // action points of the creature the fsm belongs to
        public int actionPoints;

        StateMachine<IState, ITransition> machine;
    }
}
