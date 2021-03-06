namespace EvolutionSimulation.FSM.Creature.Transitions
{
    /// <summary>
    /// Checks if a creature need to drink and do not know where to go
    /// Wander -> Explore
    /// </summary>
    class DrinkingExploreTransition : CreatureTransition
    {

        public DrinkingExploreTransition(Entities.Creature creature)
        {
            this.creature = creature;
        }

        public override bool Evaluate()
        {
            return creature.IsThirsty() && creature.WaterPosition() == null;
        }

        public override string ToString()
        {
            return "ThirstyExploreTransition";
        }

    }
}
