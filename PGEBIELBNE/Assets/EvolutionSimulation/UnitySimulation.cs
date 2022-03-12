using System;
using System.Collections.Generic;
using EvolutionSimulation;
using EvolutionSimulation.Utils;
using EvolutionSimulation.Genetics;
using EvolutionSimulation.Entities;

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
            CreatureChromosome.SetChromosome();
            UniverseParametersManager.ReadJSON();
            world = new World();
            world.Init(UserInfo.Size);
            //A minimum distance to leave in between species spawn points to give them some room.
            //Calculated based on the world size and amount of species to spawn, and then reduced by
            //a value to give room in the world and not fill it in a homogenous manner.
            int minSpawnDist = UserInfo.Size / UserInfo.Species / 5;

            //List with previous spawn positions, to know if a new spot is too close to another one used.
            List<Tuple<int, int>> spawnPositions = new List<Tuple<int, int>>();
            int x, y;

            for (int i = 0; i < UserInfo.Species; i++)
            {
                bool validPosition = true;
                do
                {
                    validPosition = true;
                    x = RandomGenerator.Next(0, UserInfo.Size);
                    y = RandomGenerator.Next(0, UserInfo.Size);

                    foreach (Tuple<int, int> p in spawnPositions)
                    {
                        Vector2Int dist = new Vector2Int(x - p.Item1, y - p.Item2);
                        if (world.map[x, y].isWater || dist.Magnitude() < minSpawnDist)
                        {
                            validPosition = false;
                            break;
                        }
                    }
                }
                while (!validPosition);

                //The specified amount of individuals of each species is created.
                Animal a = world.CreateCreature<Animal>(x, y);
                for (int j = 1; j < UserInfo.Individuals; j++)
                {
                    world.CreateCreature<Animal>(x, y, a.chromosome, a.speciesName);
                }

                //The new position is added to the list of used.
                spawnPositions.Add(new Tuple<int, int>(x, y));
            }
        }

        /// <summary>
        /// Perfoms x years of simulation
        /// </summary>
        public void Run()
        {
            int nYears = 0;
            while (nYears < UserInfo.Years) { 
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
        /// Call before Init
        /// Sets the parameters for the initial simulation before visualization
        /// </summary>
        /// <param name="years"> Number of years to simulate </param>
        /// <param name="animals"> Number of animals that are initially created </param>
        public void SetInitialParameters(int size, int years, int species, int individuals, string dataDirectory, string exportDirectory)
        {
            UserInfo.SetInformation(size, years, species, individuals, dataDirectory, exportDirectory);
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
        int initialAnimals;

        List<IListener<World>> world_listeners;
    }
}
