namespace EvolutionSimulation.FSM.Creature.Transitions
{
    /// <summary>
    /// Checks if a creature needs to find a safe and does not know where to go
    /// Wander -> Explore
    /// </summary>
    class SafePlaceExploreTransition : CreatureTransition
    {
        public SafePlaceExploreTransition(Entities.Creature creature)
        {
            this.creature = creature;
        }

        public override bool Evaluate()
        {
            return creature.IsTired() && creature.SafePosition() == null;
        }

        public override string ToString()
        {
            return "MateExploreTransition";
        }

    }
}
