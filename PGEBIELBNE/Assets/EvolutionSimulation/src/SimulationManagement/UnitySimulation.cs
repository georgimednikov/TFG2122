using System;
using System.Collections.Generic;
using EvolutionSimulation;
using EvolutionSimulation.Utils;

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
        }

        public override void Init(int years, int species, int individuals, string uniParamsFile = null, string chromosomeFile = null, string abilitiesFile = null, string sGeneWeightFile = null, string worldFile = null, string highMap = null, string exportDir = null)
        {
            base.Init(years, species, individuals, uniParamsFile, chromosomeFile, abilitiesFile, sGeneWeightFile, worldFile, highMap, exportDir);
            GenerateWorld.SetWorld(world);
            GenerateWorld.MapGen();
        }

        /// <summary>
        /// Performs a step of the simulation and updates every listener with 
        /// the state of the world after the step is performed.
        /// </summary>
        public void SimulateStep()
        {
            world.Tick();

            // Notify every listener after a step is simulated
            foreach (IListener<World> listener in world_listeners)
                listener.OnNotify(world);
        }

        /// <summary>
        /// The unity simulation its just to show off the system,
        /// no data needs to be exported at the end of the simulation.
        /// So the End method is overrided to do nothing.
        /// </summary>
        protected override void End()
        {
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

        /// <summary>
        /// World generation
        /// </summary>
        public GenerateWorld GenerateWorld { private get; set; }


        // Simulation evolutionSimulation; TODO: se podria hacer asi pero hay que revisar los listeners o la info que se quiere dar desde fuera

        /// <summary>
        /// Internal world information listeners
        /// </summary>
        List<IListener<World>> world_listeners = new List<IListener<World>>();
    }
}
