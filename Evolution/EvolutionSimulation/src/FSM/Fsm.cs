using System;
using System.Collections.Generic;

namespace EvolutionSimulation.FSM
{
    /// <summary>
    /// State Machine implementation, where the states have an associated cost.
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
        /// </summary>
        public void AddTransition(IState og, ITransition t, IState dest)
        {
            if (!machine.ContainsKey(og))   machine.Add(og, new List<Connection>());
            if (!machine.ContainsKey(dest)) machine.Add(dest, new List<Connection>());
            machine[og].Add(new Connection(t, dest));
        }

        /// <summary>
        /// Evaluates transitions and modifies the current state if any transition triggers.
        /// Returns the cost of the target state.
        /// </summary>
        public int EvaluateCost()
        {
            foreach (Connection c in machine[CurrentState])
            {
                if (c.Transition.Evaluate())
                {
                    //CurrentState.OnExit();
                    CurrentState = c.OutState;
                    //CurrentState.OnEntry();
                    break;
                }
            }
            return CurrentState.GetCost();
        }

        public string ExportToDotGraph()
        {
            return "Fsm DOT Graph export not implemented yet";
        }

        public IState CurrentState { get; private set; }
        Dictionary<IState, List<Connection>> machine;
    }
}
