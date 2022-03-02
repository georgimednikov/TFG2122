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
            
            return creature.IsTired() && creature.GetClosestSafePlace() != null;
        }

        public override string ToString()
        {
            return "GoToSafePlaceTransition";
        }

    }
}
