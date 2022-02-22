using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvolutionSimulation.Entities
{
    //TODO: Podemos quitar el tipo T, y usar IEntity, pero habria que comparar los tipos
    /// <summary>
    /// Interface to represent an object that can be interacted with
    /// </summary>
    /// <typeparam name="T"> Entities that can interact with this object </typeparam>
    interface IInteractable<T> where T : IEntity
    {
        /// <summary>
        /// Manages a 'type' interaction between the interacter T, and this object
        /// </summary>
        void ReceiveInteraction(T interacter, Interactions type);
    }

    /// <summary>
    /// Interaction types
    /// </summary>
    public enum Interactions { 
        attack, 
        initmidate, 
        mate,
        eat,
        poison
    }
}
