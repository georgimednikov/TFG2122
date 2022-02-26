namespace EvolutionSimulation.FSM.Creature.Transitions
{
    /// <summary>
    /// Checks if the creature do not know where to drink or 
    /// the creature has other needs more important
    /// Go to drink -> Wander
    /// </summary>
    class StopGoToDrinkTransition : CreatureTransition
    {
        public StopGoToDrinkTransition(Entities.Creature creature)
        {
            this.creature = creature;
        }

        public override bool Evaluate()
        {
            //TODO objetivo de beber
            //if (!creature.HasEatingObjective()) return true;

            // Max priority to sleep
            if (creature.stats.CurrRest < creature.stats.exhaustThreshold * creature.stats.MaxRest)
                return true;

            // If the creature is really thirsty continue going to drinking objective
            if (creature.stats.CurrHydration < creature.stats.veryThirstyThreshold * creature.stats.MaxHydration)
                return false;

            // If the creature is really hunger stop going to drinking objective
            if (creature.stats.CurrEnergy < creature.stats.veryHungerThreshold * creature.stats.MaxEnergy)
                return true;

            return false;
        }

        public override string ToString()
        {
            return "StopGoToDrinkTransition";
        }

    }
}
