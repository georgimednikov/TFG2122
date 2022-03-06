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
            return creature.wantMate && creature.GetClosestPossibleMatePosition() == null;
        }

        public override string ToString()
        {
            return "MateExploreTransition";
        }

    }
}
