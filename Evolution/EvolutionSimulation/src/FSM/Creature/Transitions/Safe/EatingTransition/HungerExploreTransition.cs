namespace EvolutionSimulation.FSM.Creature.Transitions
{
    /// <summary>
    /// Checks if a creature need to eat and do not know where to go
    /// Wander -> Explore
    /// </summary>
    class HungerExploreTransition : CreatureTransition
    {

        public HungerExploreTransition(Entities.Creature creature)
        {
            this.creature = creature;
        }

        public override bool Evaluate()
        {
            if(creature.stats.CurrEnergy > creature.stats.hungerThreshold * creature.stats.MaxEnergy) return false;

            return !creature.HasEatingObjective(); 
        }

        public override string ToString()
        {
            return "HungerExploreTransition";
        }

    }
}
