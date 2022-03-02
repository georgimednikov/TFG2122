namespace EvolutionSimulation.FSM.Creature.Transitions
{
    /// <summary>
    /// Checks if the creature is tired and do not know a safe place
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
            
            return creature.IsTired() && creature.GetClosestSafePlace() == null;
        }

        public override string ToString()
        {
            return "SafePlaceExploreTransition";
        }

    }
}
