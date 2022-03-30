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

            // Sleeping reduces the expense of being active
            creature.stats.EnergyExpense *= UniverseParametersManager.parameters.sleepingExpenseReduction;
            creature.stats.HydrationExpense *= UniverseParametersManager.parameters.sleepingExpenseReduction;
            creature.stats.RestExpense *= UniverseParametersManager.parameters.sleepingExpenseReduction;
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
        }

        public override void OnExit()
        {
            creature.stats.ActionPerceptionPercentage = 1;

            // Waking up reestablishes the expenses to normal
            creature.stats.EnergyExpense /= UniverseParametersManager.parameters.sleepingExpenseReduction;
            creature.stats.HydrationExpense /= UniverseParametersManager.parameters.sleepingExpenseReduction;
            creature.stats.RestExpense /= UniverseParametersManager.parameters.sleepingExpenseReduction;
        }

        public override string ToString()
        {
            return "SleepingState";
        }

        /// <summary>
        /// Text used to display state in simulation
        /// </summary>
        public override string GetInfo()
        {
            return creature.speciesName + " with ID: " + creature.ID + " SLEEPS AT (" + creature.x + ", " + creature.y + ")";
        }
    }
}
