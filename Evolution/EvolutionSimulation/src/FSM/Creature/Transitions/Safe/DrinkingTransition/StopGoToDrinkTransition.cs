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
            // If the creature does not know where is a water position (the creature is going to the water and suddenly forgot it)
            if (creature.WaterPosition() == null)
                return true;

            // Max priority to sleep
            if (creature.IsExhausted())
                return true;

            // If the creature is thirsty continue going to drinking objective
            if (creature.IsThirsty())
                return false;

            // If the creature is really hunger stop going to drinking objective
            if (creature.IsVeryHungry())
                return true;

            return false;
        }

        public override string ToString()
        {
            return "StopGoToDrinkTransition";
        }

    }
}
