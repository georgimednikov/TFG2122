namespace EvolutionSimulation.FSM.Creature.Transitions
{
    /// <summary>
    /// Checks if the creature is not in a safe temperature position and does know 
    /// a safe temperature position
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
            return !creature.CheckTemperature(creature.x, creature.y) &&
                creature.SafeTemperaturePosition() != null;
        }

        public override string ToString()
        {
            return "GoToSafeTemperaturePlaceTransition";
        }

    }
}
