﻿using System;
using System.Drawing;
using System.Collections.Generic;
using EvolutionSimulation.Entities;
using System.Numerics;
using Telemetry;
using Telemetry.Events;

namespace EvolutionSimulation
{
    /// <summary>
    /// Simulation of the evolution that provides more information about
    /// the simulation and the events of the simulation.
    /// </summary>
    public class EventSimulation : Simulation
    {
        public int TotalTicks { get => totalTicks; }
        public int CurrentTick { get => currentTick; }
        public World World { get => world; }
        public int YearTicks { get; private set; }
        public int Births { get; private set; }
        private int prevCreatures;

        public event Action<EventSimulation> OnSimulationBegin;
        public event Action<EventSimulation> OnSimulationStep;
        public event Action<EventSimulation> OnSimulationEnd;

        // TODO: quitar esto si no hacemos apocalipsis
        public int ApocalypseCount { get => apocalypseCount; }
        public Action<EventSimulation> OnApocalypse;

        /// <summary>
        /// OnSimulationStart events are performed.
        /// </summary>
        override protected void Begin()
        {
            base.Begin();
            YearTicks = world.YearToTick(1.0f);
            prevCreatures = world.Creatures.Count;
            Births = prevCreatures = 0;
            OnSimulationStep += (s) =>
            {
                if (prevCreatures < world.Creatures.Count)
                    Births += world.Creatures.Count - prevCreatures;
                prevCreatures = world.Creatures.Count;
            };

            OnSimulationBegin?.Invoke(this);
        }

        /// <summary>
        /// OnSimulationStep events are performed.
        /// </summary>
        /// <returns> False if no creatures remain, true otherwise </returns>
        override protected bool Step()
        {
            bool ret = base.Step();
            OnSimulationStep.Invoke(this);  // no nullCheck because it has at least the birth check action
            return ret;
        }

        /// <summary>
        /// OnSimulationEnd events are performed
        /// </summary>
        override protected void End()
        {
            OnSimulationEnd?.Invoke(this);
            base.End();
        }

        //TODO: apocalipsis y esas cosas
        override protected void Apocalypse()
        {
            OnApocalypse?.Invoke(this);
            base.Apocalypse();
        }
    }
}
