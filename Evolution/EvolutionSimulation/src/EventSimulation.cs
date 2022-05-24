using System;

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

        /// <summary>
        /// OnSimulationStart events are performed.
        /// </summary>
        override protected void Begin()
        {
            base.Begin();
            YearTicks = world.YearToTick(1.0f);
            prevCreatures = world.Creatures.Count;
            Births = 0;
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
            OnSimulationStep.Invoke(this);  // No nullCheck because it has at least the birth check action
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
    }
}
