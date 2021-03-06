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
            return (!creature.IsTired() && creature.PositionDanger(creature.x, creature.y) <= 0) || creature.SafePosition() == null ||
                (creature.SafePosition().x == creature.x && creature.SafePosition().y == creature.y);
        }

        public override string ToString()
        {
            return "StopGoToSafePlaceTransition";
        }

    }
}
