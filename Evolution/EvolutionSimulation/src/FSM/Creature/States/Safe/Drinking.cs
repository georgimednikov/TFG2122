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
            creature.stats.CurrHydration += creature.stats.HydrationExpense * UniverseParametersManager.parameters.drinkingMultiplier;
            if (creature.stats.CurrHydration > creature.stats.MaxHydration)
            {
                creature.stats.CurrHydration = creature.stats.MaxHydration;
                //TODO: Mirar los valores cuando se llama a SafeWaterSpotFound SafePlantFound y CreateExperience
                creature.SafeWaterSpotFound(creature.chromosome.GetFeatureMax(Genetics.CreatureFeature.Aggressiveness) * UniverseParametersManager.parameters.experienceMaxAggresivenessMultiplier);
            }
            Console.WriteLine(creature.speciesName + " DRINKS (" + creature.x + ", " + creature.y + ")");
        }

        public override string ToString()
        {
            return "DrinkingState";
        }
    }
}
