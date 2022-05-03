using System;
using System.Collections.Generic;
using EvolutionSimulation;
using EvolutionSimulation.Utils;
using EvolutionSimulation.Genetics;
using EvolutionSimulation.Entities;

namespace UnitySimulation
{
    public class UnitySimulation : Simulation, ISubject<World> 
    {

        public override void Init(int years, int species, int individuals, string dataDir, string exportDir, WorldGenConfig config)
        {
            base.Init(years, species, individuals, dataDir, exportDir, config);
            generateWorld.SetWorld(world);
            generateWorld.MapGen();
        }

        public override void Init(int years, int species, int individuals, string uniParamsFile = null, string chromosomeFile = null, string abilitiesFile = null, string sGeneWeightFile = null, string worldFile = null, string highMap = null, string exportDir = null)
        {
            base.Init(years, species, individuals, uniParamsFile, chromosomeFile, abilitiesFile, sGeneWeightFile, worldFile, highMap, exportDir);
            generateWorld.SetWorld(world);
            generateWorld.MapGen();
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

        public GenerateWorld generateWorld { private get; set; }

        List<IListener<World>> world_listeners = new List<IListener<World>>();
    }
}
