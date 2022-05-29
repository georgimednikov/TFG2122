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
            CurrentState = initialState = initalState;
        }

        /// <summary>
        /// Adds a transition from the original state to the destiny state
        /// </summary>
        public void AddTransition(IState og, ITransition t, IState dest)
        {
            if (!machine.ContainsKey(og)) machine.Add(og, new List<Connection>());
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
                    CurrentState.OnExit();
                    CurrentState = c.OutState;
                    CurrentState.OnEntry();
                    break;
                }
            }
            return CurrentState.GetCost();
        }

        /// <summary>
        /// Returns a string with the state machine for this creature exported, using DOT language
        /// To easily see it, use http://www.webgraphviz.com/
        /// </summary>
        public string ExportToDotGraph()
        {
            string accum = "";
            return "digraph FSM {\n\tsubgraph cluster_0 {\n\t\tlabel = FSM;\n" + ExportFSM(ref accum, this, 2) + "}";
        }

        /// <summary>
        /// Resets the state machine to the initial state
        /// </summary>
        public void Reset()
        {
            CurrentState.OnExit();
            CurrentState = initialState;
        }

        /// <summary>
        /// Triggers initial state onEntry event
        /// </summary>
        public void Start()
        {
            CurrentState = initialState;
            CurrentState.OnEntry();
        }

        /// <summary>
        /// Recursive method for exporting the fsm
        /// </summary>
        /// <param name="accum">Accumulated string for recursion</param>
        /// <param name="f">Which state machine should be exported (used for CompoundStates)</param>
        /// <param name="numRecursions">How deep we are in to set the correct amount of tabs</param>
        string ExportFSM (ref string accum, Fsm f, int numRecursions)
        {
            foreach (IState state in f.machine.Keys)
            {
                if (state is CompoundState)
                {
                    for (int i = 0; i < numRecursions; ++i) accum += "\t";
                    accum += "subgraph cluster_" + accum.Length + " {\n"; // Does not matter which number is behind cluster_, as long as it is not the same as another cluster_
                    //for (int i = 0; i < numRecursions + 1; ++i) accum += "\t";
                    //accum += "style=filled;\n"; 
                    for (int i = 0; i < numRecursions + 1; ++i) accum += "\t";
                    accum += "label = " + state.ToString() + ";\n";
                    ExportFSM(ref accum, (state as CompoundState).stateMachine, numRecursions + 1);
                }
                foreach (Connection c in f.machine[state])
                {
                    for (int i = 0; i < numRecursions; ++i) accum += "\t";
                    accum += state.ToString() + " -> " + c.OutState.ToString() + " [ label = " + "\"" + c.Transition.ToString() + "\"" + " ];\n";
                }
            }
            for (int i = 0; i < numRecursions - 1; ++i) accum += "\t";
            accum += "}\n";
            return accum;
        }

        public IState CurrentState { get; private set; }
        IState initialState;
        Dictionary<IState, List<Connection>> machine;
    }
}
