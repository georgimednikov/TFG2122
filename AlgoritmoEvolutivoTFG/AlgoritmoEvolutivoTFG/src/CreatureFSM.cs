using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stateless;


namespace AlgoritmoEvolutivo
{
    /// <summary>
    /// IDs for the State Machine
    /// </summary>
    public enum StateID
    {
        Idle,
        Moving,
        Alive,
        Dead
    }
    public enum TriggerID
    {
        Moves,
        Stops,
        Dies
    }
}
