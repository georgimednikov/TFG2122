namespace EvolutionSimulation.FSM.Creature.Transitions
{
    /// <summary>
    /// Checks if the creature has drunk enough 
    /// Drinking -> Wander
    /// </summary>
    class StopDrinkingTransition : CreatureTransition
    {
        public StopDrinkingTransition(Entities.Creature creature)
        {
            this.creature = creature;
        }

        public override bool Evaluate()
        {
            // Stop drinking if the creature's currHydration is at max or is not thristy and 
            // is very hungry or exhausted or hungry and doesn't have an eating objective
            // or is tired and doesn't know where to sleep
            return creature.stats.CurrHydration >= creature.stats.MaxHydration ||
                 (!creature.IsThirsty() &&
                (creature.IsVeryHungry() || creature.IsExhausted() ||
                (creature.IsHungry() && !creature.HasEatingObjective()) ||
                (creature.IsTired() && creature.SafePosition() == null)));
        }

        public override string ToString()
        {
            return "StopDrinkingTransition";
        }
    }
}
