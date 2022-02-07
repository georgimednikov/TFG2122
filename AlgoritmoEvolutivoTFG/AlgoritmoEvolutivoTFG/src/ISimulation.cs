using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoritmoEvolutivo
{
    /// <summary>
    /// Interfaz comun para las simulaciones de evolucion
    /// </summary>
    public interface ISimulation
    {
        /// <summary>
        /// Inicializacion de la simulacion
        /// TODO: Pasar archivo con datos iniciales
        /// </summary>
        void Init();

        /// <summary>
        /// Simula la evolucion
        /// </summary>
        void Run();

    }
}
