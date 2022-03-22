using System;

namespace EvolutionSimulation.FSM.Creature.States
{
    /// <summary>
    /// The creature is sleeping, he recover Rest
    /// </summary>
    class Sleeping : CreatureState
    {
        public Sleeping(Entities.Creature c) : base(c) { creature = c; }

        public override void OnEntry()
        {
            creature.stats.ActionPerceptionPercentage = UniverseParametersManager.parameters.actionPerceptionPercentage;
        }

        // This move is energy netural, costing the same energy that is obtained in a tick
        public override int GetCost()
        {
            return UniverseParametersManager.parameters.sleepingCostMultiplier * creature.stats.Metabolism;
        }

        // Increases current rest
        public override void Action()
        {
            creature.stats.CurrRest += creature.stats.RestRecovery;
            if (creature.stats.CurrRest > creature.stats.MaxRest)
            {
                creature.stats.CurrRest = creature.stats.MaxRest;
                creature.CreateDanger();
            }
            Console.WriteLine(creature.speciesName + " SLEEPS");
        }

        public override void OnExit()
        {
            creature.stats.ActionPerceptionPercentage = 1;
        }

        public override string ToString()
        {
            return "SleepingState";
        }
    }
}
