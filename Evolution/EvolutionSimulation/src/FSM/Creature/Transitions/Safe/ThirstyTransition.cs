namespace EvolutionSimulation.FSM.Creature.Transitions
{
    /// <summary>
    /// Checks if a creature need to drink
    /// </summary>
    class ThirstyTransition : CreatureTransition
    {
        public ThirstyTransition(Entities.Creature creature)
        {
            this.creature = creature;
        }

        public override bool Evaluate()
        {
            return creature.stats.CurrHydration < creature.stats.thirstyThreshold * creature.stats.MaxHydration;
        }

        public override string ToString()
        {
            return "ThirstyTransition";
        }

    }
}
