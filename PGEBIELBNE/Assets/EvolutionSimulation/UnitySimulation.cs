using System.Collections.Generic;
using EvolutionSimulation;
using EvolutionSimulation.Utils;

namespace UnitySimulation
{
    // TODO: a ver esto puede estar en el propio SimulationManager, pero por si luego usamos 
    // el propio Simulation de la dll. La cosa esque ese no tiene un Step.
    // TODO: No ser un ISubject del world directamente, ahora para probar
    public class UnitySimulation : ISimulation, ISubject<World> 
    {
        public void Init()
        {
            world_listeners = new List<IListener<World>>();
            world = new World();
            world.Init(32);
            for (int i = 0; i < initialAnimals; i++)
            {
                EvolutionSimulation.Entities.Animal c = world.CreateCreature<EvolutionSimulation.Entities.Animal>(5, 5);
            }
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

            // Notify every listener after a step is simulated
            foreach (IListener<World> listener in world_listeners)
                listener.OnNotify(world);
        }

        /// <summary>
        /// Sets the parameters for the initial simulation before visualization
        /// </summary>
        /// <param name="years"> Number of years to simulate </param>
        /// <param name="animals"> Number of animals that are initially created </param>
        public void SetInitialParameters(int years, int animals)
        {
            yearsToSimulate = years;
            initialAnimals = animals;
        }

        public bool Subscribe(IListener<World> listener)
        {
            if (world_listeners.Contains(listener)) return false;
            world_listeners.Add(listener);
            return true;
        }

        public bool Unsubscribe(IListener<World> listener)
        {
            return world_listeners.Remove(listener);
        }

        World world;
        int yearsToSimulate;
        int initialAnimals;

        List<IListener<World>> world_listeners;
    }
}
