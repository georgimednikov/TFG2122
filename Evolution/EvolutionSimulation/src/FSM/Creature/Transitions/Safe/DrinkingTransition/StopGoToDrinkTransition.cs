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
            if (creature.IsExhausted())
                return true;

            // If the creature is really thirsty continue going to drinking objective
            if (creature.IsVeryThirsty())
                return false;

            // If the creature is really hunger stop going to drinking objective
            if (creature.IsVeryHunger())
                return true;

            return false;
        }

        public override string ToString()
        {
            return "StopGoToDrinkTransition";
        }

    }
}
