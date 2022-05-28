using System.Collections.Generic;
using EvolutionSimulation;
using UnityEngine;
namespace UnitySimulation
{
    /// <summary>
    /// Evolution simulation adapted to the unity environment. 
    /// The additional functionality added are getter methods and an
    /// additional step function to be called after the simulation has runned 
    /// for the setted years.
    /// </summary>
    public class UnitySimulation : Simulation
    {
        /// <summary>
        /// Performs a step of the simulation and updates every listener with 
        /// the state of the world after the step is performed.
        /// This function is used after the simulation has been running for x
        /// </summary>
        public void SimulateStep()
        {
            world.Tick();
            SimulationEnd = world.Creatures.Count < 1;
        }

        /// <summary>
        /// The unity simulation its just to show off the system,
        /// no data needs to be exported at the end of the simulation.
        /// So the End method is overrided to just check if there are any creatures
        /// alive after the initial simulation.
        /// </summary>
        protected override void End()
        {
            SimulationEnd = world.Creatures.Count < 1;
        }

        /// <summary>
        /// Get the simulation current tick
        /// </summary>
        public int GetCurrentTicks()
        {
            return currentTick;
        }

        /// <summary>
        /// Get the ticks that correspond to one simulation day
        /// </summary>
        public int GetTicksInDay()
        {
            return World.ticksHour * World.hoursDay;
        }

        /// <summary>
        /// Property to acces the world of the simulation
        /// </summary>
        public World World { get => world; }

        /// <summary>
        /// Flag to indicate the end of the simulation
        /// </summary>
        public bool SimulationEnd;
    }
}
