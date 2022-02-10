using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvolutionSimulation.FSM
{
    /// <summary>
    /// State machine state interface
    /// </summary>
    public interface IState 
    {
        /// <summary>
        /// Returns if the state can perform 
        /// its action with the FSm's current AP.
        /// </summary>
        /// <param name="actionPoints"></param>
        bool canPerformAction(int actionPoints);

        /// <summary>
        /// Action which is executed in the state
        /// </summary>
        int Action();
    }

    /// <summary>
    /// State machine transition interface
    /// </summary>
    public interface ITransition
    {
        /// <summary>
        /// Evaluates if the transition is fullfilled
        /// </summary>
        bool Evaluate();
    }
}
