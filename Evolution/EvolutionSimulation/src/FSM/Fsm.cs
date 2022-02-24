using System;
using System.Collections.Generic;
using Stateless;
using Stateless.Graph;

namespace EvolutionSimulation.FSM
{

    /// <summary>
    /// State Machine implementation. It uses the Stateless library
    /// </summary>
    public class Fsm
    {
        /// <summary>
        /// Auxiliary structure to represent connections between states,
        /// it consists of a transition and the state that the machine goes to
        /// when it triggers.
        /// </summary>
        struct Connection
        {
            public ITransition Transition;
            public IState OutState;

            public Connection(ITransition t, IState os)
            {
                Transition = t;
                OutState = os;
            }
        }

        public Fsm(IState initalState)
        {
            machine = new Dictionary<IState, List<Connection>>();
            machine.Add(initalState, new List<Connection>());
            CurrentState = initalState;
        }

        /// <summary>
        /// Adds a transition from the original state to the destiny state
        /// If a states has no transitions
        /// </summary>
        public void AddTransition(IState og, ITransition t, IState dest)
        {
            if (!machine.ContainsKey(og))   machine.Add(og, new List<Connection>());
            if (!machine.ContainsKey(dest)) machine.Add(og, new List<Connection>());
            machine[og].Add(new Connection(t, dest));
        }

        /// <summary>
        /// Attempts to execute the current state.
        /// If the action could not be executed returns false.
        /// </summary>
        public bool Execute()
        {
            int prevActionPoints = ActionPoints;
            int cost = 0;
            if(CurrentState.canPerformAction(ActionPoints))
                cost = CurrentState.Action();
            ActionPoints -= cost;
            return prevActionPoints != ActionPoints;
        }

        /// <summary>
        /// Triggers whichever transitions return true on its Evaluate, if there is an available transition
        /// All transitions all evaluated, only the first one that triggers results in a transition.
        /// </summary>
        public void Evaluate()
        {
            bool firstTrigger = false;
            foreach (Connection c in machine[CurrentState])
            {
                if (c.Transition.Evaluate() && !firstTrigger)                
                    CurrentState = c.OutState;                
            }
        }

        public void ObtainActionPoints(int metabolism)
        {
            ActionPoints += metabolism * 10;
        }

        // Action points of the creature the fsm belongs to
        public int ActionPoints { get; private set; }
        public IState CurrentState { get; private set; }
        Dictionary<IState, List<Connection>> machine; 
    }
}
