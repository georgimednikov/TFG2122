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
            return creature.GetClosestSafePlacePosition() != null &&
                (creature.IsTired() || creature.IsInDangerousPosition());
        }

        public override string ToString()
        {
            return "GoToSafePlaceTransition";
        }

    }
}
