namespace EvolutionSimulation.FSM.Creature.Transitions
{
    /// <summary>
    /// Checks if the nearestMate (female) doesn't want to mate
    /// TryToMate -> Wander
    /// </summary>
    class StopTryMateTransition : CreatureTransition
    {
        public StopTryMateTransition(Entities.Creature creature)
        {
            this.creature = creature;
        }

        public override bool Evaluate()
        {
            return !creature.Mate() || !creature.mating || creature.HasNecesities();
        }

        public override string ToString()
        {
            return "StopTryMateTransition";
        }

    }
}
