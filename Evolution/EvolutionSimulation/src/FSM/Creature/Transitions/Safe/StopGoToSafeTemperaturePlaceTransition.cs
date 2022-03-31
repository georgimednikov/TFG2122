namespace EvolutionSimulation.FSM.Creature.Transitions
{
    /// <summary>
    /// Checks if the creature is in a safe temperature position
    /// GoToSafeTemperaturePlace -> Wander 
    /// </summary>
    class StopGoToSafeTemperaturePlaceTransition : CreatureTransition
    {
        public StopGoToSafeTemperaturePlaceTransition(Entities.Creature creature)
        {
            this.creature = creature;
        }

        public override bool Evaluate()
        {
            return creature.CheckTemperature(creature.x, creature.y);
        }

        public override string ToString()
        {
            return "StopGoToSafeTemperaturePlaceTransition";
        }

    }
}
