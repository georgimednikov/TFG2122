using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoritmoEvolutivo
{
    /// <summary>
    /// Common interface for simulations
    /// </summary>
    public interface ISimulation
    {
        /// <summary>
        /// Initialization of the simulation
        /// TODO: Input parameters through text file
        /// </summary>
        void Init();

        /// <summary>
        /// Starts the simualtion and keeps it running until the exit condition is met
        /// </summary>
        void Run();

        void Render();
    }
}
