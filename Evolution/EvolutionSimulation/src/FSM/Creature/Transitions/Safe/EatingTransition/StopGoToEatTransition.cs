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
            if (creature.IsExhausted())
                return true;

            // If the creature is really hunger continue going to eating objective
            if (creature.IsVeryHunger())
                return false;

            // If the creature is really thirsty stop going to eating objective
            if (creature.IsVeryThirsty())
                return true;

            return false;
        }

        public override string ToString()
        {
            return "StopGoToEatTransition";
        }

    }
}
