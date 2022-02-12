using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvolutionSimulation
{
    //TODO: Probablemente las interacciones no sean con esto
    /// <summary>
    /// Interface to represent an object that can be interacted with
    /// </summary>
    /// <typeparam name="T"> Objects which can interact with this object </typeparam>
    interface IInteractable<T>
    {
        /// <summary>
        /// The action that this object executes when the other T object interacts whith this
        /// </summary>
        void OnInteract(T other);
    }
}
