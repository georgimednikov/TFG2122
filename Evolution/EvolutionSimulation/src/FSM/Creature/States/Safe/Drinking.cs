using System;

namespace EvolutionSimulation.FSM.Creature.States
{
    class Drinking : CreatureState
    {
        public Drinking(Entities.Creature c) : base(c) { creature = c; }

        public override void OnEntry()
        {
            creature.stats.ActionPerceptionPercentage = UniverseParametersManager.parameters.actionPerceptionPercentage;
        }

        public override int GetCost()
        {
            return UniverseParametersManager.parameters.drinkingCostMultiplier * creature.stats.Metabolism;
        }

        public override void Action()
        {
            creature.stats.CurrHydration += creature.stats.HydrationExpense * UniverseParametersManager.parameters.drinkingMultiplier;
            if (creature.stats.CurrHydration > creature.stats.MaxHydration)
            {
                creature.stats.CurrHydration = creature.stats.MaxHydration;
                //TODO: Mirar los valores cuando se llama a SafeWaterSpotFound SafePlantFound y CreateExperience
                creature.SafeWaterSource();
            }
        }

        public override void OnExit()
        {
            creature.stats.ActionPerceptionPercentage = 1;
        }

        public override string ToString()
        {
            return "DrinkingState";
        }

        /// <summary>
        /// Text used to display state in simulation
        /// </summary>
        public override string GetInfo()
        {
            return creature.speciesName + " with ID: " + creature.ID + " DRINKS AT (" + creature.x + ", " + creature.y + ", " + creature.creatureLayer + ")";
        }
    }
}
