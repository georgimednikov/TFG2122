using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvolutionSimulation.FSM
{
    // TODO: Estos enums se pueden ir
    /// <summary>
    /// State IDs for the State Machine
    /// </summary>
    public enum StateID
    {
        Idle,
        Moving,
        Alive,
        Dead
    }

    /// <summary>
    /// Trigger IDs for the State Machine
    /// </summary>
    public enum TriggerID
    {
        Moves,
        Stops,
        Dies
    }
    public interface IState 
    {
        void Action();
    }
    public interface ITransition
    {
        bool Evaluate();
    }
}
