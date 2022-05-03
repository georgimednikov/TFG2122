using System;

namespace EvolutionSimulation.FSM.Creature.States
{
    /// <summary>
    /// The creature is sleeping, he recover Rest
    /// </summary>
    class Sleeping : CreatureState
    {
        public Sleeping(Entities.Creature c) : base(c) { 
            creature = c; 
            originalEnergyExpense = creature.stats.EnergyExpense;
            originalHydrationExpense = creature.stats.HydrationExpense;
            originalRestExpense = creature.stats.RestExpense;
        }
        float originalEnergyExpense, originalHydrationExpense, originalRestExpense;
        public override void OnEntry()
        {
            base.OnEntry();
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
            if (creature.stats.RestExpense >= creature.stats.RestRecovery)
                throw new Exception("Problema en sleeping, se cansa más de lo que recupera");
            creature.stats.CurrRest += creature.stats.RestRecovery;
            if (creature.stats.CurrRest > creature.stats.MaxRest)
            {
                creature.stats.CurrRest = creature.stats.MaxRest;
                creature.CreateSafety();
            }
        }

        public override void OnExit()
        {
            creature.stats.ActionPerceptionPercentage = 1;

            // Waking up reestablishes the expenses to normal
            creature.stats.EnergyExpense = originalEnergyExpense;
            creature.stats.HydrationExpense = originalHydrationExpense;
            creature.stats.RestExpense = originalRestExpense;
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
            return creature.speciesName + " with ID: " + creature.ID + " SLEEPS AT (" + creature.x + ", " + creature.y + ", " + creature.creatureLayer + ")";
        }
    }
}
