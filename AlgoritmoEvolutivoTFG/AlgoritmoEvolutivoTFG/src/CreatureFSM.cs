using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stateless;


namespace AlgoritmoEvolutivo
{
    /// <summary>
    /// IDs para la maquina de estados
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
