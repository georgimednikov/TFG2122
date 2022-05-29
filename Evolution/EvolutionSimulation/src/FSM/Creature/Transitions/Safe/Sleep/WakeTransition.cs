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
            // Awake if the creature's currRest is at max or is not tired and 
            // is very hungry or very thirsty or hungry and doesn't have an eating objective
            // or is thirsty and doesn't know where to drink
            return creature.stats.CurrRest >= creature.stats.MaxRest ||
                (!creature.IsTired() && 
                (creature.IsVeryHungry() || creature.IsVeryThirsty() ||
                (creature.IsHungry() && !creature.HasEatingObjective()) ||
                (creature.IsThirsty() && creature.WaterPosition() == null)));
        }

        public override string ToString()
        {
            return "WakeTransition";
        }

    }
}
