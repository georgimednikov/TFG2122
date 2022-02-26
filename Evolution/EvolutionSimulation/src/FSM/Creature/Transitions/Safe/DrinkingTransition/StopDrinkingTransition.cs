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
            return creature.stats.CurrHydration >= creature.stats.MaxHydration;
        }

        public override string ToString()
        {
            return "StopDrinkingTransition";
        }
    }
}
