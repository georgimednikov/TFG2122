namespace EvolutionSimulation.FSM.Creature.Transitions
{
    /// <summary>
    /// Checks if the creature has rest enough
    /// Sleeping -> Wander
    /// </summary>
    class WakeTransition : CreatureTransition
    {
        public WakeTransition(Entities.Creature creature)
        {
            this.creature = creature;
        }

        public override bool Evaluate()
        {
            return creature.stats.CurrRest >= creature.stats.MaxRest;
        }

        public override string ToString()
        {
            return "WakeTransition";
        }

    }
}
