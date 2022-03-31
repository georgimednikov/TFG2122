namespace EvolutionSimulation.FSM.Creature.Transitions
{
    /// <summary>
    /// Checks if the creature is tired and know a safe place
    /// Wander -> GoToSafeTemperaturePlace
    /// </summary>
    class GoToSafeTemperaturePlaceTransition : CreatureTransition
    {
        public GoToSafeTemperaturePlaceTransition(Entities.Creature creature)
        {
            this.creature = creature;
        }

        public override bool Evaluate()
        {
            bool t = creature.SafeTemperaturePosition() != null;
            return !creature.CheckTemperature(creature.x, creature.y) &&
                creature.SafeTemperaturePosition() != null;
        }

        public override string ToString()
        {
            return "GoToSafePlaceTransition";
        }

    }
}
