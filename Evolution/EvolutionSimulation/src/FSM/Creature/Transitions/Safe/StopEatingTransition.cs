namespace EvolutionSimulation.FSM.Creature.Transitions
{
    /// <summary>
    /// Checks if the creature has eaten enough
    /// </summary>
    class StopEatingTransition : CreatureTransition
    {
        public StopEatingTransition(Entities.Creature creature)
        {
            this.creature = creature;
        }

        public override bool Evaluate()
        {
            return creature.stats.CurrEnergy >= creature.stats.MaxEnergy;
        }

        public override string ToString()
        {
            return "StopEatingTransition";
        }

    }
}
