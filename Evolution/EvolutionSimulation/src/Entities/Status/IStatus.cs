using EvolutionSimulation.Genetics;
using System;

namespace EvolutionSimulation.Entities.Status
{
    /// <summary>
    /// Interface detailing methods a status effect must have
    /// </summary>
    public interface IStatus
    {
        /// <summary>
        /// Performs the status' action and reduces its remaining duration
        /// </summary>
        /// <returns>If the status has expired</returns>
        bool OnTick();

        /// <summary>
        /// Performs an action when the effect is first applied
        /// </summary>
        void OnApply();

        /// <summary>
        /// Performs an action when the status is removed before its duration expires
        /// </summary>
        void OnRemove();
        
        /// <summary>
        /// Performs an action when the status' duration expires
        /// </summary>
        void OnExpire();
    }
}
