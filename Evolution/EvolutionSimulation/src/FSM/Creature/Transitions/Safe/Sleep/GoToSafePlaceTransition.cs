namespace EvolutionSimulation.FSM.Creature.Transitions
{
    /// <summary>
    /// Checks if the creature is tired and know a safe place
    /// Wander -> GoToSafePlace
    /// </summary>
    class GoToSafePlaceTransition : CreatureTransition
    {
        public GoToSafePlaceTransition(Entities.Creature creature)
        {
            this.creature = creature;
        }

        public override bool Evaluate()
        {
            return creature.SafePosition() != null &&
                (creature.IsTired() || creature.PositionDanger(creature.x, creature.y) > 0);
        }

        public override string ToString()
        {
            return "GoToSafePlaceTransition";
        }

    }
}
