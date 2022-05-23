namespace EvolutionSimulation.FSM.Creature.Transitions
{
    /// <summary>
    /// Checks if the creature is tired and know a safe place
    /// Wander -> Explore
    /// </summary>
    class GoToSafeTemperaturePlaceExploreTransition : CreatureTransition
    {
        public GoToSafeTemperaturePlaceExploreTransition(Entities.Creature creature)
        {
            this.creature = creature;
        }

        public override bool Evaluate()
        {
            return !creature.CheckTemperature(creature.x, creature.y) && creature.SafeTemperaturePosition() == null;
        }

        public override string ToString()
        {
            return "GoToSafeTemperaturePlaceExploreTransition";
        }

    }
}
