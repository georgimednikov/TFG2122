using System;

namespace EvolutionSimulation.FSM.Creature.States
{
    /// <summary>
    /// The creature is sleeping, he recover Rest
    /// </summary>
    class Sleeping : CreatureState
    {
        public Sleeping(Entities.Creature c) : base(c) { creature = c; }

        // This move is energy netural, costing the same nergy that is obtained in a tick
        public override int GetCost()
        {
            return UniverseParametersManager.parameters.sleepingCostMultiplier * creature.stats.Metabolism;
        }

        // Increases current rest
        public override void Action()
        {
            Console.WriteLine("Sleeping action");

            creature.stats.CurrRest += creature.stats.RestRecovery;
            if (creature.stats.CurrRest > creature.stats.MaxRest)
                creature.stats.CurrRest = creature.stats.MaxRest;
        }

        public override string ToString()
        {
            return "SleepingState";
        }
    }
}
