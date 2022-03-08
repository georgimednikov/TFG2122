using System;

namespace EvolutionSimulation.FSM.Creature.States
{
    class Drinking : CreatureState
    {
        public Drinking(Entities.Creature c) : base(c) { creature = c; }

        public override int GetCost()
        {
            return UniverseParametersManager.parameters.drinkingCostMultiplier * creature.stats.Metabolism;
        }

        public override void Action()
        {
            Console.WriteLine("Drinking action");

            creature.stats.CurrHydration += creature.stats.HydrationExpense * 5;
            if (creature.stats.CurrHydration > creature.stats.MaxHydration)
            {
                creature.stats.CurrHydration = creature.stats.MaxHydration;
                //TODO: Mirar los valores cuando se llama a SafeWaterSpotFound SafePlantFound y CreateExperience
                creature.SafeWaterSpotFound(creature.chromosome.GetFeatureMax(Genetics.CreatureFeature.Aggressiveness) * UniverseParametersManager.parameters.experienceMaxAggresivenessMultiplier);
            }
        }

        public override string ToString()
        {
            return "DrinkingState";
        }
    }
}
