namespace EvolutionSimulation.FSM.Creature.Transitions
{
    /// <summary>
    /// Checks if the creature do not know where to eat or 
    /// the creature has other needs more important
    /// Go to eat -> Wander
    /// </summary>
    class StopGoToEatTransition : CreatureTransition
    {
        public StopGoToEatTransition(Entities.Creature creature)
        {
            this.creature = creature;
        }

        public override bool Evaluate()
        {
            if (!creature.HasEatingObjective()) return true;

            // Max priority to sleep
            if (creature.stats.CurrRest < creature.stats.exhaustThreshold * creature.stats.MaxRest)
                return true;

            // If the creature is really hunger continue going to eating objective
            if (creature.stats.CurrEnergy < creature.stats.veryHungerThreshold * creature.stats.MaxEnergy)
                return false;

            // If the creature is really thirsty stop going to eating objective
            if (creature.stats.CurrHydration < creature.stats.veryThirstyThreshold * creature.stats.MaxHydration)
                return true;

            return false;
        }

        public override string ToString()
        {
            return "StopGoToEatTransition";
        }

    }
}
