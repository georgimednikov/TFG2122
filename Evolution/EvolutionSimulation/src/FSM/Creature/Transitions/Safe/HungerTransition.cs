namespace EvolutionSimulation.FSM.Creature.Transitions
{
    /// <summary>
    /// Checks if a creature need to eat
    /// </summary>
    class HungerTransition : CreatureTransition
    {

        public HungerTransition(Entities.Creature creature)
        {
            this.creature = creature;
        }

        public override bool Evaluate()
        {
            return creature.stats.CurrEnergy < creature.stats.hungerThreshold * creature.stats.MaxEnergy;
        }

        public override string ToString()
        {
            return "HungerTransition";
        }

    }
}
