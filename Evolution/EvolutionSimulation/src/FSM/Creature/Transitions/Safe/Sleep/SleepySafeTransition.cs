namespace EvolutionSimulation.FSM.Creature.Transitions
{
    /// <summary>
    /// The creature is in a safe place (probably with other creatures of his species)
    /// and want to sleep. If the creature is exhausted, it goes to sleep although it is 
    /// not in a safe place
    /// GoToSafePlace -> Sleeping
    /// </summary>
    class SleepySafeTransition : CreatureTransition
    {
        public SleepySafeTransition(Entities.Creature creature)
        {
            this.creature = creature;
        }

        /// <summary>
        /// Check is is tired and close to the safe place or is exhausted
        /// </summary>
        public override bool Evaluate()
        {
            int xObj= creature.GetClosestSafePlace().Item1, yObj = creature.GetClosestSafePlace().Item2;
            return (creature.IsTired() && creature.DistanceToObjective(xObj, yObj) <= 1)
                || creature.IsExhausted();
        }

        public override string ToString()
        {
            return "SleepySafeTransition";
        }
    }
}
