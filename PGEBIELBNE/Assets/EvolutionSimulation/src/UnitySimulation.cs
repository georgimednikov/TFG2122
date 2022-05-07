using System;
using System.Collections.Generic;
using EvolutionSimulation;
using EvolutionSimulation.Utils;

namespace UnitySimulation
{
    public class UnitySimulation : Simulation, ISubject<World> 
    {

        public override void Init(int years, int species, int individuals, string dataDir, string exportDir, WorldGenConfig config)
        {
            base.Init(years, species, individuals, dataDir, exportDir, config);
            GenerateWorld.SetWorld(world);
            GenerateWorld.MapGen();
        }

        public override void Init(int years, int species, int individuals, string uniParamsFile = null, string chromosomeFile = null, string abilitiesFile = null, string sGeneWeightFile = null, string worldFile = null, string highMap = null, string exportDir = null)
        {
            base.Init(years, species, individuals, uniParamsFile, chromosomeFile, abilitiesFile, sGeneWeightFile, worldFile, highMap, exportDir);
            GenerateWorld.SetWorld(world);
            GenerateWorld.MapGen();
        }

        /// <summary>
        /// Performs a step of the simulation
        /// </summary>
        public void SimulateStep()
        {
            world.Tick(currentTick);    //TODO: el current tick es otro si se ponen anios de evolucion al principio
            currentTick++;

            // Notify every listener after a step is simulated
            foreach (IListener<World> listener in world_listeners)
                listener.OnNotify(world);
        }

        // TODO: no exportar siempre?
        protected override void End()
        {
            //EndTracker();
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

        public int GetCurrentTicks()
        {
            return currentTick;
        }
        public int GetTicksInDay()
        {
            return World.ticksHour * World.hoursDay;
        }

        public GenerateWorld GenerateWorld { private get; set; }
        public World World { get => world; }

        List<IListener<World>> world_listeners = new List<IListener<World>>();
    }
}
