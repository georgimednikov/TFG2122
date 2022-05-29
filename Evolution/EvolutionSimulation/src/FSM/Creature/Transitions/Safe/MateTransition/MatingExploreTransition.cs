namespace EvolutionSimulation.FSM.Creature.Transitions
{
    /// <summary>
    /// Checks if a creature needs to find a mate and does not know where to go
    /// Wander -> Explore
    /// </summary>
    class MatingExploreTransition : CreatureTransition
    {
        public MatingExploreTransition(Entities.Creature creature)
        {
            this.creature = creature;
        }

        public override bool Evaluate()
        {
            return creature.stats.Gender != Genetics.Gender.Female
                && !creature.HasNecesities() && !creature.Mate();               
        }

        public override string ToString()
        {
            return "MateExploreTransition";
        }

    }
}
