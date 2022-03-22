namespace EvolutionSimulation.FSM.Creature.Transitions
{
    /// <summary>
    /// Checks if the creature is tired and know a safe place
    /// GoToSafePlace -> Wander
    /// </summary>
    class StopGoToSafePlaceTransition : CreatureTransition
    {
        public StopGoToSafePlaceTransition(Entities.Creature creature)
        {
            this.creature = creature;
        }

        public override bool Evaluate()
        {
            return !creature.IsTired() || creature.SafePosition() == null;
        }

        public override string ToString()
        {
            return "StopGoToSafePlaceTransition";
        }

    }
}
