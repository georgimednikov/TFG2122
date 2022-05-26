using System.Collections.Generic;
using EvolutionSimulation;
using UnityEngine;
namespace UnitySimulation
{
    /// <summary>
    /// Evolution simulation adapted to the unity environment. 
    /// </summary>
    public class UnitySimulation : Simulation, ISubject<World>
    {
        public override void Init(int years, int species, int individuals, string dataDir, string exportDir, WorldGenConfig config)
        {
            base.Init(years, species, individuals, dataDir, exportDir, config);
            GenerateWorld.SetWorld(world);
            GenerateWorld.MapGen();
            SimulationEnd = false;
        }

        public override void Init(int years, int species, int individuals, string exportDir, WorldGenConfig worldConfig, string uniParamsFile = null, string chromosomeFile = null, string abilitiesFile = null, string sGeneWeightFile = null, string worldFile = null, string highMap = null)
        {
            base.Init(years, species, individuals, exportDir, worldConfig, uniParamsFile, chromosomeFile, abilitiesFile, sGeneWeightFile, worldFile, highMap);
            GenerateWorld.SetWorld(world);
            GenerateWorld.MapGen();
            SimulationEnd = false;
        }

        /// <summary>
        /// Performs a step of the simulation and updates every listener with 
        /// the state of the world after the step is performed.
        /// </summary>
        public void SimulateStep()
        {
            world.Tick();
            SimulationEnd = world.Creatures.Count < 1;
            // Notify every listener after a step is simulated
            foreach (IListener<World> listener in world_listeners)
            {
                listener.OnNotify(world);
                Debug.Log(world.Creatures.Count);
            }
        }

        /// <summary>
        /// The unity simulation its just to show off the system,
        /// no data needs to be exported at the end of the simulation.
        /// So the End method is overrided to just notify the listeners when the 
        /// </summary>
        protected override void End()
        {
            SimulationEnd = world.Creatures.Count < 1;
        }

        public int GetCurrentTicks()
        {
            return currentTick;
        }
        public int GetTicksInDay()
        {
            return World.ticksHour * World.hoursDay;
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

        /// <summary>
        /// World generation
        /// </summary>
        public WorldGenerator GenerateWorld { private get; set; }

        /// <summary>
        /// Internal world information listeners
        /// </summary>
        List<IListener<World>> world_listeners = new List<IListener<World>>();

        public bool SimulationEnd;
    }
}
