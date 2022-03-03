using UnityEngine;
using EvolutionSimulation;
using System;
using System.Collections.Generic;

namespace EvolutionSimulation.Unity
{
    // TODO: a ver esto puede estar en el propio SimulationManager, pero por si luego usamos 
    // el propio Simulation de la dll. La cosa esque ese no tiene un Step.
    public class UnitySimulation : ISimulation
    {
        public void Init()
        {
            world = new World();
            world.Init(32);
        }

        /// <summary>
        /// Perfoms x years of simulation
        /// </summary>
        public void Run()
        {
            int nYears = 0;
            while (nYears <= yearsToSimulate) { 
                world.Tick();
                // TODO: No se suma un anio cada tick pero para probar
                nYears++;
            }
        }

        /// <summary>
        /// Performs a step of the simulation
        /// </summary>
        public void Step()
        {
            world.Tick();
            //foreach
        }



        World world;
        public int yearsToSimulate;

        List<IObserver<SimulationInfo>> observers;
    }
}
