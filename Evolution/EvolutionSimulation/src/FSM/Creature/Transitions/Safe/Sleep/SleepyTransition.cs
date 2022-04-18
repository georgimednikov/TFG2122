namespace EvolutionSimulation.FSM.Creature.Transitions
{
    /// <summary>
    /// The creature try to sleep in a safe place but 
    /// if it is exhausted, it will sleep anywhere
    /// Wander/Explore -> Sleeping
    /// </summary>
    class SleepyTransition : CreatureTransition
    {
        public SleepyTransition(Entities.Creature creature)
        {
            this.creature = creature;
        }

        public override bool Evaluate()
        {
            return creature.IsExhausted() && !creature.IsVeryHungry() && !creature.IsVeryThirsty();
        }

        public override string ToString()
        {
            return "SleepyTransition";
        }
    }
}
