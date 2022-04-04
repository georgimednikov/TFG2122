using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvolutionSimulation
{
    /// <summary>
    /// Common interface for simulations
    /// </summary>
    public interface ISimulation
    {
        /// <summary>
        /// Starts the simualtion and keeps it running until the exit condition is met
        /// </summary>
        void Run();

    }
}
