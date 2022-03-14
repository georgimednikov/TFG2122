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
        int GetCost();

        /// <summary>
        /// Action which is executed in the state
        /// </summary>
        void Action();

        /// <summary>
        /// Action executed when it is transitioned into
        /// </summary>
        void OnEntry();

        /// <summary>
        /// Action executed when the state transitions to other
        /// </summary>
        void OnExit();

        /// <summary>
        /// Additional information about this state
        /// </summary>
        string GetInfo();
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
