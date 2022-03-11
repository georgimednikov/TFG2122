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
            return (creature.IsTired() && creature.DistanceToObjective(creature.GetClosestSafePlacePosition()) <= UniverseParametersManager.parameters.adjacentLength) || creature.IsExhausted();
        }

        public override string ToString()
        {
            return "SleepySafeTransition";
        }
    }
}
