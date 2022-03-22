namespace EvolutionSimulation.FSM.Creature.Transitions
{
    /// <summary>
    /// Checks if the nearestMate (female) doesn't want to mate
    /// or the creature has some needs
    /// GoToMate -> Wander
    /// </summary>
    class StopGoToMateTransition : CreatureTransition
    {
        public StopGoToMateTransition(Entities.Creature creature)
        {
            this.creature = creature;
        }

        public override bool Evaluate()
        {
            return !creature.Mate() || creature.IsHungry() || creature.IsThirsty() || creature.IsTired();
        }

        public override string ToString()
        {
            return "StopGoToMateTransition";
        }

    }
}
